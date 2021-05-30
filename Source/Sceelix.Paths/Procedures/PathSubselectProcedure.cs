using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Parameters;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Helpers;
using Sceelix.Paths.Data;
using Sceelix.Paths.Parameters;

namespace Sceelix.Paths.Procedures
{
    /// <summary>
    /// Selects a subset of edges from a given path
    /// creating new paths.
    /// </summary>
    [Procedure("894f34ff-1d05-454b-b002-4a8fa248bf17", Label = "Path Subselect", Category = "Path")]
    public class PathSubselectProcedure : SystemProcedure
    {
        /// <summary>
        /// The path which to apply the subselection to.
        /// </summary>
        private readonly SingleInput<PathEntity> _input = new SingleInput<PathEntity>("Input");

        /// <summary>
        /// The edge selections to apply.
        /// </summary>
        private readonly ListParameter<PathSelectionParameter> _parameterPathSelection = new ListParameter<PathSelectionParameter>("Selection");



        protected override void Run()
        {
            var originalPathEntity = _input.Read();

            _parameterPathSelection.Items.ForEach(val => val.Edges = new List<PathEdge>());

            foreach (PathEdge edge in originalPathEntity)
            foreach (var pathSelectionParameter in _parameterPathSelection.Items)
                if (pathSelectionParameter.EvaluateEdge(originalPathEntity, edge))
                {
                    pathSelectionParameter.Edges.Add(edge);
                    break;
                }


            foreach (var splitParameter in _parameterPathSelection.Items)
                if (splitParameter.Edges.Any())
                {
                    if (splitParameter.ParameterSeparate.HasValue)
                    {
                        foreach (var edge in splitParameter.Edges) splitParameter.Output.Write(splitParameter.ParameterSeparate.Value.Process(originalPathEntity, edge));
                    }
                    else
                    {
                        //the deepclone is important
                        //otherwise we cannot perform clean properly
                        //because despite we are separating the edges, the vertices are still shared
                        //so they need to be cloned
                        PathEntity derivedPath = (PathEntity) new PathEntity(splitParameter.Edges).DeepClone();
                        derivedPath.AdjustScope(originalPathEntity.BoxScope);
                        derivedPath.CleanConnections();
                        originalPathEntity.Attributes.SetAttributesTo(derivedPath.Attributes);

                        splitParameter.Output.Write(derivedPath);
                    }
                }
        }



        #region Abstract Parameter

        public abstract class PathSelectionParameter : CompoundParameter
        {
            /// <summary>
            /// The path resulting from the edges that matched the respective selection.
            /// </summary>
            internal readonly Output<PathEntity> Output = new Output<PathEntity>("Output");

            /// <summary>
            /// if true, each one of the edges of the path will be placed into a separate path entity.
            /// </summary>
            [Order(100)] internal readonly OptionalListParameter<PathSeparateParameter> ParameterSeparate = new OptionalListParameter<PathSeparateParameter>("Separate");



            protected PathSelectionParameter(string label)
                : base(label)
            {
            }



            public List<PathEdge> Edges
            {
                get;
                set;
            }


            public abstract bool EvaluateEdge(PathEntity pathEntity, PathEdge edge);
        }

        #endregion

        #region Index

        /// <summary>
        /// Selects edges from the path by their index.
        /// </summary>
        public class IndexSelectionParameter : PathSelectionParameter
        {
            /// <summary>
            /// Index of the edge to select.
            /// </summary>
            public IntParameter ParameterIndex = new IntParameter("Index", 0);



            public IndexSelectionParameter()
                : base("Index")
            {
            }



            public override bool EvaluateEdge(PathEntity pathEntity, PathEdge edge)
            {
                return pathEntity.Edges.ToList().IndexOf(edge) == ParameterIndex.Value;
            }
        }

        #endregion

        #region Custom

        /// <summary>
        /// Selects edges by a custom condition.
        /// </summary>
        public class CustomSelectionParameter : PathSelectionParameter
        {
            /// <summary>
            /// Condition to evaluate for each edge. If true, the edge will be included in the resulting path.
            /// Can access the attributes of each edge using the @@attributeName notation.
            /// </summary>
            private readonly BoolParameter _parameterCondition = new BoolParameter("Condition", true) {IsExpression = true, EntityEvaluation = true};



            public CustomSelectionParameter()
                : base("Custom")
            {
            }



            public override bool EvaluateEdge(PathEntity pathEntity, PathEdge edge)
            {
                return _parameterCondition.Get(edge);
            }
        }

        #endregion

        #region All

        /// <summary>
        /// Selects all (remaining) edges.
        /// </summary>
        public class AllSelectionParameter : PathSelectionParameter
        {
            public AllSelectionParameter()
                : base("All")
            {
            }



            public override bool EvaluateEdge(PathEntity pathEntity, PathEdge edge)
            {
                return true;
            }
        }

        #endregion

        #region Direction

        /// <summary>
        /// Selects edges based on the direction of their normals.
        /// </summary>
        public class DirectionSelectionParameter : PathSelectionParameter
        {
            /// <summary>
            /// The direction of the edges.
            /// </summary>
            private readonly SelectListParameter<DirectionSelectionChoiceParameter> _parameterDirectionSelection = new SelectListParameter<DirectionSelectionChoiceParameter>("Vector", "Top");

            /// <summary>
            /// The angle tolerance which will be used for the comparison.
            /// </summary>
            private readonly FloatParameter _parameterAngleTolerance = new FloatParameter("Angle Tolerance", 45);

            /// <summary>
            /// Indicates if the direction to compare to will be relative to the scope or the world.
            /// </summary>
            private readonly ChoiceParameter _parameterRelativeTo = new ChoiceParameter("Relative To", "Scope", "Scope", "World");



            public DirectionSelectionParameter()
                : base("Direction")
            {
            }



            public override bool EvaluateEdge(PathEntity pathEntity, PathEdge edge)
            {
                var normalVector = _parameterRelativeTo.Value == "Scope" ? pathEntity.BoxScope.ToScopeDirection(edge.Direction) : edge.Direction;

                double degreesLimit = Math.Cos(MathHelper.ToRadians(_parameterAngleTolerance.Value));

                var selection = _parameterDirectionSelection.Items.FirstOrDefault();
                if (selection != null) return selection.Evaluate(normalVector, degreesLimit);

                return false;
            }
        }

        #endregion
    }
}