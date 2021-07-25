using System.Linq;
using Sceelix.Core.Parameters;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Materials;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Sets a multi-layer material, whereas its textures are chosen and interpolated based on the defined splatmap (defined, for instance, through the surface painting node).
    /// </summary>
    /// <seealso cref="SurfaceMaterialParameter" />
    public class MultiTextureSurfaceMaterialParameter : SurfaceMaterialParameter
    {
        /// <summary>
        /// The textures of the material
        /// </summary>
        private readonly ListParameter _parameterTextures = new ListParameter("Textures",
            () => new FileParameter("Texture", "") {ExtensionFilter = BitmapExtension.SupportedFileExtensions, Description = "Texture of the material."});

        /// <summary>
        /// The UV sizing coordinates for texture mapping.
        /// </summary>
        private readonly Vector2DParameter _mappingMultiplierParameter = new Vector2DParameter("UV", Vector2D.One);

        /// <summary>
        /// Indicates if the defined UV coordinates represent an absolute size in world space, or relative to the surface size.
        /// </summary>
        private readonly BoolParameter _absoluteSizingParameter = new BoolParameter("Absolute Sizing", true);



        public MultiTextureSurfaceMaterialParameter()
            : base("Multi Texture")
        {
            _parameterTextures.Add("Texture");
        }



        protected internal override void SetMaterial(SurfaceEntity surfaceEntity)
        {
            surfaceEntity.Material = new MultiTextureSurfaceMaterial
            {
                TextureSetups = _parameterTextures.Items.Select(x => new TextureSetup
                {
                    DiffuseMapPath = ((FileParameter) x).Value,
                    UVTiling = _absoluteSizingParameter.Value ? new Vector2D(surfaceEntity.Width / _mappingMultiplierParameter.Value.X, surfaceEntity.Length / _mappingMultiplierParameter.Value.Y) : _mappingMultiplierParameter.Value
                }).ToArray()
            };

            //if there isn't any blend layer yet, we should initialize the blend layers
            var blendLayers = surfaceEntity.GetLayers<BlendLayer>().ToDictionary(x => x.TextureIndex, x => x);

            for (int i = 0; i < _parameterTextures.Items.Count; i++)
                if (!blendLayers.ContainsKey(i))
                {
                    var surfaceLayer = surfaceEntity.AddLayer(new BlendLayer(new float[surfaceEntity.NumColumns, surfaceEntity.NumRows], i));

                    //if there were at least one blendlayer, the one's we're adding should not affect the result
                    //if there were no blendlayers in the first place, make sure to have the first blendlayer with maximum values
                    surfaceLayer.Fill(blendLayers.Count == 0 && i == 0 ? 1f : 0f);
                }
        }
    }
}