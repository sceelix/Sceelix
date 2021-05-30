using System;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Extensions;
using Sceelix.Surfaces.Materials;
using ScMaterial = Sceelix.Actors.Data.Material;
using DrMaterial = DigitalRune.Graphics.Material;

namespace Sceelix.Designer.Surfaces.Translators.Materials
{
    [MaterialTranslator(typeof(FurSurfaceMaterial))]
    public class FurTextureSurfaceMaterialTranslator : SurfaceMaterialTranslator<VertexPositionNormalTexture>
    {
        private const float ReferenceAlpha = 0.5f;

        private ContentManager _content;
        private IGraphicsService _graphicsService;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _content = services.Get<ContentManager>();
            _graphicsService = services.Get<IGraphicsService>();
        }


        protected override Func<Coordinate, VertexPositionNormalTexture?> GetPrepareVertexFunc(SurfaceEntity surfaceEntity, ScMaterial sceelixMaterial)
        {
            var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
            var normalsLayer = surfaceEntity.GetLayer<NormalLayer>();

            var furSurfaceMaterial = (FurSurfaceMaterial) sceelixMaterial;

            return (coordinate) =>
            {
                VertexPositionNormalTexture customVertex = new VertexPositionNormalTexture();

                customVertex.Position = (heightLayer != null ? heightLayer.GetPosition(coordinate) : surfaceEntity.CalculateFlatPosition(coordinate)).ToVector3();
                customVertex.Normal = (normalsLayer != null ? normalsLayer.GetGenericValue(coordinate) : heightLayer.CalculateNormal(coordinate)).ToVector3();
                customVertex.TextureCoordinate = surfaceEntity.CalculateUV(coordinate, furSurfaceMaterial.UVTiling, new Vector2D(0, 0), false, true).ToVector2();// * new Vector2D(surfaceEntity.Width, surfaceEntity.Length)

                return customVertex;
            };
        }



        protected override DrMaterial CreateMaterial(ContentLoader contentLoader, ScMaterial furMaterial)
        {
            FurSurfaceMaterial textureMaterial = (FurSurfaceMaterial) furMaterial;
            Texture2D texture = contentLoader.LoadTexture(textureMaterial.Texture);


            DrMaterial material = new DigitalRune.Graphics.Material();

            BasicEffectBinding basicEffectBinding = new BasicEffectBinding(_graphicsService, null) {LightingEnabled = true, TextureEnabled = true, VertexColorEnabled = false,};
            basicEffectBinding.Set("Texture", texture);
            material.Add("Default", basicEffectBinding);

            if (BuildPlatform.IsWindows)
            {
                EffectBinding shadowMapEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/ShadowMap"), null, EffectParameterHint.Material);
                material.Add("ShadowMap", shadowMapEffectBinding);

                // EffectBinding for the "GBuffer" pass.
                EffectBinding gBufferEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/GBuffer"), null, EffectParameterHint.Material);
                gBufferEffectBinding.Set("SpecularPower", 100f);
                material.Add("GBuffer", gBufferEffectBinding);

                // EffectBinding for the "Material" pass.
                EffectBinding materialEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("DigitalRune/Materials/Material"), null, EffectParameterHint.Material);
                materialEffectBinding.Set("DiffuseTexture", texture);
                materialEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1)); //  * 2f
                materialEffectBinding.Set("SpecularColor", new Vector3(0.0f, 0.0f, 0.0f));
                material.Add("Material", materialEffectBinding);


                EffectBinding alphaEffectBinding = new EffectBinding(_graphicsService, _content.Load<Effect>("Materials/Fur_2"), null, EffectParameterHint.Material);
                alphaEffectBinding.Set("MaxFurLength", textureMaterial.MaxFurLength);
                alphaEffectBinding.Set("FurDensity", textureMaterial.FurDensity); //  * 2f
                alphaEffectBinding.Set("SelfShadowStrength", textureMaterial.SelfShadowStrength);
                alphaEffectBinding.Set("JitterMapScale", textureMaterial.JitterMapScale);
                alphaEffectBinding.Set("DiffuseColor", new Vector3(1, 1, 1));
                alphaEffectBinding.Set("SpecularColor", new Vector3(0.0f, 0.0f, 0.0f));
                alphaEffectBinding.Set("DiffuseTexture", texture);
                alphaEffectBinding.Set("Alpha", 1f);

                /*<Parameter Name="FurDensity" Value="0.5" />
                <Parameter Name="SelfShadowStrength" Value="0.7" />
                <Parameter Name="JitterMapScale" Value="0.02" />
                <Parameter Name="DiffuseColor" Value="1,1,1" />
                <Parameter Name="SpecularColor" Value="0,0,0" />
                <Texture Name="DiffuseTexture" File="Fur.png" />
                <Parameter Name="Alpha" Value="0.5" />*/

                material.Add("AlphaBlend", alphaEffectBinding);
            }

            return material;
        }
    }
}