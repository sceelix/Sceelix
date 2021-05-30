using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Extensions;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Adjusts the surface around meshes.
    /// </summary>
    /// <seealso cref="SurfaceAdjustParameter" />
    public class MeshOnSurfaceAdjustParameter : SurfaceAdjustParameter
    {
        private const float MinimumDistanceForBase = 0.01f;


        /// <summary>
        /// Mesh entities around which the surface should be adjusted.
        /// </summary>
        private readonly CollectiveInput<MeshEntity> _inputMesh = new CollectiveInput<MeshEntity>("Mesh");

        /// <summary>
        /// Mesh entities around which the surface was adjusted.
        /// </summary>
        private readonly Output<MeshEntity> _outputMesh = new Output<MeshEntity>("Mesh");

        /// <summary>
        /// Indicates how much below the meshes the surface should be adjusted.
        /// </summary>
        private readonly FloatParameter _parameterHeightOffset = new FloatParameter("Height Offset", 0.1f);

        /// <summary>
        /// Indicates if a base should be created on the meshes. Such bases are horizontal faces that connect the meshes to the surface, so as
        /// to avoid having the meshes flying above the surface.
        /// </summary>
        private readonly BoolParameter _parameterCreateBase = new BoolParameter("Create Base", true);

        /// <summary>
        /// Attribute where to store the type of geometry created. Possible values are "Original" (which correspond
        /// to the placed geometry) and "Base" (which correspond to the side based, if created).
        /// </summary>
        private readonly AttributeParameter<string> _attributeSection = new AttributeParameter<string>("Section", AttributeAccess.Write);


        private readonly SurfaceOrientation _orientation = SurfaceOrientation.CornerTopLeft;



        protected MeshOnSurfaceAdjustParameter()
            : base("Mesh")
        {
        }



        /// <summary>
        /// Algorithm from http://alienryderflex.com/polygon/
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool PointInPolygon(List<Vector2D> polygon, Vector2D point)
        {
            int i, j = polygon.Count - 1;
            bool oddNodes = false;

            for (i = 0; i < polygon.Count; i++)
            {
                if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y
                    || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                        oddNodes = !oddNodes;
                j = i;
            }

            return oddNodes;
        }



        protected internal override void Run(IEnumerable<SurfaceEntity> surfaces)
        {
            var meshes = _inputMesh.Read().ToList();

            foreach (var surface in surfaces)
            {
                //try to get the height layer
                //if it does not exist, create it,
                //otherwise force the layer to have the same size as the surface
                //since we need to set data on it
                var heightLayer = surface.GetLayer<HeightLayer>();
                if (heightLayer == null)
                    surface.AddLayer(heightLayer = new HeightLayer(new float[surface.NumColumns, surface.NumRows]));


                bool[,] edited = new bool[surface.NumColumns, surface.NumRows];

                foreach (MeshEntity meshEntity in meshes)
                foreach (Face face in meshEntity)
                {
                    var zOffset = _parameterHeightOffset.Value;

                    var boundingRectangle = face.BoundingBox.BoundingRectangle;

                    boundingRectangle.Expand(surface.CellSize);

                    //TODO: Check these cases again, because there might be objects on the edge of the surface
                    if (!surface.Contains(boundingRectangle.Min) || !surface.Contains(boundingRectangle.Max))
                        continue;

                    var minCoords = surface.ToCoordinates(boundingRectangle.Min);
                    var maxCoords = surface.ToCoordinates(boundingRectangle.Max);

                    //very simple: project the face into the XY plane
                    var boundary = face.Vertices.Select(x => x.Position.ToVector2D()).ToList();
                    var plane = face.Plane;

                    bool[,] sectionEdited = new bool[maxCoords.X - minCoords.X + 1, minCoords.Y - maxCoords.Y + 1];
                    //HashSet<Coordinate,>


                    for (int i = minCoords.X; i <= maxCoords.X; i++)
                    for (int j = maxCoords.Y; j <= minCoords.Y; j++)
                    {
                        int sectionI = i - minCoords.X;
                        int sectionJ = j - maxCoords.Y;

                        //if we have already edited this element, skip it
                        if (sectionEdited[sectionI, sectionJ])
                            continue;

                        Vector2D gridCornerPosition = surface.ToWorldPosition(new Coordinate(i, j));

                        //if the point is contained in the polygon (i.e. it is below the polygon)
                        if (PointInPolygon(boundary, gridCornerPosition))
                        {
                            //determine the coordinates around this point
                            var coordinates = new List<Coordinate>
                            {
                                new Coordinate(i, j), new Coordinate(i - 1, j), new Coordinate(i + 1, j),
                                new Coordinate(i, j - 1), new Coordinate(i, j + 1)
                            };

                            if (_orientation == SurfaceOrientation.CornerTopLeft)
                            {
                                coordinates.Add(new Coordinate(i + 1, j + 1));
                                coordinates.Add(new Coordinate(i - 1, j - 1));
                            }
                            else
                            {
                                coordinates.Add(new Coordinate(i + 1, j - 1));
                                coordinates.Add(new Coordinate(i - 1, j + 1));
                            }

                            foreach (Coordinate coordinate in coordinates)
                            {
                                if (sectionEdited[coordinate.X - minCoords.X, coordinate.Y - maxCoords.Y])
                                    continue;

                                var coordinatePosition = surface.ToWorldPosition(new Coordinate(coordinate.X, coordinate.Y));

                                //get the expected height at that point
                                var newHeight = plane.GetHeightAt(coordinatePosition);

                                //we will only assign the new height if it has not be set by another face
                                //or (having already been) does only introduce a smaller height
                                if (!edited[coordinate.X, coordinate.Y] || heightLayer.GetGenericValue(coordinate) > newHeight)
                                {
                                    heightLayer.SetValue(coordinate, newHeight - zOffset);
                                    //surface.Colors[i, j] = new Color(0, 255, 0, 0);

                                    edited[coordinate.X, coordinate.Y] = true;
                                }

                                sectionEdited[sectionI, sectionJ] = true;
                            }
                        }
                    }

                    _attributeSection[face] = "Original";
                }


                //after we have made all the changes to the terrain, we may need to connect the bases again
                if (_parameterCreateBase.Value)
                    foreach (MeshEntity meshEntity in meshes)
                    {
                        foreach (Vertex vertex in meshEntity.FaceVerticesWithHoles.Distinct())
                        {
                            var heightAt = heightLayer.GetGenericValue(vertex.Position.ToVector2D());
                            if (float.IsNegativeInfinity(heightAt))
                                heightAt = 0;

                            var distanceToSurface = vertex.Position.Z - heightAt;
                            if (distanceToSurface > MinimumDistanceForBase) vertex.SetLocalAttribute("BottomVertex", Procedure, new Vertex(vertex.Position - new Vector3D(0, 0, distanceToSurface)));
                        }


                        foreach (HalfVertex halfVertex in meshEntity.Faces.SelectMany(x => x.HalfVertices).Where(x => x.IsBoundary).ToList())
                        {
                            var firstVertex = halfVertex.Next;
                            var secondVertex = halfVertex.Vertex;

                            List<Vertex> newFaceVertices = new List<Vertex> {firstVertex, secondVertex};

                            //check if a bottom vertex has been created
                            var firstVertexBottom = halfVertex.Next.GetLocalAttribute<Vertex>("BottomVertex", Procedure);
                            var secondVertexBottom = halfVertex.Vertex.GetLocalAttribute<Vertex>("BottomVertex", Procedure);

                            if (secondVertexBottom != null)
                                newFaceVertices.Add(secondVertexBottom);

                            if (firstVertexBottom != null)
                                newFaceVertices.Add(firstVertexBottom);


                            if (newFaceVertices.Count > 2)
                            {
                                var newFace = new Face(newFaceVertices);
                                _attributeSection[newFace] = "Base";

                                meshEntity.Add(newFace);
                            }
                        }
                    }
            }

            _outputMesh.Write(meshes);
        }
    }
}