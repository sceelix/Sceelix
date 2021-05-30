using System;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Spatial;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Performs data combination - addition, subtraction and set - between two overlapping surfaces.
    /// </summary>
    [Procedure("b2ce5c70-77de-4954-a025-6c7f50d1c99e", Label = "Surface Combine", Category = "Surface")]
    public class SurfaceCombineProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (square) input means that the node will be executed once for all surfaces. Useful to combine any collection of surfaces at once. <br/>
        /// Setting a <b>Dual</b> (circles) input means that the node will be executed once for each pair of surfaces. Useful to combine exactly two surfaces at once.
        /// </summary>
        private readonly DualOrCollectiveInputChoiceParameter<SurfaceEntity> _input = new DualOrCollectiveInputChoiceParameter<SurfaceEntity>("Input", "Collective");


        /// <summary>
        /// A new surfaces whose size encompasses the bounding area of all the input surfaces.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");

        /// <summary>
        /// The surface operation to perform:<br/>
        /// <b>Add</b> means that the values from the surface A will be added to the ones from surface B for the overlapping areas.<br/>
        /// <b>Set</b> means that the values from the surface A will be set to the ones from surface B for the overlapping areas.<br/>
        /// <b>Subtract</b> means that the values from the surface B will be subtracted from the ones at surface A (i.e. the result is A - B).<br/>
        /// <b>Average</b> means that the values from the surface A will be averaged with the ones from surface B for the overlapping areas.
        /// </summary>
        private readonly ChoiceParameter _parameterOperation = new ChoiceParameter("Operation", "Add", "Set", "Add", "Subtract", "Multiply", "Average");

        /// <summary>
        /// Determines what the resulting surface should be:<br/>
        /// <b>First</b> means that the result will have the position and dimensions of the first surface and the data operations will apply to this area only.<br/>
        /// <b>Intersection</b> means that the result will have the position and dimensions of the area corresponding to the intersections between all surfaces. The data operations will apply to this area only.<br/>
        /// <b>Union</b> means that the result will have the position and dimensions of the whole bounding area. The data operations will apply to the whole area, meaning that some data may be set to the default (such as zero values).
        /// </summary>
        private readonly ChoiceParameter _parameterResult = new ChoiceParameter("Result", "First", "First", "Intersection", "Union");



        public bool CombineWith(SurfaceLayer firstLayer, SurfaceLayer otherLayer, SurfaceOperation operation)
        {
            if (firstLayer.CanCombineWith(otherLayer))
                return false;

            Func<object, object, object> operationFunc = GetCombineFunction(firstLayer, operation);


            ParallelHelper.For(0, firstLayer.NumColumns, i =>
            {
                for (int j = 0; j < firstLayer.NumRows; j++)
                {
                    var coordinate = new Coordinate(i, j);
                    var worldPosition = firstLayer.Surface.ToWorldPosition(coordinate);

                    if (otherLayer.Surface.Contains(worldPosition))
                    {
                        var layerValue = otherLayer.GetValue(worldPosition);
                        var result = operationFunc(firstLayer.GetValue(new Coordinate(i, j)), layerValue);

                        firstLayer.SetValue(coordinate, result);
                    }
                }
            });

            return true;
        }



        protected Func<object, object, object> GetCombineFunction(SurfaceLayer surfaceLayer, SurfaceOperation operation)
        {
            switch (operation)
            {
                case SurfaceOperation.Add:
                    return surfaceLayer.Add;
                case SurfaceOperation.Multiply:
                    return surfaceLayer.Multiply;
                case SurfaceOperation.Subtract:
                    return (t1, t2) => surfaceLayer.Add(t1, surfaceLayer.MultiplyScalar(t2, -1));
                case SurfaceOperation.Average:
                    return (t1, t2) => surfaceLayer.MultiplyScalar(surfaceLayer.Add(t1, t2), 0.5f);
                default:
                    //throw new ArgumentException("The operation is not supported for the given layer.");
                    return (t1, t2) => t2;
            }
        }



        protected override void Run()
        {
            var groupingSurfaces = _input.Read().ToList();

            //if there is only one surface, just return it and proceed to the next group
            if (groupingSurfaces.Count == 1)
            {
                _output.Write(groupingSurfaces.First());
                return;
            }

            SurfaceEntity superSurfaceEntity = null;
            int numColumns, numRows;

            if (_parameterResult.Value == "First")
            {
                var firstSurface = groupingSurfaces.First();

                superSurfaceEntity = new SurfaceEntity(firstSurface.NumColumns, firstSurface.NumRows, firstSurface.CellSize) {Origin = firstSurface.Origin, Material = (Material) groupingSurfaces.First().Material.DeepClone()};

                numColumns = firstSurface.NumColumns;
                numRows = firstSurface.NumRows;
            }
            else if (_parameterResult.Value == "Intersection")
            {
                var intersection = BoundingBox.Intersection(groupingSurfaces.Select(x => x.BoundingBox));
                if (intersection != null)
                {
                    //the Cell Size will be the same as from the first surface. We could set this as a parameter later.
                    var cellSize = groupingSurfaces.First().CellSize;

                    numColumns = (int) (intersection.Width / cellSize) + 1;
                    numRows = (int) (intersection.Height / cellSize) + 1;


                    //create the new surface that will enclose the information of all the previous ones
                    superSurfaceEntity = new SurfaceEntity(numColumns, numRows, cellSize) {Origin = intersection.Min.ToVector2D(), Material = (Material) groupingSurfaces.First().Material.DeepClone()};
                }
                else
                {
                    //do not return anything
                    return;
                }
            }
            else
            {
                //otherwise, calculate the total bounding box of the surfaces we are merging
                var totalBoundingBox = groupingSurfaces.Select(x => x.BoundingBox).Aggregate((x, result) => result.Union(x));

                //the Cell Size will be the same as from the first surface. We could set this as a parameter later.
                var cellSize = groupingSurfaces.First().CellSize;

                numColumns = (int) (totalBoundingBox.Width / cellSize) + 1;
                numRows = (int) (totalBoundingBox.Length / cellSize) + 1;

                //create the new surface that will enclose the information of all the previous ones
                superSurfaceEntity = new SurfaceEntity(numColumns, numRows, cellSize) {Origin = totalBoundingBox.Min.ToVector2D(), Material = (Material) groupingSurfaces.First().Material.DeepClone()};
            }


            //reverse the list of surfaces, since value prevalence is from the last to the first
            //groupingSurfaces.Reverse();

            foreach (var surface in groupingSurfaces)
            {
                var surfaceOperation = (SurfaceOperation) Enum.Parse(typeof(SurfaceOperation), _parameterOperation.Value);

                foreach (SurfaceLayer layer in surface.Layers)
                {
                    var hasMerged = false;

                    foreach (var superSurfaceLayer in superSurfaceEntity.Layers)
                        //try to merge the existing layer with the new one
                        //if it succeeded, we stop searching for more
                        if (CombineWith(superSurfaceLayer, layer, surfaceOperation))
                        {
                            hasMerged = true;
                            break;
                        }

                    //if we still haven't merged, 
                    //we need to create a brand new one
                    if (!hasMerged)
                    {
                        var newLayer = superSurfaceEntity.AddLayer(layer.CreateEmpty(numColumns, numRows));
                        CombineWith(newLayer, layer, SurfaceOperation.Set);
                    }
                }
            }

            _output.Write(superSurfaceEntity);
        }
    }
}