using System.Linq;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Parameters
{
    /// <summary>
    /// Reads/calculates properties from mesh entities.
    /// </summary>
    /// <seealso cref="Sceelix.Core.Procedures.PropertyProcedure.PropertyParameter" />
    public class MeshPropertyParameter : PropertyProcedure.PropertyParameter
    {
        /// <summary>
        /// Mesh entity from which to read the properties.
        /// </summary>
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");

        /// <summary>
        /// Mesh entity from which the properties were read.
        /// </summary>
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// Area of all faces.
        /// </summary>
        private readonly AttributeParameter<float> _parameterArea = new AttributeParameter<float>("Area", AttributeAccess.Write);

        /// <summary>
        /// Returns the scope axis Sizes.
        /// </summary>
        private readonly AttributeParameter<SceeList> _parameterAxisSizes = new AttributeParameter<SceeList>("Axis Sizes", AttributeAccess.Write);

        /// <summary>
        /// Center of the mesh.
        /// </summary>
        private readonly AttributeParameter<Vector3D> _parameterCentroid = new AttributeParameter<Vector3D>("Centroid", AttributeAccess.Write);

        /// <summary>
        /// Gets a list of all vertex position pairs of all edges.
        /// </summary>
        private readonly AttributeParameter<SceeList> _parameterEdges = new AttributeParameter<SceeList>("Edges", AttributeAccess.Write);

        /// <summary>
        /// Number of faces in the mesh.
        /// </summary>
        private readonly AttributeParameter<int> _parameterFaceCount = new AttributeParameter<int>("Face Count", AttributeAccess.Write);

        /// <summary>
        /// Indicates if all the faces in this mesh are convex.
        /// </summary>
        private readonly AttributeParameter<bool> _parameterIsConvex = new AttributeParameter<bool>("Is Convex?", AttributeAccess.Write);

        /// <summary>
        /// Indicates if all the faces in this mesh are convex.
        /// </summary>
        private readonly AttributeParameter<bool> _parameterIsSelfIntersecting = new AttributeParameter<bool>("Is Self-Intersecting?", AttributeAccess.Write);

        /// <summary>
        /// Perimeter of all the edges of all faces.
        /// </summary>
        private readonly AttributeParameter<float> _parameterPerimeter = new AttributeParameter<float>("Perimeter", AttributeAccess.Write);

        /// <summary>
        /// X-Y Axis Aligned bounding rectangle.
        /// </summary>
        private readonly AttributeParameter<Rectangle> _parameterRectangle = new AttributeParameter<Rectangle>("Bounding Rectangle", AttributeAccess.Write);

        /// <summary>
        /// Number of vertices in the mesh.
        /// </summary>
        private readonly AttributeParameter<int> _parameterVertexCount = new AttributeParameter<int>("Vertex Count", AttributeAccess.Write);



        public MeshPropertyParameter()
            : base("Mesh")
        {
        }



        public override void Run()
        {
            var meshEntity = _input.Read();

            if (_parameterCentroid.IsMapped)
                _parameterCentroid[meshEntity] = meshEntity.Centroid;

            if (_parameterArea.IsMapped)
                _parameterArea[meshEntity] = meshEntity.Area;

            if (_parameterPerimeter.IsMapped)
                _parameterPerimeter[meshEntity] = meshEntity.Perimeter;

            if (_parameterVertexCount.IsMapped)
                _parameterVertexCount[meshEntity] = meshEntity.FaceVertices.Count();

            if (_parameterFaceCount.IsMapped)
                _parameterFaceCount[meshEntity] = meshEntity.Faces.Count;

            if (_parameterRectangle.IsMapped)
                _parameterRectangle[meshEntity] = new Rectangle(meshEntity.BoundingBox);

            if (_parameterIsConvex.IsMapped)
                _parameterIsConvex[meshEntity] = meshEntity.Faces.All(x => x.IsConvex);

            if (_parameterIsSelfIntersecting.IsMapped)
                _parameterIsSelfIntersecting[meshEntity] = meshEntity.Faces.Any(x => x.IsSelfIntersecting());

            if (_parameterAxisSizes.IsMapped)
                _parameterAxisSizes[meshEntity] = ConvertHelper.Convert<SceeList>(meshEntity.BoxScope.Sizes);

            /*new SceeList(new KeyValuePair<string, object>("X", meshEntity.BoxScope.Sizes.X),
                    new KeyValuePair<string, object>("Y", meshEntity.BoxScope.Sizes.Y),
                    new KeyValuePair<string, object>("Z", meshEntity.BoxScope.Sizes.Z));*/

            if (_parameterEdges.IsMapped)
            {
                SceeList sceeList = new SceeList();

                foreach (Face face in meshEntity.Faces)
                foreach (Edge edge in face.Edges)
                {
                    SceeList edgeData = new SceeList();
                    foreach (var vertex in edge.Vertices)
                    {
                        SceeList vertexData = new SceeList();
                        var scopeRelativePosition = meshEntity.BoxScope.ToScopePosition(vertex.Position);

                        vertexData.Add("X", scopeRelativePosition.X);
                        vertexData.Add("Y", scopeRelativePosition.Y);
                        vertexData.Add("Z", scopeRelativePosition.Z);
                        edgeData.Add(vertexData);
                    }

                    sceeList.Add(edgeData);
                }

                _parameterEdges[meshEntity] = sceeList;
            }

            _output.Write(meshEntity);
        }
    }
}