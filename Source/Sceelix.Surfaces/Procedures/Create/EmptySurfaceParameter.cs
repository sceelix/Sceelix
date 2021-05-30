using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Creates a empty, flat surface.
    /// </summary>
    [Parameter("Empty", Guid = "F2A491F6-1B28-4F21-8F51-9CB578E6C47C")]
    public class EmptySurfaceParameter : SurfaceCreateParameter
    {
        /// <summary>
        /// Width (size in the X-Axis of the terrain.
        /// </summary>
        private readonly IntParameter _parameterWidth = new IntParameter("Width", 100) {MinValue = 1};

        /// <summary>
        /// Length in the Y-Axis of the terrain.
        /// </summary>
        private readonly IntParameter _parameterLength = new IntParameter("Length", 100) {MinValue = 1};

        /// <summary>
        /// Square size of each terrain cell. Should be a multiple of both Width and Length.
        /// </summary>
        private readonly FloatParameter _parameterCellSize = new FloatParameter("Cell Size", 1);

        /// <summary>
        /// The type of height data interpolation. Because surfaces are often rendered as triangle or quad meshes, there is more than one possible setup.
        /// Different target platforms may use different setups, which could affect, for instance, the positioning of the objects on the surface.<br/>
        /// <b>Top Left</b> means that the quads are split from the top left corner to the bottom right corner.<br/>
        /// <b>Top Right</b> means that the quads are split from the top right corner to the bottom left corner.<br/>
        /// <b>Bilinear</b> assumes that the quad is not split and the height values are bilinearly interpolated.
        /// </summary>
        private readonly EnumChoiceParameter<SurfaceInterpolation> _parameterSurfaceInterpolation = new EnumChoiceParameter<SurfaceInterpolation>("Interpolation", SurfaceInterpolation.TopLeft);



        protected EmptySurfaceParameter()
            : base("Empty")
        {
        }



        protected internal override SurfaceEntity Create()
        {
            int columns = (int) (_parameterWidth.Value / _parameterCellSize.Value) + 1;
            int rows = (int) (_parameterLength.Value / _parameterCellSize.Value) + 1;

            var surfaceEntity = new SurfaceEntity(columns, rows, _parameterCellSize.Value);
            surfaceEntity.AddLayer(new HeightLayer(new float[columns, rows]) {Interpolation = _parameterSurfaceInterpolation.Value});

            return surfaceEntity;
        }
    }
}