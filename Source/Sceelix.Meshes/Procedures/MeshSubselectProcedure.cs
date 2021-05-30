using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Parameters;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Helpers;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Parameters;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Selects a subset of faces from a given mesh
    /// creating new meshes.
    /// </summary>
    [Procedure("827e1ac6-ff43-4c26-b6d0-4e64b0c26d46", Label = "Mesh Subselect", Category = "Mesh")]
    public class MeshSubselectProcedure : SystemProcedure
    {
        /// <summary>
        /// The mesh which to apply the subselection to.
        /// </summary>
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");

        /// <summary>
        /// The mesh selections to apply.
        /// </summary>
        private readonly ListParameter<MeshSelectionParameter> _parameterMeshSelection = new ListParameter<MeshSelectionParameter>("Mesh Selection");



        protected override void Run()
        {
            var originalMeshEntity = _input.Read();

            _parameterMeshSelection.Items.ForEach(val => val.Faces = new List<Face>());

            foreach (Face face in originalMeshEntity)
            foreach (var meshSelectionParameter in _parameterMeshSelection.Items)
                if (meshSelectionParameter.EvaluateFace(originalMeshEntity, face))
                {
                    meshSelectionParameter.Faces.Add(face);
                    break;
                }


            foreach (var splitParameter in _parameterMeshSelection.Items)
                if (splitParameter.Faces.Any())
                {
                    if (splitParameter.ParameterSeparateMesh.HasValue)
                    {
                        foreach (var face in splitParameter.Faces) splitParameter.Output.Write(splitParameter.ParameterSeparateMesh.Value.Process(originalMeshEntity, face));
                    }
                    else
                    {
                        //the deepclone is important
                        //otherwise we cannot perform clean properly
                        //because despite we are separating the faces, the vertices are still shared
                        //so they need to be cloned
                        MeshEntity derivedMesh = (MeshEntity) originalMeshEntity.CreateDerived(splitParameter.Faces).DeepClone();
                        derivedMesh.CleanFaceConnections();
                        splitParameter.Output.Write(derivedMesh);
                    }
                }
        }



        #region Abstract Parameter

        public abstract class MeshSelectionParameter : CompoundParameter
        {
            /// <summary>
            /// The mesh resulting from the faces that matched the respective selection.
            /// </summary>
            internal readonly Output<MeshEntity> Output = new Output<MeshEntity>("Output");

            /// <summary>
            /// if true, each one of the faces of the mesh will be placed into a separate mesh entity.
            /// </summary>
            [Order(100)] internal readonly OptionalListParameter<MeshSeparateParameter> ParameterSeparateMesh = new OptionalListParameter<MeshSeparateParameter>("Separate");



            protected MeshSelectionParameter(string label)
                : base(label)
            {
            }



            public List<Face> Faces
            {
                get;
                set;
            }


            public abstract bool EvaluateFace(MeshEntity meshEntity, Face face);
        }

        #endregion

        #region Index

        /// <summary>
        /// Selects faces from the mesh by their index.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSubselectProcedure.MeshSelectionParameter" />
        public class IndexSelectionParameter : MeshSelectionParameter
        {
            /// <summary>
            /// Index of the face to select.
            /// </summary>
            public IntParameter ParameterIndex = new IntParameter("Index", 0);



            public IndexSelectionParameter()
                : base("Index")
            {
            }



            public override bool EvaluateFace(MeshEntity meshEntity, Face face)
            {
                return meshEntity.Faces.IndexOf(face) == ParameterIndex.Value;
            }
        }

        #endregion

        #region Custom

        /// <summary>
        /// Selects faces by a custom condition.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSubselectProcedure.MeshSelectionParameter" />
        public class CustomSelectionParameter : MeshSelectionParameter
        {
            /// <summary>
            /// Condition to evaluate for each face. If true, the face will be included in the resulting mesh.
            /// Can access the attributes of each face using the @@attributeName notation.
            /// </summary>
            private readonly BoolParameter _parameterCondition = new BoolParameter("Condition", true) {IsExpression = true, EntityEvaluation = true};



            public CustomSelectionParameter()
                : base("Custom")
            {
            }



            public override bool EvaluateFace(MeshEntity meshEntity, Face face)
            {
                return _parameterCondition.Get(face);
            }
        }

        #endregion

        #region All

        /// <summary>
        /// Selects all (remaining) faces.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSubselectProcedure.MeshSelectionParameter" />
        public class AllSelectionParameter : MeshSelectionParameter
        {
            public AllSelectionParameter()
                : base("All")
            {
            }



            public override bool EvaluateFace(MeshEntity meshEntity, Face face)
            {
                return true;
            }
        }

        #endregion

        #region Direction

        /// <summary>
        /// Selects faces based on the direction of their normals.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSubselectProcedure.MeshSelectionParameter" />
        public class DirectionSelectionParameter : MeshSelectionParameter
        {
            /// <summary>
            /// The direction of the face normals.
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



            public override bool EvaluateFace(MeshEntity meshEntity, Face face)
            {
                var normalVector = _parameterRelativeTo.Value == "Scope" ? meshEntity.BoxScope.ToScopeDirection(face.Normal) : face.Normal;

                double degreesLimit = Math.Cos(MathHelper.ToRadians(_parameterAngleTolerance.Value));

                var selection = _parameterDirectionSelection.Items.FirstOrDefault();
                if (selection != null) return selection.Evaluate(normalVector, degreesLimit);

                return false;
            }
        }

        #endregion
    }
}