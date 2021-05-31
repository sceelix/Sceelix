using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This sample illustrates how to place actors on surfaces.
    /// Actors are all objects that have a 3D representation that therefore can be found and moved in 3D space.
    /// 
    /// This is a very basic example, and it does not provide a comprehensive solution for placing actors on surfaces. 
    /// It is merely to provide some insight on how to use Actors and Surfaces.
    /// </summary>
    [Procedure("C309DE52-BF8D-44A5-B00A-BCD381792EE3", Label = "Actor On Surface Example")]
    public class ActorOnExampleProcedure : SystemProcedure
    {
        private readonly SingleInput<IActor> _inputActor = new SingleInput<IActor>("Actor");
        private readonly SingleInput<SurfaceEntity> _inputSurface = new SingleInput<SurfaceEntity>("Surface");

        private readonly Output<IActor> _outputActor = new Output<IActor>("Actor");
        private readonly Output<SurfaceEntity> _outputSurface = new Output<SurfaceEntity>("Surface");

        protected override void Run()
        {
            IActor actor = _inputActor.Read();
            SurfaceEntity surface = _inputSurface.Read();

            //for simplicity, we are just going to sample the height corresponding to the center of our actor object.
            Vector2D centroid = actor.BoxScope.Centroid.ToVector2D();

            //get the height layer, if the surface has one
            var heightLayer = surface.GetLayer<HeightLayer>();

            //we confirm if that this actor is on the surface and that we have a height layer
            if (surface.BoundingRectangle.Contains(centroid) && heightLayer != null)
            {
                //we can easily query the height of a surface in a given location
                var absoluteHeightAt = heightLayer.GetValue(centroid);

                //the InsertInto function is implemented by all actors. It receives the "BoxScope" (which indicates position, sizing and rotation of an object)
                //and moves the object there. That's why here we are taking the original position of the actor, placing it at zero height (by subtracting 
                //the current actor.BoxScope.Translation.Z) and adding back the height at the queried point.
                actor.InsertInto(new BoxScope(actor.BoxScope,
                    translation: actor.BoxScope.Translation + new Vector3D(0, 0, -actor.BoxScope.Translation.Z + absoluteHeightAt)));

                //now, we are going to "flatten" all the areas inside the base rectangle of our actor
                var rectangle = actor.BoxScope.BoundingBox.BoundingRectangle;

                //since, in practice, our surface is a discrete bi-dimensional array of values, we only need to lower the individual vertex locations
                //we are equally spaced, being the spacing set in "CellSize"
                var xCount = rectangle.Width/surface.CellSize;
                var yCount = rectangle.Height/surface.CellSize;
                for (int i = 0; i < xCount; i++)
                {
                    for (int j = 0; j < yCount; j++)
                    {
                        //for simplicity, get only the coordinates of the top-left corner of the cell
                        var coordinates = surface.ToCoordinates(rectangle.Min + (new Vector2D(i, j)*surface.CellSize));

                        heightLayer.SetLayerValue(coordinates, absoluteHeightAt); //we set the same height at all coordinates
                    }
                }
            }

            _outputActor.Write(actor);
            _outputSurface.Write(surface);
        }
    }
}
