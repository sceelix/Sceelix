using System;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Services;
using Sceelix.Extensions;
using Sceelix.Props.Data;
using Color = Sceelix.Mathematics.Data.Color;

namespace Sceelix.Designer.Props.Components
{
    public class OceanComponent : EntityObjectComponent, IUpdateableElement, IServiceable
    {
        private readonly Ocean _oceanEntity;
        private RenderTargetControl _renderTargetControl;
        private IScene _scene;
        private ContentManager _contentManager;
        private IGraphicsService _graphicsService;


        public OceanComponent(Ocean oceanEntity)
        {
            _oceanEntity = oceanEntity;
        }


        public void Initialize(IServiceLocator services)
        {
            _scene = services.Get<IScene>();
            _contentManager = services.Get<ContentManager>();
            _graphicsService = services.Get<IGraphicsService>();
            _renderTargetControl = services.Get<RenderTargetControl>();
        }


        public override void OnLoad()
        {
            // Define the appearance of the water.
            var waterOcean = new Water
            {
                SpecularColor = new Vector3F(20f),
                SpecularPower = 500,
                NormalMap0 = null,
                NormalMap1 = null,
                RefractionDistortion = 0.1f,
                ReflectionColor = new Vector3F(0.2f),
                RefractionColor = new Vector3F(0.6f),

                // Water is scattered in high waves and this makes the wave crests brighter.
                // ScatterColor defines the intensity of this effect.
                ScatterColor = new Vector3F(0.05f, 0.1f, 0.1f),

                // Foam is automatically rendered where the water intersects geometry and
                // where wave are high.
                FoamMap = _contentManager.Load<Texture2D>("Water/Foam"),
                FoamMapScale = 5,
                FoamColor = new Vector3F(1),
                FoamCrestMin = 0.3f,
                FoamCrestMax = 0.8f,

                // Approximate underwater caustics are computed in real-time from the waves.
                CausticsSampleCount = 3,
                CausticsIntensity = 3,
                CausticsPower = 100,
            };

            // If we do not specify a MeshEntity in the WaterNode constructor, we get an infinite
            // water plane.
            var waterNode = new WaterNode(waterOcean, null)
            {
                PoseWorld = new Pose(new Vector3F(0, (float) _oceanEntity.Attributes["Water Level"], 0)),
                SkyboxReflection = _scene.Children.OfType<SkyboxNode>().FirstOrDefault(),

                // ExtraHeight must be set to a value greater than the max. wave height. 
                ExtraHeight = 2,
            };

            // OceanWaves can be set to displace water surface using a displacement map.
            // The displacement map is computed by the WaterWaveRenderer (see DeferredGraphicsScreen)
            // using FFT and a statistical ocean model.
            waterNode.Waves = new OceanWaves
            {
                TextureSize = 256,
                HeightScale = Math.Max(0, 0.001f*(float) _oceanEntity.Attributes["Wave Scale"]),
                Wind = new Vector3F(10, 0, 10),
                Directionality = 1,
                Choppiness = 1,
                TileSize = 20,

                // If we enable CPU queries, we can call OceanWaves.GetDisplacement()
                // (see Update() method below).
                EnableCpuQueries = true,
            };

            // Optional: Use a planar reflection instead of the skybox reflection.
            // We add a PlanarReflectionNode as a child of the WaterNode.
            var renderToTexture = new RenderToTexture
            {
                Texture = new RenderTarget2D(_graphicsService.GraphicsDevice, 512, 512, false, SurfaceFormat.HdrBlendable, DepthFormat.None),
            };
            var planarReflectionNode = new PlanarReflectionNode(renderToTexture)
            {
                Shape = waterNode.Shape,
                NormalLocal = new Vector3F(0, 1, 0),
                IsEnabled = false,
            };
            waterNode.PlanarReflection = planarReflectionNode;
            waterNode.Children = new SceneNodeCollection(1) {planarReflectionNode};

            waterNode.Water.WaterColor = Vector3F.FromXna(_oceanEntity.Attributes["Water Color"].CastTo<Color>().ToXnaVector())*0.005f;
            
            EntityObjectDomain.SceneNodes.Add(waterNode);

            // To let rigid bodies swim, we add a Buoyancy force effect. This force effect
            // computes buoyancy of a flat water surface.
            /*_simulation.ForceEffects.Add(new Buoyancy
            {
                Surface = new Plane(new Vector3F(0, 1, 0), _waterNode.PoseWorld.Position.Y),
                Density = 1500,
                AngularDrag = 0.3f,
                LinearDrag = 3,
            });*/
        }


        public void Update(TimeSpan timeSpan)
        {
            if (_renderTargetControl != null)
                _renderTargetControl.ShouldRender = true;
        }
    }
}