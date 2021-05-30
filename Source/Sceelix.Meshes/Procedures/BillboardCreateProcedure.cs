using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Creates billboard entities with given characteristics.
    /// </summary>
    [Procedure("f1dfcf18-2508-4669-8e83-e34fc75c11f0", Label = "Billboard Create", Category = "Billboard")]
    public class BillboardCreateProcedure : SystemProcedure
    {
        /// <summary>
        /// Billboard created according to the defined parameters and/or inputs.
        /// </summary>
        private readonly Output<BillboardEntity> _output = new Output<BillboardEntity>("Output");

        /// <summary>
        /// Color tint of the billboard.
        /// </summary>
        private readonly ColorParameter _parameterColor = new ColorParameter("Color", Color.Black);

        /// <summary>
        /// Image to be set as billboard.
        /// </summary>
        private readonly FileParameter _parameterFile = new FileParameter("Image", "", BitmapExtension.SupportedFileExtensions);

        /// <summary>
        /// Size of the billboard in width (X) and height (Y).
        /// </summary>
        private readonly Vector2DParameter _parameterSize = new Vector2DParameter("Size", new Vector2D(1, 1));



        protected override void Run()
        {
            _output.Write(new BillboardEntity(_parameterSize.Value, _parameterColor.Value) {Image = _parameterFile.Value});
        }
    }
}