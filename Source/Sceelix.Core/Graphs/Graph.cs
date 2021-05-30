using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Core.Annotations;
using Sceelix.Core.Environments;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Extensions;
using ParameterInfo = Sceelix.Core.Parameters.Infos.ParameterInfo;

namespace Sceelix.Core.Graphs
{
    public enum OutputStyle
    {
        Standard,
        Aggregative
    }

    public class Graph
    {
        private Guid _guid = Guid.NewGuid();

        private int _idCounter;
        //private List<AttributeInfo> _variableInfos = new List<AttributeInfo>();

        //ObservableCollection<Node> nodes = new ObservableCollection<Node>();



        public Graph()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }



        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
        }



        /// <summary>
        /// Adds the node to the graph, initializes it and returns it back.
        /// </summary>
        /// <param name="node">The node.</param>
        public Node AddNode(Node node)
        {
            Nodes.Add(node);
            node.Initialize(this, _idCounter++);

            return node;
        }



        public void AnalyseReferences(IProcedureEnvironment loadEnvironment)
        {
            foreach (Node node in Nodes) node.Analyse(loadEnvironment);
        }



        /// <summary>
        /// Removes any aspects that are not references by any of the nodes.
        /// </summary>
        public void CleanupUpParameters()
        {
            foreach (ParameterInfo parameterInfo in ParameterInfos.ToList())
                if (!Nodes.Any(node => node.HasReferenceToParameter(parameterInfo.Identifier)))
                    ParameterInfos.Remove(parameterInfo);

            /*foreach (AttributeInfo variableInfo in _variableInfos.ToList())
            {
                if (!Nodes.Any(node => node.HasReferenceToVariable(variableInfo.Label)))
                    _variableInfos.Remove(variableInfo);
            }*/
        }



        public Graph Clone(IProcedureEnvironment environment)
        {
            string xml = this.GetXML();
            return GraphLoad.LoadFromXML(xml, environment);
        }



        public string GetNameSuggestion<T>(List<T> aspectInfos, string mainSugggestion) where T : ParameterInfo
        {
            int index = 1;
            string modifiedSuggestion = mainSugggestion;
            while (aspectInfos.Any(val => val.Label == modifiedSuggestion)) modifiedSuggestion = mainSugggestion + "0" + index++;

            return modifiedSuggestion;
        }



        public IEnumerable<Assembly> GetReferencedAssemblies(IProcedureEnvironment loadEnvironment)
        {
            HashSet<Assembly> assemblies = new HashSet<Assembly>();

            foreach (Node node in Nodes)
                assemblies.UnionWith(node.GetReferencedAssemblies(loadEnvironment));

            return assemblies;
        }



        public IEnumerable<string> GetReferencedAttributes()
        {
            IEnumerable<ParameterInfo> parameterInfos = Nodes.SelectMany(x => x.Parameters).SelectMany(y => y.GetThisAndSubtree(false));

            foreach (var parameterInfo in parameterInfos)
                if (parameterInfo is AttributeParameterInfo)
                {
                    var attributeParameterInfo = (AttributeParameterInfo) parameterInfo;

                    //attribute must be write or read/write and must not be empty
                    if (attributeParameterInfo.Access != AttributeAccess.Read &&
                        !string.IsNullOrWhiteSpace(attributeParameterInfo.AttributeString))
                    {
                        var attributeName = AttributeParameter.GetAttributeName(attributeParameterInfo.AttributeString);
                        if (!string.IsNullOrWhiteSpace(attributeName))
                            yield return attributeName;
                    }
                }
        }



        public IEnumerable<string> GetReferencedPaths(IProcedureEnvironment environment)
        {
            HashSet<string> relativePaths = new HashSet<string>();

            foreach (Node node in Nodes)
                relativePaths.UnionWith(node.GetReferencedPaths(environment));

            return relativePaths;
        }



        /// <summary>
        /// Gets the node, edge or port (which may belong to a different graph) that is structurally equal in this graph.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Element that is structurally equal to the input, or null if it does not exist.</returns>
        /// <exception cref="ArgumentException">If the element is not a node, edge or port.</exception>
        public object GetStructurallyEqual(object element)
        {
            if (element is Port)
            {
                var port = (Port) element;
                return Nodes.SelectMany(x => x.Ports).FirstOrDefault(x => x.IsStructurallyEqual(port));
            }

            if (element is Edge)
            {
                var edge = element as Edge;
                return Edges.FirstOrDefault(x => x.IsStructurallyEqual(edge));
            }

            if (element is Node)
            {
                var node = (Node) element;
                return Nodes.FirstOrDefault(x => x.IsStructurallyEqual(node));
            }

            throw new ArgumentException("Input element is not a Port, Edge or Node.");
        }



        /// <summary>
        /// Merges a graph into this one.
        /// </summary>
        /// <param name="otherGraph"></param>
        /// <param name="point"></param>
        public void Incorporate(Graph otherGraph, Point point = new Point())
        {
            //incorporate parameters
            IncorporateParameters(otherGraph);

            //incorporate attributes
            //IncorporateAspects(_variableInfos, otherGraph.VariableInfos, otherGraph.RefactorVariableReferences);

            //calculate the minimum point of the othergraph
            Point minimumPoint = new Point(float.MaxValue, float.MaxValue);
            foreach (Node node in otherGraph.Nodes)
                minimumPoint = Point.Minimize(node.Position, minimumPoint);

            foreach (Node node in otherGraph.Nodes)
            {
                AddNode(node);
                node.Position = point + node.Position - minimumPoint;
            }

            foreach (Edge edge in otherGraph.Edges) AddEdge(edge);

            //RefreshNodeIds();
        }



        private void IncorporateParameters(Graph otherGraph)
        {
            //first, we try to merge the parameters, if they do not conflict
            foreach (ParameterInfo aspectInfo in otherGraph.ParameterInfos)
            {
                ParameterInfo existingParameter = ParameterInfos.FirstOrDefault(val => val.Label == aspectInfo.Label);
                if (existingParameter == null)
                {
                    //if no parameter with this name exists, add it
                    ParameterInfos.Add(aspectInfo);
                }
                //if a parameter with this name exists, but has a different type, add this one with a different name
                else if (!existingParameter.StructurallyEqual(aspectInfo))
                {
                    aspectInfo.Label = StringExtension.FindNonConflict(existingParameter.Label + "{0}", ParameterInfos.Select(val => val.Label).Union(otherGraph.ParameterInfos.Select(val => val.Label)).ToList());

                    otherGraph.RefactorParameterReferences(existingParameter.Label, aspectInfo.Label);

                    ParameterInfos.Add(aspectInfo);
                }

                //else, if the parameter has the same name and type, just let the existing one keep its other details
            }
        }



        public string ParameterCheckFunction(string label)
        {
            if (ParameterInfos.Any(val => val.Label == label))
                return "A parameter with that name already exists. Please choose another.";

            return string.Empty;
        }



        public void RefactorParameterReferences(string oldLabel, string newLabel)
        {
            foreach (Node node in Nodes)
                node.RefactorParameterReferences(oldLabel, newLabel);
        }



        /// <summary>
        /// Refactors references to the indicated file. Looks into graph parameters and node parameters.
        /// </summary>
        /// <param name="procedureEnvironment">The procedure environment (used for loading resources).</param>
        /// <param name="originalPath">The original file path.</param>
        /// <param name="replacementPath">The new file path.</param>
        /// <returns>The number of reference that were found.</returns>
        public int RefactorReferencedFile(IProcedureEnvironment loadEnvironment, string originRelativePath, string destinationRelativePath)
        {
            var numReferences = ParameterInfos.Count(x => x.RefactorReferencedFile(loadEnvironment, originRelativePath, destinationRelativePath));
            numReferences += Nodes.SelectMany(y => y.Parameters).Count(y => y.RefactorReferencedFile(loadEnvironment, originRelativePath, destinationRelativePath));

            return numReferences;
        }



        /*public void RefactorReferencedPaths(Environment loadEnvironment, string origin, string destination)
        {
            ParameterInfos.ForEach(x => x.RefactorReferencedFolder(loadEnvironment,origin, destination));

            Nodes.ForEach(x => x.Arguments.ForEach(y => y.RefactorReferencedFolder(loadEnvironment, origin, destination)));

            foreach (ParameterInfo info in _parameterInfos.SelectMany(x => x.GetThisAndSubtree(false)))
            {
                if (info is FolderParameterInfo || info is FileParameterInfo)
                {
                    PrimitiveParameterInfo<String> parameterInfo = info as PrimitiveParameterInfo<String>;

                    if (parameterInfo.FixedValue.Contains(origin))
                        parameterInfo.FixedValue = parameterInfo.FixedValue.Replace(origin, destination);
                }
            }

            Nodes.ForEach(x => x.RefactorReferencedPaths(origin, destination));
        }*/



        /// <summary>
        /// Refactors references to the indicated folder. Looks into graph parameters and node parameters.
        /// </summary>
        /// <param name="procedureEnvironment">The procedure environment (used for loading resources).</param>
        /// <param name="originalPath">The original folder path.</param>
        /// <param name="replacementPath">The new folder path.</param>
        /// <returns>The number of reference that were found.</returns>
        public int RefactorReferencedFolder(IProcedureEnvironment loadEnvironment, string originRelativePath, string destinationRelativePath)
        {
            var numReferences = ParameterInfos.Count(x => x.RefactorReferencedFolder(loadEnvironment, originRelativePath, destinationRelativePath));
            numReferences += Nodes.SelectMany(y => y.Parameters).Count(y => y.RefactorReferencedFolder(loadEnvironment, originRelativePath, destinationRelativePath));

            return numReferences;
        }



        public void RefreshNodeIds()
        {
            for (int i = 0; i < Nodes.Count; i++) Nodes[i].Id = i;
        }



        public void ResetNodeGuids()
        {
            foreach (Node node in Nodes) node.Guid = Guid.NewGuid();
        }



        public string VariableCheckFunction(string label)
        {
            /*if (_variableInfos.Any(val => val.Label == label))
                return "A Attribute with that name already exists. Please choose another.";*/

            return string.Empty;
        }



        #region Properties

        public List<ParameterInfo> ParameterInfos
        {
            get;
            set;
        } = new List<ParameterInfo>();


        /*public List<AttributeInfo> VariableInfos
        {
            get { return _variableInfos; }
            set { _variableInfos = value; }
        }*/


        public string Tags
        {
            get;
            set;
        } = string.Empty;


        public List<Node> Nodes
        {
            get;
            set;
        }


        public List<Edge> Edges
        {
            get;
            set;
        }


        public string Category
        {
            get;
            set;
        } = string.Empty;


        public string Author
        {
            get;
            set;
        } = Environment.UserName;


        public string Description
        {
            get;
            set;
        } = "A standard graph procedure.";


        public string Color
        {
            get;
            set;
        } = "ffffff";


        public ProcedureAttribute ProcedureReference => new ProcedureAttribute(_guid.ToString()) {Author = Author, Description = Description, HexColor = Color};



        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }



        public IEnumerable<Port> GetFellowPorts(Port port)
        {
            if (port is InputPort)
                return Nodes.SelectMany(val => val.InputPorts);

            return Nodes.SelectMany(val => val.OutputPorts);
        }

        #endregion
    }
}