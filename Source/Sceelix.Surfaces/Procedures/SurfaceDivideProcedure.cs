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
    /// Divides surface layers into new surfaces according to specific criteria.
    /// </summary>
    [Procedure("f1227b4d-74a1-475a-b766-d90afa67f66e", Label = "Surface Divide", Category = "Surface")]
    public class SurfaceDivideProcedure : SystemProcedure
    {
        /// <summary>
        /// The surface to be divided.
        /// </summary>
        private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");

        /// <summary>
        /// The divided surfaces, according to the defined groups.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");


        /// <summary>
        /// Criteria by which the surface should be divided. If none is indicated, the whole set of layers will be considered.
        /// </summary>
        private readonly ListParameter<SurfaceDivideParameter> _parameterDivideGroups = new ListParameter<SurfaceDivideParameter>("Groups");

        /// <summary>
        /// if true, each one of the layers of the surface will be placed into a separate surface entity.
        /// </summary>
        private readonly OptionalListParameter<SurfaceSeparateParameter> _parameterSeparate = new OptionalListParameter<SurfaceSeparateParameter>("Separate");



        protected override void Run()
        {
            var originalSurfaceEntity = _input.Read();

            List<IEnumerable<SurfaceLayer>> layerGroups = new List<IEnumerable<SurfaceLayer>>();
            layerGroups.Add(originalSurfaceEntity.Layers);

            //each parameter will do its grouping and return enumerables of groups, which will be added to the list of enumerables
            foreach (var surfaceDivideParameter in _parameterDivideGroups.Items)
                layerGroups = layerGroups.SelectMany(faceGroup => surfaceDivideParameter.PerformGroupBy(faceGroup)).ToList();

            foreach (var layerGroup in layerGroups)
                if (_parameterSeparate.HasValue)
                {
                    foreach (var face in layerGroup) _output.Write(_parameterSeparate.Value.Process(originalSurfaceEntity, face));
                }
                else
                {
                    var newSurfaceEntity = new SurfaceEntity(originalSurfaceEntity.NumColumns, originalSurfaceEntity.NumRows, originalSurfaceEntity.CellSize)
                    {
                        Origin = originalSurfaceEntity.Origin,
                        Material = (Material) originalSurfaceEntity.Material.DeepClone()
                    };

                    foreach (var splitParameterLayer in layerGroup)
                        newSurfaceEntity.AddLayer(splitParameterLayer);

                    originalSurfaceEntity.Attributes.SetAttributesTo(newSurfaceEntity.Attributes);

                    _output.Write(newSurfaceEntity);
                }
        }



        #region Abstract Parameter

        public abstract class SurfaceDivideParameter : CompoundParameter
        {
            protected SurfaceDivideParameter(string label)
                : base(label)
            {
            }



            protected internal abstract IEnumerable<IEnumerable<SurfaceLayer>> PerformGroupBy(IEnumerable<SurfaceLayer> layers);
        }

        #endregion

        #region Attribute

        /// <summary>
        /// Divides the layers by attribute value, i.e. building sets of layers that share the same value.
        /// </summary>
        public class SurfaceDivideAttributeSetParameter : SurfaceDivideParameter
        {
            /// <summary>
            /// Value to divide the surfaces by.
            /// </summary>
            private readonly ObjectParameter _parameterValue = new ObjectParameter("Value") {EntityEvaluation = true};



            public SurfaceDivideAttributeSetParameter()
                : base("Attribute")
            {
            }



            protected internal override IEnumerable<IEnumerable<SurfaceLayer>> PerformGroupBy(IEnumerable<SurfaceLayer> layers)
            {
                return layers.GroupBy(x => _parameterValue.Get(x));
            }
        }

        #endregion

        #region Type

        /// <summary>
        /// Divides the surfaces into sets of layers that share the same type.
        /// </summary>
        public class SurfaceDivideTypeSetParameter : SurfaceDivideParameter
        {
            public SurfaceDivideTypeSetParameter()
                : base("Type")
            {
            }



            protected internal override IEnumerable<IEnumerable<SurfaceLayer>> PerformGroupBy(IEnumerable<SurfaceLayer> faces)
            {
                return faces.GroupBy(x => x.GetType());
            }
        }

        #endregion
    }
}