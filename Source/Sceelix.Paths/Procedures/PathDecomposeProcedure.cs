using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Paths.Data;

namespace Sceelix.Paths.Procedures
{
    /// <summary>
    /// Decomposes a Path into vertices or edges, all without 
    /// destroying the links between the path parts.
    /// </summary>
    [Procedure("0f139dea-e38c-4b77-ab59-43f6e2c2bbcf", Label = "Path Decompose", Category = "Path")]
    public class PathDecomposeProcedure : SystemProcedure
    {
        /// <summary>
        /// Path to be decomposed.
        /// </summary>
        private readonly SingleInput<PathEntity> _input = new SingleInput<PathEntity>("Input");

        /// <summary>
        /// Path subentities (according to the selected "Entity Type" parameter).
        /// </summary>
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");


        /// <summary>
        /// The original path.
        /// </summary>
        private readonly Output<PathEntity> _outputOriginal = new Output<PathEntity>("Original");

        /// <summary>
        /// The type of entities into which the path should be decomposed.
        /// </summary>
        private readonly ChoiceParameter _parameterSubEntities = new ChoiceParameter("Entity Type", "Vertices", "Edges", "Vertices");



        protected override void Run()
        {
            var pathEntity = _input.Read();

            switch (_parameterSubEntities.Value)
            {
                case "Edges":
                    foreach (var edge in pathEntity.Edges)
                        _output.Write(edge);
                    break;
                case "Vertices":
                    foreach (var vertex in pathEntity.Vertices)
                        _output.Write(vertex);
                    break;
            }

            //output the original too
            _outputOriginal.Write(pathEntity);
        }
    }
}