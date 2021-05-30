using System.Linq;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Paths.Data;

namespace Sceelix.Paths.Parameters
{
    /// <summary>
    /// Reads/calculates properties from path entities.
    /// </summary>
    public class PathPropertyParameter : PropertyProcedure.PropertyParameter
    {
        /// <summary>
        /// Path entity from which to read the properties.
        /// </summary>
        private readonly SingleInput<PathEntity> _input = new SingleInput<PathEntity>("Input");

        /// <summary>
        /// Path entity from which the properties were read.
        /// </summary>
        private readonly Output<PathEntity> _output = new Output<PathEntity>("Output");

        /// <summary>
        /// Number of edges in the path.
        /// </summary>
        private readonly AttributeParameter<int> _parameterEdgeCount = new AttributeParameter<int>("Edge Count", AttributeAccess.Write);

        /// <summary>
        /// Total length (sum of all edge sizes) of the path.
        /// </summary>
        private readonly AttributeParameter<float> _parameterLength = new AttributeParameter<float>("Length", AttributeAccess.Write);

        /// <summary>
        /// Number of vertices in the path.
        /// </summary>
        private readonly AttributeParameter<int> _parameterVertexCount = new AttributeParameter<int>("Vertex Count", AttributeAccess.Write);



        public PathPropertyParameter()
            : base("Path")
        {
        }



        public override void Run()
        {
            PathEntity pathEntity = _input.Read();

            if (_parameterVertexCount.IsMapped)
                _parameterVertexCount[pathEntity] = pathEntity.Vertices.Count();

            if (_parameterEdgeCount.IsMapped)
                _parameterEdgeCount[pathEntity] = pathEntity.Edges.Count();

            if (_parameterLength.IsMapped)
                _parameterLength[pathEntity] = pathEntity.Edges.Sum(x => x.Length);

            _output.Write(pathEntity);
        }
    }
}