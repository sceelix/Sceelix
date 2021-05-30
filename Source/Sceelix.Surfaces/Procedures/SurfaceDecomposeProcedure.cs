using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Decomposes a Surface into layers, all without 
    /// destroying the links between the surface parts.
    /// </summary>
    [Procedure("089f31b7-3b93-4a46-b2dc-34590321481b", Label = "Surface Decompose", Category = "Surface")]
    public class SurfaceDecomposeProcedure : SystemProcedure
    {
        /// <summary>
        /// Surface to be decomposed.
        /// </summary>
        private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");

        /// <summary>
        /// Surface subentities - the layers.
        /// </summary>
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");


        /// <summary>
        /// The original surface.
        /// </summary>
        private readonly Output<SurfaceEntity> _outputOriginal = new Output<SurfaceEntity>("Original");



        protected override void Run()
        {
            var surfaceEntity = _input.Read();

            foreach (SurfaceLayer surfaceEntityLayer in surfaceEntity.Layers)
                _output.Write(surfaceEntityLayer);

            _outputOriginal.Write(surfaceEntity);
        }
    }
}