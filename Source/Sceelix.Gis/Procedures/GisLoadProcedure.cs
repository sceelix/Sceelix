using System;
using System.Collections.Generic;
using System.Linq;
using DotSpatial.Data;
using DotSpatial.Projections;
using DotSpatial.Topology;
using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Gis.Parameters;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Paths.Data;
using Sceelix.Points.Data;
using Coordinate = DotSpatial.Topology.Coordinate;

namespace Sceelix.Gis.Procedures
{
    /// <summary>
    /// Loads features from Esri Shapefiles into point, path or mesh entities.
    /// </summary>
    [Procedure("a7c10a2b-5554-4210-a0d9-36ad20fc8795", Label = "Gis Load", Category = "GIS")]
    public class GisLoadProcedure : SystemProcedure
    {
        /// <summary>
        /// Actors that were loaded. Depending on the file, it could return points, paths, meshes or a combination of all.
        /// </summary>
        private readonly Output<IActor> _output = new Output<IActor>("Output");

        /// <summary>
        /// Shape file from which to load the Gis data.
        /// </summary>
        private readonly FileParameter _parameterFile = new FileParameter("File", "", ".shp");

        /// <summary>
        /// If checked, the data will be reprojected to the indicated projection.
        /// </summary>
        private readonly OptionalListParameter<ProjectionSelectParameter> _parameterReproject = new OptionalListParameter<ProjectionSelectParameter>("Reproject");

        /// <summary>
        /// Attribute where to store the original coordinate projection.
        /// </summary>
        private readonly AttributeParameter<ProjectionInfo> _attributeProjection = new AttributeParameter<ProjectionInfo>("Projection", AttributeAccess.Write);

        /// <summary>
        /// Attribute where to store the list/row of data associated to each feature.
        /// </summary>
        private readonly AttributeParameter<SceeList> _attributeData = new AttributeParameter<SceeList>("Data", AttributeAccess.Write);



        private IEnumerable<Vector3D> GetPositions(IEnumerable<Coordinate> lineStringCoordinates)
        {
            return lineStringCoordinates.Select(coordinate => new Vector3D(Convert.ToSingle(coordinate.X), Convert.ToSingle(coordinate.Y), Convert.ToSingle(double.IsNaN(coordinate.Z) ? 0 : coordinate.Z)));
        }



        protected override void Run()
        {
            List<IActor> actors = new List<IActor>();

            //Pass in the file path for the standard shapefile that will be opened
            IFeatureSet featureSet = FeatureSet.Open(Resources.GetFullPath(_parameterFile.Value));

            var columnNames = featureSet.DataTable.Columns.Cast<Field>().Select(x => x.ColumnName).ToArray();

            //reproject to the new projection, if requested
            if (_parameterReproject.HasValue)
                featureSet.Reproject(_parameterReproject.Value.Projection);


            //start loading the features and convert each one to a Sceelix feature
            foreach (IFeature feature in featureSet.Features)
            {
                IActor createdActor = null;

                var basicGeometry = feature.BasicGeometry;

                //points become simple pointentities
                //multipoints become groups of points
                //linestrings become simple paths
                //multilinestrings become several sets of edges inside the same path
                //polygons become meshes with one face
                //multipolygons become meshes with several faces
                if (basicGeometry is Point) createdActor = new PointEntity(GetPositions(basicGeometry.Coordinates).First());
                if (basicGeometry is MultiPoint) createdActor = new ActorGroup(GetPositions(basicGeometry.Coordinates).Select(x => new PointEntity(x)));
                if (basicGeometry is LineString)
                {
                    var lineString = (LineString) basicGeometry;
                    createdActor = PathEntity.CreateSequence(GetPositions(lineString.Coordinates).ToList());
                }
                else if (basicGeometry is MultiLineString)
                {
                    var multiLineString = (MultiLineString) basicGeometry;
                    List<PathEdge> pathEdges = new List<PathEdge>();
                    foreach (LineString lineString in multiLineString.Geometries.OfType<LineString>())
                    {
                        var vertexList = GetPositions(lineString.Coordinates).Select(x => new PathVertex(x)).ToList();
                        for (int i = 1; i < vertexList.Count; i++)
                        {
                            PathEdge pathEdge = new PathEdge(vertexList[i - 1], vertexList[i]);
                            pathEdges.Add(pathEdge);
                        }
                    }

                    createdActor = new PathEntity(pathEdges);
                }
                else if (basicGeometry is Polygon)
                {
                    var polygon = (Polygon) basicGeometry;

                    var newFace = new Face(GetPositions(polygon.Coordinates));

                    createdActor = new MeshEntity(newFace);
                }
                else if (basicGeometry is MultiPolygon)
                {
                    var multiPolygon = (MultiPolygon) basicGeometry;
                    List<Face> faces = new List<Face>();
                    foreach (Polygon polygon in multiPolygon.Geometries.OfType<Polygon>()) faces.Add(new Face(GetPositions(polygon.Coordinates)));
                    createdActor = new MeshEntity(faces);
                }


                if (createdActor != null)
                {
                    //add the attributes associated to the geometries
                    _attributeData[createdActor] = new SceeList(columnNames, feature.DataRow.ItemArray);
                    actors.Add(createdActor);
                }
            }

            if (_attributeProjection.IsMapped)
                foreach (IActor actor in actors)
                    _attributeProjection[actor] = featureSet.Projection;

            _output.Write(actors);
        }
    }
}