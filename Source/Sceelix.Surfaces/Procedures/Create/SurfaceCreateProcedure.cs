using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Creates or loads Surfaces with particular characteristics.
    /// </summary>
    [Procedure("e704d554-70d8-473d-b9ba-4f52782e1546", Label = "Surface Create", Category = "Surface")]
    public class SurfaceCreateProcedure : SystemProcedure
    {
        /// <summary>
        /// Surface created according to the defined parameters and/or inputs.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");

        /// <summary>
        /// Type of surface pattern/method to create.
        /// </summary>
        private readonly SelectListParameter<SurfaceCreateParameter> _parameterCreateSurface = new SelectListParameter<SurfaceCreateParameter>("Type", "Perlin");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterCreateSurface.SubParameterLabels);



        protected override void Run()
        {
            var first = _parameterCreateSurface.Items.FirstOrDefault();
            if (first != null) _output.Write(first.Create());
        }
    }
}