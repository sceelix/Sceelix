using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Parameters;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Selects a subset of layers from a given surface
    /// creating new surfaces.
    /// </summary>
    [Procedure("e21e7aed-61ae-435f-a11b-0028251ea898", Label = "Surface Subselect", Category = "Surface")]
    public class SurfaceSubselectProcedure : SystemProcedure
    {
        /// <summary>
        /// The surface which to apply the subselection to.
        /// </summary>
        private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");

        /// <summary>
        /// The surface selections to apply.
        /// </summary>
        private readonly ListParameter<SurfaceSelectionParameter> _parameterSurfaceSelection = new ListParameter<SurfaceSelectionParameter>("Selection");



        protected override void Run()
        {
            var originalSurfaceEntity = _input.Read();

            _parameterSurfaceSelection.Items.ForEach(val => val.Layers = new List<SurfaceLayer>());

            foreach (SurfaceLayer layer in originalSurfaceEntity.Layers)
            foreach (var surfaceSelectionParameter in _parameterSurfaceSelection.Items)
                if (surfaceSelectionParameter.EvaluateLayer(originalSurfaceEntity, layer))
                {
                    surfaceSelectionParameter.Layers.Add(layer);
                    break;
                }


            foreach (var splitParameter in _parameterSurfaceSelection.Items)
                if (splitParameter.Layers.Any())
                {
                    if (splitParameter.ParameterSeparate.HasValue)
                    {
                        foreach (var layer in splitParameter.Layers) splitParameter.Output.Write(splitParameter.ParameterSeparate.Value.Process(originalSurfaceEntity, layer));
                    }
                    else
                    {
                        //create a copy of the old scope and adjust it to the new set of shapes
                        var newSurfaceEntity = new SurfaceEntity(originalSurfaceEntity.NumColumns, originalSurfaceEntity.NumRows, originalSurfaceEntity.CellSize)
                        {
                            Origin = originalSurfaceEntity.Origin,
                            Material = (Material) originalSurfaceEntity.Material.DeepClone()
                        };

                        foreach (var splitParameterLayer in splitParameter.Layers)
                            newSurfaceEntity.AddLayer(splitParameterLayer);

                        originalSurfaceEntity.Attributes.SetAttributesTo(newSurfaceEntity.Attributes);

                        splitParameter.Output.Write(newSurfaceEntity);
                    }
                }
        }



        #region Abstract Parameter

        public abstract class SurfaceSelectionParameter : CompoundParameter
        {
            /// <summary>
            /// The surface resulting from the layers that matched the respective selection.
            /// </summary>
            internal readonly Output<SurfaceEntity> Output = new Output<SurfaceEntity>("Output");

            /// <summary>
            /// if true, each one of the layers of the surface will be placed into a separate surface entity.
            /// </summary>
            [Order(100)] internal readonly OptionalListParameter<SurfaceSeparateParameter> ParameterSeparate = new OptionalListParameter<SurfaceSeparateParameter>("Separate");



            protected SurfaceSelectionParameter(string label)
                : base(label)
            {
            }



            public List<SurfaceLayer> Layers
            {
                get;
                set;
            }


            public abstract bool EvaluateLayer(SurfaceEntity surfaceEntity, SurfaceLayer layer);
        }

        #endregion

        #region Index

        /// <summary>
        /// Selects layers from the surface by their index.
        /// </summary>
        public class IndexSelectionParameter : SurfaceSelectionParameter
        {
            /// <summary>
            /// Index of the layer to select.
            /// </summary>
            public IntParameter ParameterIndex = new IntParameter("Index", 0);



            public IndexSelectionParameter()
                : base("Index")
            {
            }



            public override bool EvaluateLayer(SurfaceEntity surfaceEntity, SurfaceLayer layer)
            {
                return surfaceEntity.Layers.ToList().IndexOf(layer) == ParameterIndex.Value;
            }
        }

        #endregion

        #region Custom

        /// <summary>
        /// Selects layers by a custom condition.
        /// </summary>
        public class CustomSelectionParameter : SurfaceSelectionParameter
        {
            /// <summary>
            /// Condition to evaluate for each layer. If true, the layer will be included in the resulting surface.
            /// Can access the attributes of each layer using the @@attributeName notation.
            /// </summary>
            private readonly BoolParameter _parameterCondition = new BoolParameter("Condition", true) {IsExpression = true, EntityEvaluation = true};



            public CustomSelectionParameter()
                : base("Custom")
            {
            }



            public override bool EvaluateLayer(SurfaceEntity surfaceEntity, SurfaceLayer layer)
            {
                return _parameterCondition.Get(layer);
            }
        }

        #endregion

        #region All

        /// <summary>
        /// Selects all (remaining) layers.
        /// </summary>
        public class AllSelectionParameter : SurfaceSelectionParameter
        {
            public AllSelectionParameter()
                : base("All")
            {
            }



            public override bool EvaluateLayer(SurfaceEntity surfaceEntity, SurfaceLayer layer)
            {
                return true;
            }
        }

        #endregion
    }
}