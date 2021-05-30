using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    public enum SurfaceOrientation
    {
        CornerTopLeft,
        CornerTopRight
    }

    /// <summary>
    /// Adjusts surfaces to entities lying on top of them.
    /// </summary>
    [Procedure("926790d6-ec93-4f53-b87f-0b023d8177fc", Label = "Surface Adjust", Category = "Surface")]
    public class SurfaceAdjustProcedure : SystemProcedure
    {
        /// <summary>
        /// The surface to be adjusted.
        /// </summary>
        //private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Surface");

        /// <summary>
        /// The surface(s) to be adjusted. <br/>
        /// Setting a <b>Single</b> (circle) input means that the node will be executed once per surface. <br/>
        /// Setting a <b>Collective</b> (square) input means that the node will be executed once for all surfaces. Especially useful when actors are to be placed on the surfaces, but it would be complex to match with the right surface.
        /// </summary>
        private readonly SingleOrCollectiveInputChoiceParameter<SurfaceEntity> _parameterInput = new SingleOrCollectiveInputChoiceParameter<SurfaceEntity>("Inputs", "Single");


        /// <summary>
        /// The surface that was adjusted.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Surface");

        /// <summary>
        /// The type of entity that the surface should adjust to.
        /// </summary>
        private readonly SelectListParameter<SurfaceAdjustParameter> _parameterEntity = new SelectListParameter<SurfaceAdjustParameter>("Entity", "Mesh");



        protected override void Run()
        {
            var surfaceEntities = _parameterInput.Read().ToList();

            SurfaceAdjustParameter entityOperation = _parameterEntity.Items.FirstOrDefault();
            if (entityOperation != null)
                entityOperation.Run(surfaceEntities);

            _output.Write(surfaceEntities);
        }
    }
}