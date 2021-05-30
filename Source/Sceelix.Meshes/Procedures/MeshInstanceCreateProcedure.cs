using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Creates mesh instances of a given input mesh.
    /// </summary>
    [Procedure("9924b6f2-761d-4cfd-b06e-6ba5b7cf2072", Label = "Mesh Instance Create", Category = "Mesh Instance")]
    public class MeshInstanceCreateProcedure : SystemProcedure
    {
        /// <summary>
        /// Mesh to be turned into a mesh instance.
        /// </summary>
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");

        /// <summary>
        /// The created mesh instance.
        /// </summary>
        private readonly Output<MeshInstanceEntity> _output = new Output<MeshInstanceEntity>("Output");



        protected override void Run()
        {
            _output.Write(new MeshInstanceEntity(_input.Read()));
        }
    }
}