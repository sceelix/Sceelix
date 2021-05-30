using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Performs mesh lofting by interpolating the first faces of the input meshes 
    /// into a new mesh.
    /// </summary>
    /// <seealso cref="Sceelix.Core.Procedures.SystemProcedure" />
    [Procedure("8a3878a1-6f1d-4f9f-b8d5-b62555803169", Label = "Mesh Loft", Category = "Mesh")]
    public class MeshLoftProcedure : SystemProcedure
    {
        /// <summary>
        /// The meshes whose first faces are to be interpolated.
        /// </summary>
        private readonly CollectiveInput<MeshEntity> _input = new CollectiveInput<MeshEntity>("Input");

        /// <summary>
        /// The lofted mesh.
        /// </summary>
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// Number of segments generated between each pair of faces.
        /// </summary>
        private readonly IntParameter _parameterSegments = new IntParameter("Segments", 5) {MinValue = 1};

        /// <summary>
        /// Lower curve strength at each connected face. Can access the attributes of each mesh using the @@attributeName notation.
        /// </summary>
        private readonly FloatParameter _parameterLowerStrength = new FloatParameter("Lower Strength", 1) {EntityEvaluation = true};

        /// <summary>
        /// Upper curve strength at each connected face. Can access the attributes of each mesh using the @@attributeName notation.
        /// </summary>
        private readonly FloatParameter _parameterUpperStrength = new FloatParameter("Upper Strength", 1) {EntityEvaluation = true};



        private float[,] GetBezierDerivatives(int smoothSteps)
        {
            float[,] steps = new float[smoothSteps, 4];
            float increment = 1f / (smoothSteps + 1);
            for (int i = 0; i < smoothSteps; i++)
            {
                var t = (i + 1) * increment;

                //P(t) = (1 − 3t^2 + 2t^3) * P(0) +(3t^2 − 2t^3)*P(1) +(t − 2t^2 + t^3)*P'(0) + (-t^2 + t^3)*P'(1)
                steps[i, 0] = -6 * t + 6 * t * t;
                steps[i, 1] = 6 * t - 6 * t * t;
                steps[i, 2] = 1 - 4 * t + 3 * t * t;
                steps[i, 3] = -2 * t + 3 * t * t;
            }

            return steps;
        }



        private float[,] GetBezierSteps(int smoothSteps)
        {
            float[,] steps = new float[smoothSteps, 4];
            float increment = 1f / (smoothSteps + 1);
            for (int i = 0; i < smoothSteps; i++)
            {
                var t = (i + 1) * increment;

                //P(t) = (1 − 3t^2 + 2t^3) * P(0) +(3t^2 − 2t^3)*P(1) +(t − 2t^2 + t^3)*P'(0) + (-t^2 + t^3)*P'(1)
                steps[i, 0] = 1 - 3 * t * t + 2 * t * t * t;
                steps[i, 1] = 3 * t * t - 2 * t * t * t;
                steps[i, 2] = t - 2 * t * t + t * t * t;
                steps[i, 3] = -t * t + t * t * t;
            }

            return steps;
        }



        protected override void Run()
        {
            var meshes = _input.Read().ToList();

            var faces = meshes.Select(x => x.Faces.First()).ToList();
            if (faces.Count < 2)
                throw new Exception("Cannot perform lofting on less than 2 faces.");

            Face firstFace = faces.First();

            CircularList<CircularList<Vertex>> faceVerticesLists = new CircularList<CircularList<Vertex>>(faces.Select(x => new CircularList<Vertex>(x.Vertices)));
            List<Vector3D> faceNormals = faces.Select(x => x.Normal).ToList();
            List<float> lowerStrengths = meshes.Select(x => _parameterLowerStrength.Get(x)).ToList();
            List<float> upperStrengths = meshes.Select(x => _parameterUpperStrength.Get(x)).ToList();
            CircularList<CircularList<Vertex>> completeFaceVerticesLists = new CircularList<CircularList<Vertex>>();


            var steps = GetBezierSteps(_parameterSegments.Value);
            var derivatives = GetBezierDerivatives(_parameterSegments.Value);

            var segmentsNumber = _parameterSegments.Value;
            for (int i = 0; i < faceVerticesLists.Count - 1; i++)
            {
                CircularList<Vertex> currentVertexList = faceVerticesLists[i];
                CircularList<Vertex> nextVertexList = faceVerticesLists[i + 1];

                if (currentVertexList.Count != nextVertexList.Count)
                    throw new Exception("Face vertex count does not match.");

                //first, add the original list
                completeFaceVerticesLists.Add(currentVertexList);

                var currentNormal = faceNormals[i];

                var tan1 = faceNormals[i] * upperStrengths[i];
                var tan2 = faceNormals[i + 1] * lowerStrengths[i + 1];

                //Vector3D previousCentroid = currentVertexList.Select(x => x.Position).Aggregate((x, total) => total + x) / currentVertexList.Count;

                //var previousBezierPosition = currentVertexList[0].Position;

                for (int j = 0; j < segmentsNumber; j++)
                {
                    Vector3D bezierDerivative = new Vector3D();
                    //Vector3D nextBezierPosition = new Vector3D();

                    CircularList<Vertex> intermediateVertexList = new CircularList<Vertex>();
                    for (int k = 0; k < currentVertexList.Count; k++)
                    {
                        var basePosition = currentVertexList[k].Position;
                        var finalPosition = nextVertexList[k].Position;
                        var direction = finalPosition - basePosition;
                        var halfSize = direction.Length / 2f;

                        var bezierPosition = basePosition * steps[j, 0]
                                             + finalPosition * steps[j, 1]
                                             + tan1 * halfSize * steps[j, 2]
                                             + tan2 * halfSize * steps[j, 3];


                        if (k == 0)
                            bezierDerivative = basePosition * derivatives[j, 0]
                                               + finalPosition * derivatives[j, 1]
                                               + tan1 * halfSize * derivatives[j, 2]
                                               + tan2 * halfSize * derivatives[j, 3];


                        //var newPosition = basePosition + direction*(j/(float)segmentsNumber);
                        intermediateVertexList.Add(new Vertex(bezierPosition));
                    }

                    Vector3D centroid = intermediateVertexList.Select(x => x.Position).Aggregate((x, total) => total + x) / intermediateVertexList.Count;

                    var bezierDirection = bezierDerivative.Normalize(); //(currentBezierPosition - previousBezierPosition).Normalize();//(centroid - previousCentroid).Normalize();

                    var angle = bezierDirection.AngleTo(currentNormal);
                    var axis = bezierDirection.Cross(currentNormal);

                    var rotationMatrix = Matrix.CreateTranslation(centroid) * Matrix.CreateAxisAngle(axis, -angle) * Matrix.CreateTranslation(-centroid);

                    foreach (Vertex vertex in intermediateVertexList)
                        vertex.Position = rotationMatrix.Transform(vertex.Position);

                    completeFaceVerticesLists.Add(intermediateVertexList);

                    //previousBezierPosition = currentBezierPosition;
                }
            }

            //don't forget to add the last list, too
            completeFaceVerticesLists.Add(faceVerticesLists.Last());

            //create the meshEntity with the 
            MeshEntity meshEntity = new MeshEntity(faces.First());
            meshEntity.Add(faces.Last());


            for (int i = 0; i < completeFaceVerticesLists.Count - 1; i++)
            {
                var currentFaceVertices = completeFaceVerticesLists[i];
                var nextFaceVertices = completeFaceVerticesLists[i + 1];

                for (int j = 0; j < currentFaceVertices.Count; j++)
                {
                    var newFace = new Face(currentFaceVertices[j], currentFaceVertices[j + 1], nextFaceVertices[j + 1], nextFaceVertices[j]) {Material = firstFace.Material};
                    meshEntity.Add(newFace);
                }
            }


            /*for (int i = 1; i < faces.Count; i++)
            {
                var currentFace = faces[i];

                var previousFaceVertices = new CircularList<Vertex>(firstFace.Vertices);
                var currentFaceVertices = new CircularList<Vertex>(currentFace.Vertices);

                if(previousFaceVertices.Count != currentFaceVertices.Count)
                    throw new Exception("Face vertex count does not match.");

                for (int j = 0; j < previousFaceVertices.Count; j++)
                {
                    var newFace = new Face(previousFaceVertices[j], previousFaceVertices[j + 1], currentFaceVertices[j + 1],currentFaceVertices[j]){Material = firstFace.Material};
                    meshEntity.Add(newFace);
                }

                firstFace = currentFace;
            }*/

            _output.Write(meshEntity);
        }
    }
}