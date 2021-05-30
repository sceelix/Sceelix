using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Decomposes a Mesh into faces, vertices or half-vertices, all without 
    /// destroying the links between the mesh parts.
    /// </summary>
    [Procedure("4c08b94e-0fbf-444a-97c1-7e06e79f67c3", Label = "Mesh Decompose", Category = "Mesh")]
    public class MeshDecomposeProcedure : SystemProcedure
    {
        /// <summary>
        /// Mesh to be decomposed.
        /// </summary>
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Mesh");

        /// <summary>
        /// Mesh subentities (according to the selected "Entity Type" parameter).
        /// </summary>
        private readonly Output<IEntity> _output = new Output<IEntity>("SubEntity");

        /// <summary>
        /// The original mesh.
        /// </summary>
        private readonly Output<MeshEntity> _outputMesh = new Output<MeshEntity>("Mesh");


        /// <summary>
        /// The type of entities into which the mesh should be decomposed.
        /// </summary>
        private readonly ChoiceParameter _parameterSubEntities = new ChoiceParameter("Entity Type", "Faces", "Faces", "Half-Vertices", "Vertices");



        protected override void Run()
        {
            var meshEntity = _input.Read();

            switch (_parameterSubEntities.Value)
            {
                case "Faces":
                    foreach (var face in meshEntity.Faces) _output.Write(face);
                    break;
                case "Half-Vertices":
                    foreach (var face in meshEntity.Faces)
                    foreach (var halfVertex in face.HalfVertices)
                        _output.Write(halfVertex);
                    break;
                case "Vertices":
                    foreach (var vertex in meshEntity.FaceVertices) _output.Write(vertex);
                    break;
            }

            _outputMesh.Write(meshEntity);
        }
    }
}