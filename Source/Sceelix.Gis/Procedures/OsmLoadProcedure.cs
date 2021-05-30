using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Tags;
using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Gis.Data;
using Sceelix.Gis.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Helpers;
using Sceelix.Paths.Data;
using Sceelix.Points.Data;

namespace Sceelix.Gis.Procedures
{
    /// <summary>
    /// Loads features from OpenStreetMap files into point, path or mesh entities.
    /// </summary>
    [Procedure("213ea21a-e13b-44ce-a4ec-75a048dfa0aa", Label = "Osm Load", Category = "GIS")]
    public class OsmLoadProcedure : SystemProcedure
    {
        /// <summary>
        /// Actors that were loaded. Depending on the file, it could return points, paths, meshes or a combination of all.
        /// </summary>
        private readonly Output<IActor> _output = new Output<IActor>("Output");

        /// <summary>
        /// The OpenStreetMap file to be loaded. OpenStreeMap files can be downloaded from http://www.openstreetmap.org/ by choosing the
        /// "Export" section on the top menu of the site.
        /// </summary>
        private readonly FileParameter _parameterFile = new FileParameter("File", "", ".osm");

        /// <summary>
        /// Indicates if the loaded data should be centered on the origin (0,0,0) or not. Easier for data manipulation and to reduce precision errors.
        /// </summary>
        private readonly BoolParameter _parameterCenterResults = new BoolParameter("Center Results", true);

        /// <summary>
        /// The attribute where tag information associated to each Openstreetmap feature is stored.
        /// </summary>
        private readonly AttributeParameter<SceeList> _attributeTags = new AttributeParameter<SceeList>("Tags", AttributeAccess.Write);

        /// <summary>
        /// The attribute where meta information (id, author, etc.) associated to each Openstreetmap feature is stored.
        /// </summary>
        private readonly AttributeParameter<SceeList> _attributeMeta = new AttributeParameter<SceeList>("Meta", AttributeAccess.Write);



        private List<Vertex> GetFaceVertices(Way way, Dictionary<long, Node3D> nodes)
        {
            Dictionary<long, Vertex> nodeVertices = new Dictionary<long, Vertex>();

            return way.Nodes.Take(way.Nodes.Length - 1).Select(x => GetOrCreateVertex(x, nodes, nodeVertices)).Where(x => x != null).ToList();
        }



        private PathVertex GetOrCreatePathVertex(long id, Dictionary<long, Node3D> nodes, Dictionary<long, PathVertex> nodesPathVertices)
        {
            PathVertex pathvertex;

            //search for the created pathvertex, if it exists
            if (!nodesPathVertices.TryGetValue(id, out pathvertex))
            {
                Node3D node;
                if (!nodes.TryGetValue(id, out node))
                    return null;

                node.Referenced = true;

                pathvertex = new PathVertex(node.Position);

                _attributeTags[pathvertex] = ProcessTags(node.OsmNode);
                _attributeMeta[pathvertex] = ProcessMeta(node.OsmNode);

                nodesPathVertices.Add(id, pathvertex);
            }

            return pathvertex;
        }



        private Vertex GetOrCreateVertex(long id, Dictionary<long, Node3D> nodes, Dictionary<long, Vertex> nodesPathVertices)
        {
            Vertex vertex;

            if (!nodesPathVertices.TryGetValue(id, out vertex))
            {
                Node3D node;
                if (!nodes.TryGetValue(id, out node))
                    return null;

                node.Referenced = true;

                vertex = new Vertex {Position = node.Position};

                _attributeTags[vertex] = ProcessTags(node.OsmNode);
                _attributeMeta[vertex] = ProcessMeta(node.OsmNode);

                nodesPathVertices.Add(id, vertex);
            }

            return vertex;
        }



        private SceeList ProcessMeta(OsmGeo geoFeature)
        {
            var list = new SceeList();

            list.Add("id", geoFeature.Id);
            list.Add("visible", geoFeature.Visible);
            list.Add("version", geoFeature.Version);
            list.Add("changeset", geoFeature.ChangeSetId);
            list.Add("timestamp", geoFeature.TimeStamp);
            list.Add("uid", geoFeature.UserId);
            list.Add("user", geoFeature.UserName);

            return list;
        }



        private SceeList ProcessTags(OsmGeo geoFeature)
        {
            var list = new SceeList();

            if (geoFeature.Tags != null)
                foreach (Tag tag in geoFeature.Tags)
                    list.Add(tag.Key, tag.Value);

            return list;
        }



        protected override void Run()
        {
            //List<PathEdge> edges = new List<PathEdge>();
            Dictionary<long, Node3D> nodes = new Dictionary<long, Node3D>();
            Dictionary<long, Way3D> ways = new Dictionary<long, Way3D>();
            List<IActor> actors = new List<IActor>();


            //using (var fileStream = new FileInfo(_parameterFile.Value).OpenRead())
            using (var stream = Resources.Load<Stream>(_parameterFile.Value))
            {
                var source = new XmlOsmStreamSource(stream);

                foreach (var element in source)
                    if (element.Type == OsmGeoType.Node)
                    {
                        var node = (Node) element;

                        if (node.Id.HasValue) nodes.Add(node.Id.Value, new Node3D(node));
                    }
                    else if (element.Type == OsmGeoType.Way)
                    {
                        var way = (Way) element;

                        if (way.Id.HasValue) ways.Add(way.Id.Value, new Way3D(way));
                    }
                    else if (element.Type == OsmGeoType.Relation)
                    {
                        var relation = (Relation) element;
                        if (relation.Tags["type"] == "multipolygon")
                        {
                            var outerRelationMember = relation.Members.FirstOrDefault(x => x.Role == "outer");
                            if (outerRelationMember != null && ways.ContainsKey(outerRelationMember.Id))
                            {
                                Way3D outerWay3D;

                                if (ways.TryGetValue(outerRelationMember.Id, out outerWay3D))
                                {
                                    var outerWay = outerWay3D.OsmWay;

                                    outerWay3D.Referenced = true;

                                    var faceVertices = GetFaceVertices(outerWay, nodes);
                                    if (faceVertices.Count > 2)
                                    {
                                        var newFace = new Face(faceVertices);

                                        if (newFace.Normal.Dot(Vector3D.ZVector) < 0)
                                            newFace.Flip();

                                        var newMesh = new MeshEntity(newFace);

                                        foreach (var innerRelationMember in relation.Members.Where(x => x.Role == "inner"))
                                        {
                                            Way3D innerWay3D;

                                            if (ways.TryGetValue(innerRelationMember.Id, out innerWay3D))
                                            {
                                                var innerWay = innerWay3D.OsmWay;

                                                innerWay3D.Referenced = true;

                                                var innerFaceVertices = GetFaceVertices(innerWay, nodes);
                                                innerFaceVertices.Reverse();
                                                if (faceVertices.Count > 2)
                                                    newFace.AddHole(innerFaceVertices);
                                            }
                                        }

                                        _attributeTags[newMesh] = ProcessTags(element);
                                        _attributeMeta[newMesh] = ProcessMeta(element);

                                        actors.Add(newMesh);
                                        _output.Write(newMesh);
                                    }
                                }
                            }
                        }
                    }
            }

            foreach (Way3D way3D in ways.Values.Where(x => !x.Referenced))
            {
                var way = way3D.OsmWay;

                //we have a closed polygon
                if (way.Nodes.First() == way.Nodes.Last())
                {
                    var faceVertices = GetFaceVertices(way, nodes);
                    if (faceVertices.Count > 2)
                    {
                        var newFace = new Face(faceVertices);

                        if (newFace.Normal.Dot(Vector3D.ZVector) < 0)
                            newFace.Flip();

                        var newMesh = new MeshEntity(newFace);

                        _attributeTags[newMesh] = ProcessTags(way);
                        _attributeMeta[newMesh] = ProcessMeta(way);

                        actors.Add(newMesh);
                        _output.Write(newMesh);
                    }
                }
                else
                {
                    Dictionary<long, PathVertex> nodePathVertices = new Dictionary<long, PathVertex>();
                    List<PathEdge> edges = new List<PathEdge>();

                    //otherwise, create a line
                    for (int i = 1; i < way.Nodes.Length; i++)
                    {
                        var v1 = GetOrCreatePathVertex(way.Nodes[i - 1], nodes, nodePathVertices);
                        var v2 = GetOrCreatePathVertex(way.Nodes[i], nodes, nodePathVertices);

                        //in case v1 or v2 are referenced but not existent as nodes,
                        //we need to check
                        if (v1 != null && v2 != null)
                            edges.Add(new PathEdge(v1, v2));
                    }

                    if (edges.Count > 0)
                    {
                        var newPathEntity = new PathEntity(edges);
                        _attributeTags[newPathEntity] = ProcessTags(way);
                        _attributeMeta[newPathEntity] = ProcessMeta(way);

                        actors.Add(newPathEntity);
                        _output.Write(newPathEntity);
                    }
                }
            }

            foreach (Node3D node3D in nodes.Values.Where(x => !x.Referenced))
            {
                var pointEntity = new PointEntity(node3D.Position);

                _attributeTags[pointEntity] = ProcessTags(node3D.OsmNode);
                _attributeMeta[pointEntity] = ProcessMeta(node3D.OsmNode);

                actors.Add(pointEntity);
                _output.Write(pointEntity);
            }

            foreach (IActor actor in actors)
                actor.ResetScope();

            if (_parameterCenterResults.Value)
            {
                var allActors = new ActorGroup(actors);

                allActors.ResetTranslation(TranslationReset.Center, TranslationReset.Center, TranslationReset.Minimum);
            }
        }



        private Vertex ToPointCloudVertex(Node3D node)
        {
            node.Referenced = true;

            var vertex = new Vertex {Position = node.Position};

            _attributeTags[vertex] = ProcessTags(node.OsmNode);
            _attributeMeta[vertex] = ProcessMeta(node.OsmNode);

            return vertex;
        }



        private class Node3D
        {
            public Node3D(Node osmNode)
            {
                OsmNode = osmNode;
                if (!osmNode.Longitude.HasValue || !osmNode.Latitude.HasValue)
                    throw new NotSupportedException("Could not load OSM data because some data has missing or invalid lat/long coordinates.");

                Position = ProjectionHelper.ProjectGeoToMercator(new GeoLocation(osmNode.Latitude.Value, osmNode.Longitude.Value));
            }



            public Node OsmNode
            {
                get;
            }


            public Vector3D Position
            {
                get;
            }


            public bool Referenced
            {
                get;
                set;
            }
        }


        public class Way3D
        {
            public Way3D(Way osmWay)
            {
                OsmWay = osmWay;
            }



            public Way OsmWay
            {
                get;
                set;
            }


            public bool Referenced
            {
                get;
                set;
            }
        }
    }
}