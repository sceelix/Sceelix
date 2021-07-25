using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    public enum Anchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        MidLeft,
        MidCenter,
        MidRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    
    /// <summary>
    /// Resizes surfaces by removing peripheral areas or by adding new ones according to a selected pattern.
    /// </summary>
    [Procedure("ecd4b1a9-433f-4e94-a713-2de606b472c4", Label = "Surface Crop", Category = "Surface")]
    public class SurfaceCropProcedure : SystemProcedure
    {
        /// <summary>
        /// The surface to be cropped.
        /// </summary>
        private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");

        /// <summary>
        /// The cropped surface.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");

        /// <summary>
        /// The anchor corner/location from which the surface will be resized.
        /// </summary>
        private readonly EnumChoiceParameter<Anchor> _parameterAnchor = new EnumChoiceParameter<Anchor>("Anchor", Anchor.TopLeft);
        
        /// <summary>
        /// The sampling method/pattern to apply when enlarging the surface.
        /// </summary>
        private readonly EnumChoiceParameter<SampleMethod> _parameterMethod = new EnumChoiceParameter<SampleMethod>("Method", SampleMethod.Mirror);

        /// <summary>
        /// The new width of the resulting surface.
        /// </summary>
        private readonly IntParameter _parameterWidth = new IntParameter("Width", 250) {MinValue = 0};
        
        /// <summary>
        /// The new height of the resulting surface.
        /// </summary>
        private readonly IntParameter _parameterHeight = new IntParameter("Height", 250) {MinValue = 0};



        protected override void Run()
        {
            var surfaceEntity = _input.Read();

            Vector2D oldSize = surfaceEntity.BoxScope.Sizes.ToVector2D();
            Vector2D newSize = new Vector2D(_parameterWidth.Value, _parameterHeight.Value);
            Vector2D minCorner = surfaceEntity.Origin;

            switch (_parameterAnchor.Value)
            {
                case Anchor.BottomLeft:
                    minCorner = minCorner + new Vector2D(0, 0) - new Vector2D(0, 0);
                    break;
                case Anchor.BottomCenter:
                    minCorner = minCorner + new Vector2D(oldSize.X / 2, 0) - new Vector2D(newSize.X / 2, 0);
                    break;
                case Anchor.BottomRight:
                    minCorner = minCorner + new Vector2D(oldSize.X, 0) - new Vector2D(newSize.X, 0);
                    break;
                case Anchor.MidLeft:
                    minCorner = minCorner + new Vector2D(0, oldSize.Y / 2) - new Vector2D(0, newSize.Y / 2);
                    break;
                case Anchor.MidCenter:
                    minCorner = minCorner + new Vector2D(oldSize.X / 2, oldSize.Y / 2) - new Vector2D(newSize.X / 2, newSize.Y / 2);
                    break;
                case Anchor.MidRight:
                    minCorner = minCorner + new Vector2D(oldSize.X, oldSize.Y / 2) - new Vector2D(newSize.X, newSize.Y / 2);
                    break;
                case Anchor.TopLeft:
                    minCorner = minCorner + new Vector2D(0, oldSize.Y) - new Vector2D(0, newSize.Y);
                    break;
                case Anchor.TopCenter:
                    minCorner = minCorner + new Vector2D(oldSize.X / 2, oldSize.Y) - new Vector2D(newSize.X / 2, newSize.Y);
                    break;
                case Anchor.TopRight:
                    minCorner = minCorner + new Vector2D(oldSize.X, oldSize.Y) - new Vector2D(newSize.X, newSize.Y);
                    break;
            }

            var numColumns = (int) (newSize.X / surfaceEntity.CellSize) + 1;
            var numRows = (int) (newSize.Y / surfaceEntity.CellSize) + 1;

            var newSurfaceEntity = new SurfaceEntity(numColumns, numRows, surfaceEntity.CellSize);
            newSurfaceEntity.Origin = minCorner;
            newSurfaceEntity.Material = surfaceEntity.Material;
            surfaceEntity.Attributes.SetAttributesTo(newSurfaceEntity.Attributes);

            foreach (var oldSurfaceLayer in surfaceEntity.Layers)
            {
                var newSurfaceLayer = newSurfaceEntity.AddLayer(oldSurfaceLayer.CreateEmpty(numColumns, numRows));

                var layerCopy = oldSurfaceLayer;
                ParallelHelper.For(0, numColumns, i =>
                {
                    for (int j = 0; j < numRows; j++)
                    {
                        var newSurfaceCoordinate = new Coordinate(i, j);
                        var worldPosition = newSurfaceEntity.ToWorldPosition(newSurfaceCoordinate);

                        newSurfaceLayer.SetValue(newSurfaceCoordinate, layerCopy.GetValue(worldPosition, _parameterMethod.Value));
                    }
                });
            }

            _output.Write(newSurfaceEntity);
        }
    }
}