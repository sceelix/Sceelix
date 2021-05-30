using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.GraphicsScreens;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Settings.Types;
using Sceelix.Extensions;

namespace Sceelix.Designer.Renderer3D.Services
{
#if WINDOWS
    [Renderer3DService]
    public class PostProcessingManager : IServiceable
    {
        private RenderTargetControl _renderControl;
        private DeferredGraphicsScreen _deferredGraphicsScreen;
        private ContentManager _contentManager;
        private Renderer3DSettings _settings;
        private IGraphicsService _graphicsDevice;


        public void Initialize(IServiceLocator services)
        {
            _deferredGraphicsScreen = (DeferredGraphicsScreen)services.Get<ICustomGraphicsScreen>();
            _graphicsDevice = services.Get<IGraphicsService>();
            _settings = services.Get<SettingsManager>().Get<Renderer3DSettings>();
            _renderControl = services.Get<RenderTargetControl>();
            _contentManager = services.Get<ContentManager>();

            CreatePostProcessingFilters();
        }

        private void CreatePostProcessingFilters()
        {
            //Adds Screen Space Ambient Occlusion (SSAO) filter
            CreatePostProcessor(_settings.Ssao, new SsaoFilter(_graphicsDevice) {Enabled = _settings.Ssao.Value});
            
            //Add the edge filter post-processor
            CreatePostProcessor(_settings.Edge, new EdgeFilter(_graphicsDevice)
            {
                SilhouetteColor = new Vector4F(0, 0, 0, 1),
                CreaseColor = new Vector4F(0, 0, 0f, 1),
                EdgeWidth = 1f,
                Enabled = _settings.Edge.Value
            });

            //Adds Fast Approximate Anti-Aliasing filter
            CreatePostProcessor(_settings.Fxaa,new FxaaFilter(_graphicsDevice) {ComputeLuminance = false, Enabled = _settings.Fxaa.Value});
            
            //Adds Enhanced Subpixel Morphological Anti-Aliasing filter
            CreatePostProcessor(_settings.Smaa, new SmaaFilter(_graphicsDevice) {Enabled = _settings.Smaa.Value});
            
            //Adds ColorCorrection filter
            var colorCorrectionEffect = _contentManager.Load<Effect>("Effects/ColorCorrection");
            CreatePostProcessor(_settings.ColorCorrection, new EffectPostProcessor(_graphicsDevice, colorCorrectionEffect) { Enabled = _settings.ColorCorrection.Value });

            //Adds Vignette filter
            var vignetteEffect = _contentManager.Load<Effect>("Effects/Vignette");
            var vignettePostProcessor = new EffectPostProcessor(_graphicsDevice, vignetteEffect) {Enabled = _settings.Vignette.Value};
            vignettePostProcessor.EffectBinding.ParameterBindings["Scale"].CastTo<ConstParameterBinding<Vector2>>().Value = new Vector2(2.0f, 2.0f);
            vignettePostProcessor.EffectBinding.ParameterBindings["Power"].CastTo<ConstParameterBinding<float>>().Value = 2;
            CreatePostProcessor(_settings.Vignette, vignettePostProcessor);
            

            var hdrEffect = _contentManager.Load<Effect>("Effects/HDR");
            var hdrPostProcessor = new EffectPostProcessor(_graphicsDevice, hdrEffect) { Enabled = _settings.HDR.Value };
            CreatePostProcessor(_settings.HDR, hdrPostProcessor);
            
            
            //HDR Filter
            //Didn't look too good last time that it was tried, should try again (with some parametrization) in the future
            //CreatePostProcessor(_settings.HDRFilter, new HdrFilter(_graphicsDevice) { Enabled = _settings.HDR.Value });
        }

        private void CreatePostProcessor(BoolApplicationField postprocessorSetting, PostProcessor postProcessor)
        {
            postprocessorSetting.Changed += delegate(ApplicationField<bool> field, bool value, bool newValue)
            {
                postProcessor.Enabled = newValue;
                _renderControl.ShouldRender = true;
            };
            _deferredGraphicsScreen.PostProcessors.Add(postProcessor);
        }
    }
    #endif
}