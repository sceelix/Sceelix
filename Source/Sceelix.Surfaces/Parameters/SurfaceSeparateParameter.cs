using Sceelix.Actors.Data;
using Sceelix.Core.Parameters;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Parameters
{
    /// <summary>
    /// Separation options.
    /// </summary>
    public class SurfaceSeparateParameter : CompoundParameter
    {
        /// <summary>
        /// Defines what the attributes of the individual surfaces should be:
        /// if they should take the attributes of the parent, use the ones of the layer
        /// or mix both.
        /// </summary>
        private readonly ChoiceParameter _parameterAttributes = new ChoiceParameter("Attributes", "Parent and Layer", "Parent", "Layer", "Parent and Layer");

        /// <summary>
        /// Indicates if the resolution of the resulting surfaceEntity should match the one from the original surface
        /// of should retain the one from the layer.
        /// </summary>
        private readonly ChoiceParameter _parameterResolution = new ChoiceParameter("Resolution", "Layer", "Surface", "Layer");



        public SurfaceSeparateParameter()
            : base("Separate")
        {
        }



        public SurfaceEntity Process(SurfaceEntity parent, SurfaceLayer layer)
        {
            SurfaceEntity surfaceEntity;

            if (_parameterResolution.Value == "Layer")
                surfaceEntity = new SurfaceEntity(layer.NumColumns, layer.NumRows, parent.CellSize)
                {
                    Origin = parent.Origin,
                    Material = (Material) parent.Material.DeepClone()
                };
            else
                surfaceEntity = new SurfaceEntity(parent.NumColumns, parent.NumRows, parent.CellSize)
                {
                    Origin = parent.Origin,
                    Material = (Material) parent.Material.DeepClone()
                };

            surfaceEntity.AddLayer(layer);

            //Now, the attributes
            if (_parameterAttributes.Value == "Parent")
            {
                parent.Attributes.SetAttributesTo(surfaceEntity.Attributes);
            }
            else if (_parameterAttributes.Value == "Layer")
            {
                //promote the attributes of the layer
                layer.Attributes.SetAttributesTo(surfaceEntity.Attributes);
            }
            else if (_parameterAttributes.Value == "Parent and Layer")
            {
                //promote the attributes of the layer
                layer.Attributes.SetAttributesTo(surfaceEntity.Attributes);

                //copy the remaining attributes of the parent
                parent.Attributes.MergeAttributesTo(surfaceEntity.Attributes);
            }

            return surfaceEntity;
        }
    }
}