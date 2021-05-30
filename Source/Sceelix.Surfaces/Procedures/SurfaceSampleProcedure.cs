using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Spatial;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Extensions;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Performs sampling/extraction of surface layer data, such as height, color 
    /// or normals, storing this information as attributes the actors placed in the same area. 
    /// </summary>
    [Procedure("b91f4cb3-e7e9-40db-8fce-6352b88683f5", Label = "Surface Sample", Category = "Surface")]
    public class SurfaceSampleProcedure : SystemProcedure
    {
        /// <summary>
        /// Surface to be sampled.
        /// </summary>
        /*private readonly SingleInput<SurfaceEntity> _inputSurface = new SingleInput<SurfaceEntity>("Surface");*/

        /// <summary>
        /// The surface(s) to be sampled. <br/>
        /// Setting a <b>Single</b> (circle) input means that the node will be executed once per surface. <br/>
        /// Setting a <b>Collective</b> (square) input means that the node will be executed once for all surfaces. Especially useful when actors are to be placed on the surfaces, but it would be complex to match with the right surface.
        /// </summary>
        private readonly SingleOrCollectiveInputChoiceParameter<SurfaceEntity> _parameterInput = new SingleOrCollectiveInputChoiceParameter<SurfaceEntity>("Inputs", "Single");


        /// <summary>
        /// The actor whose position is to be used for sampling.
        /// </summary>
        private readonly CollectiveInput<IActor> _inputActor = new CollectiveInput<IActor>("Actor");

        /// <summary>
        /// Surface that was sampled.
        /// </summary>
        private readonly Output<SurfaceEntity> _outputSurface = new Output<SurfaceEntity>("Surface");

        /// <summary>
        /// The actor whose position was used for sampling.
        /// </summary>
        private readonly Output<IActor> _outputActor = new Output<IActor>("Actor");


        /// <summary>
        /// The surface layer from which the data should be sampled.
        /// </summary>
        private readonly ListParameter<SurfaceSampleParameter> _parameterSurfaceSample = new ListParameter<SurfaceSampleParameter>("Data");



        public SurfaceSampleProcedure()
        {
            _parameterSurfaceSample.Set("Heights");
        }



        protected override void Run()
        {
            var surfaceEntities = _parameterInput.Read();
            foreach (var surfaceEntity in surfaceEntities)
            {
                List<IActor> actors = _inputActor.Read().ToList();

                BoundingRectangle surfaceRectangle = surfaceEntity.BoundingRectangle;

                foreach (var surfaceSampleParameter in _parameterSurfaceSample.Items)
                    surfaceSampleParameter.Initialize(surfaceEntity);

                foreach (var actor in actors)
                {
                    BoundingRectangle actorRectangle = actor.BoxScope.BoundingRectangle;

                    //if they don't even concern the same area (i.e. their projections don't intersect on the XY plane), just forward the data as it was
                    if (surfaceRectangle.Intersects(actorRectangle))
                    {
                        List<Coordinate> coordinates = new List<Coordinate>();

                        for (float i = actorRectangle.Min.X; i < actorRectangle.Max.X; i += surfaceEntity.CellSize)
                        for (float j = actorRectangle.Min.Y; j < actorRectangle.Max.Y; j += surfaceEntity.CellSize)
                        {
                            var position = new Vector2D(i, j);
                            if (surfaceEntity.Contains(position))
                                coordinates.Add(surfaceEntity.ToCoordinates(position));
                        }

                        //if coordinates were found, grab their layer values
                        if (coordinates.Any())
                            foreach (var surfaceSampleParameter in _parameterSurfaceSample.Items)
                                surfaceSampleParameter.AttributeResult[actor] = surfaceSampleParameter.GetData(surfaceEntity, coordinates);
                        else
                            foreach (var surfaceSampleParameter in _parameterSurfaceSample.Items)
                                surfaceSampleParameter.AttributeResult[actor] = surfaceSampleParameter.GetDefault();
                    }
                    else
                    {
                        foreach (var surfaceSampleParameter in _parameterSurfaceSample.Items)
                            surfaceSampleParameter.AttributeResult[actor] = surfaceSampleParameter.GetDefault();
                    }

                    _outputActor.Write(actor);
                }

                _outputSurface.Write(surfaceEntity);
            }
        }



        #region Abstract Parameter

        public abstract class SurfaceSampleParameter : CompoundParameter
        {
            /// <summary>
            /// Attribute where the sampled data should be stored to.
            /// </summary>
            [Order(100)] protected internal readonly AttributeParameter<object> AttributeResult = new AttributeParameter<object>("Data", AttributeAccess.Write);



            protected SurfaceSampleParameter(string label)
                : base(label)
            {
            }



            public abstract object GetData(SurfaceEntity surfaceEntity, List<Coordinate> coordinates);


            public abstract object GetDefault();



            public virtual void Initialize(SurfaceEntity surfaceEntity)
            {
            }
        }

        #endregion

        #region Heights

        /// <summary>
        /// Samples height information from the surface.
        /// </summary>
        /// <seealso cref="Sceelix.Surfaces.Procedures.SurfaceSampleProcedure.SurfaceSampleParameter" />
        public class HeightsSampleParameter : SurfaceSampleParameter
        {
            /// <summary>
            /// The sampling method. Since there may be several values corresponding to the area of the actor, there are
            /// several ways to keep the sampled data:<br/>
            /// <b>First</b> means that the first value will be chosen.<br/>
            /// <b>Average</b> means that the values will be averaged.<br/>
            /// <b>Last</b> means that the last value will be chosen.<br/>
            /// <b>List</b> means that all the values will be returned as a list.
            /// </summary>
            private readonly ChoiceParameter _parameterSampling = new ChoiceParameter("Sampling", "Average", "First", "Last", "Lowest", "Highest", "Median", "Average", "List");



            protected HeightsSampleParameter()
                : base("Heights")
            {
            }



            public override object GetData(SurfaceEntity surfaceEntity, List<Coordinate> coordinates)
            {
                var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
                if (heightLayer == null)
                    return GetDefault();

                switch (_parameterSampling.Value)
                {
                    case "First":
                        return heightLayer.GetValue(coordinates.First());
                    case "Last":
                        return heightLayer.GetValue(coordinates.Last());
                    case "Lowest":
                        return coordinates.Select(coord => heightLayer.GetValue(coord)).Min();
                    case "Highest":
                        return coordinates.Select(coord => heightLayer.GetValue(coord)).Max();
                    case "Median":
                        var heightsToMedian = coordinates.Select(coord => heightLayer.GetValue(coord)).OrderBy(x => x).ToList();
                        var medianCoordinate = coordinates.Count / 2;
                        return heightsToMedian[medianCoordinate];
                    case "Average":
                        var heightsToAverage = coordinates.Select(coord => heightLayer.GetGenericValue(coord));
                        return heightsToAverage.Average();
                    case "List":
                        return new SceeList(coordinates.Select(coord => heightLayer.GetValue(coord)));
                    default:
                        throw new Exception("Invalid Sampling Choice.");
                }
            }



            public override object GetDefault()
            {
                if (_parameterSampling.Value == "List")
                    return new SceeList();

                return -1;
            }
        }

        #endregion

        #region Normals

        /// <summary>
        /// Samples normal information from the surface.
        /// </summary>
        /// <seealso cref="Sceelix.Surfaces.Procedures.SurfaceSampleProcedure.SurfaceSampleParameter" />
        public class NormalsSampleParameter : SurfaceSampleParameter
        {
            /// <summary>
            /// The sampling method. Since there may be several values corresponding to the area of the actor, there are
            /// several ways to keep the sampled data:<br/>
            /// <b>First</b> means that the first value will be chosen.<br/>
            /// <b>Average</b> means that the values will be averaged.<br/>
            /// <b>Last</b> means that the last value will be chosen.<br/>
            /// <b>List</b> means that all the values will be returned as a list.
            /// </summary>
            private readonly ChoiceParameter _parameterSampling = new ChoiceParameter("Sampling", "Average", "First", "Last", "Average", "List");



            protected NormalsSampleParameter()
                : base("Normals")
            {
            }



            public override object GetData(SurfaceEntity surfaceEntity, List<Coordinate> coordinates)
            {
                var normalLayer = surfaceEntity.GetLayer<NormalLayer>();
                if (normalLayer == null)
                    return GetDefault();

                switch (_parameterSampling.Value)
                {
                    case "First":
                        return normalLayer.GetValue(coordinates.First());
                    case "Last":
                        return normalLayer.GetValue(coordinates.Last());
                    case "Average":
                        var heightsToAverage = coordinates.Select(coord => normalLayer.GetGenericValue(coord));
                        return heightsToAverage.Aggregate((result, next) => result + next).Normalize();
                    case "List":
                        return new SceeList(coordinates.Select(coord => normalLayer.GetValue(coord)));
                    default:
                        throw new Exception("Invalid Sampling Choice.");
                }
            }



            public override object GetDefault()
            {
                if (_parameterSampling.Value == "List")
                    return new SceeList();

                return Vector3D.Zero;
            }
        }

        #endregion

        /*#region Colors

        /// <summary>
        /// Samples color information from the surface.
        /// </summary>
        /// <seealso cref="Sceelix.Surfaces.Procedures.SurfaceSampleProcedure.SurfaceSampleParameter" />
        public class ColorsSampleParameter : SurfaceSampleParameter
        {
            /// <summary>
            /// The sampling method. Since there may be several values corresponding to the area of the actor, there are
            /// several ways to keep the sampled data:<br/>
            /// <b>First</b> means that the first value will be chosen.<br/>
            /// <b>Average</b> means that the values will be averaged.<br/>
            /// <b>Last</b> means that the last value will be chosen.<br/>
            /// <b>List</b> means that all the values will be returned as a list.
            /// </summary>
            private readonly ChoiceParameter _parameterSampling = new ChoiceParameter("Sampling", "Average", "First", "Last", "Average", "List");



            protected ColorsSampleParameter()
                : base("Colors")
            {
            }



            public override object GetDefault()
            {
                if (_parameterSampling.Value == "List")
                    return new SceeList();

                return Color.Transparent;
            }



            public override object GetData(SurfaceEntity surfaceEntity, List<int[]> coordinates)
            {
                var colorLayer = surfaceEntity.GetLayer<ColorLayer>();
                if (colorLayer == null)
                    return GetDefault();

                switch (_parameterSampling.Value)
                {
                    case "First":
                        var firstCoordinates = coordinates.First();
                        return colorLayer.GetValue(firstCoordinates[0], firstCoordinates[1]);
                    case "Last":
                        var lastCoordinates = coordinates.Last();
                        return colorLayer.GetValue(lastCoordinates[0], lastCoordinates[1]);
                    case "Average":
                        var colors = coordinates.Select(coord => colorLayer.GetValue(coord[0], coord[1])).ToList();
                        var red = colors.Select(x => (int) x.R).Average();
                        var green = colors.Select(x => (int) x.G).Average();
                        var blue = colors.Select(x => (int) x.B).Average();
                        var alpha = colors.Select(x => (int) x.A).Average();

                        return new Color((byte) red, (byte) green, (byte) blue, (byte) alpha);
                    case "List":
                        return new SceeList(coordinates.Select(coord => colorLayer.GetValue(coord[0], coord[1])).Cast<Object>());
                    default:
                        throw new Exception("Invalid Sampling Choice.");
                }
            }
        }

        #endregion*/

        #region Colors

        /// <summary>
        /// Samples color information from the surface.
        /// </summary>
        public class ColorsSampleParameter : SurfaceSampleParameter
        {
            /// <summary>
            /// The sampling method. Since there may be several values corresponding to the area of the actor, there are
            /// several ways to keep the sampled data:<br/>
            /// <b>First</b> means that the first value will be chosen.<br/>
            /// <b>Average</b> means that the values will be averaged.<br/>
            /// <b>Last</b> means that the last value will be chosen.<br/>
            /// <b>List</b> means that all the values will be returned as a list.
            /// </summary>
            private readonly ChoiceParameter _parameterSampling = new ChoiceParameter("Sampling", "Average", "First", "Last", "Average", "List");

            private Color[,] _colorArray;



            protected ColorsSampleParameter()
                : base("Colors")
            {
            }



            public override object GetData(SurfaceEntity surfaceEntity, List<Coordinate> coordinates)
            {
                //var colorArray = SurfaceBlendExtensions.ToColorArray(surfaceEntity.Layers.OfType<BlendLayer>(), surfaceEntity.NumColumns, surfaceEntity.NumRows);

                //var blendLayers = surfaceEntity.Layers.OfType<BlendLayer>().ToList();

                //var colorLayer = surfaceEntity.GetLayer<ColorLayer>();
                //if (colorLayer == null)
                //    return GetDefault();

                switch (_parameterSampling.Value)
                {
                    case "First":
                        var firstCoordinates = coordinates.First();

                        return _colorArray[firstCoordinates.X, firstCoordinates.Y];
                    case "Last":
                        var lastCoordinates = coordinates.Last();
                        return _colorArray[lastCoordinates.X, lastCoordinates.Y];
                    case "Average":
                        var colors = coordinates.Select(coord => _colorArray[coord.X, coord.Y]).ToList();

                        var red = colors.Select(x => (int) x.R).Average();
                        var green = colors.Select(x => (int) x.G).Average();
                        var blue = colors.Select(x => (int) x.B).Average();
                        var alpha = colors.Select(x => (int) x.A).Average();

                        return new Color((byte) red, (byte) green, (byte) blue, (byte) alpha);
                    case "List":
                        return new SceeList(coordinates.Select(coord => _colorArray[coord.X, coord.Y]).Cast<object>());
                    default:
                        throw new Exception("Invalid Sampling Choice.");
                }
            }



            public override object GetDefault()
            {
                if (_parameterSampling.Value == "List")
                    return new SceeList();

                return Color.Transparent;
            }



            public override void Initialize(SurfaceEntity surfaceEntity)
            {
                _colorArray = surfaceEntity.Layers.OfType<BlendLayer>().ToColorArray(surfaceEntity.NumColumns, surfaceEntity.NumRows);
            }
        }

        #endregion
    }
}