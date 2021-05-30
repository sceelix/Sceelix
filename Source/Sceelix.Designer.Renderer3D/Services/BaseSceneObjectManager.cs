using DigitalRune.Game;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;

namespace Sceelix.Designer.Renderer3D.Services
{
    [Renderer3DService]
    public class BaseSceneObjectManager : IServiceable
    {
        private Renderer3DSettings _settings;
        private IGameObjectService _gameObjectManager;

        
        private DebugRenderer _debugRenderer;
        private IScene _scene;
        private IServiceLocator _services;
        private RenderTargetControl _render3DControl;

        

        public void Initialize(IServiceLocator services)
        {
            _services = services;

            _settings = services.Get<SettingsManager>().Get<Renderer3DSettings>();
            _gameObjectManager = services.Get<IGameObjectService>();
            _debugRenderer = services.Get<DebugRenderer>();
            _scene = services.Get<IScene>();
            _render3DControl = services.Get<RenderTargetControl>();


            AddObjects();
        }


        private void AddObjects()
        {
            WhiteGroundObject whiteGroundObject = new WhiteGroundObject(_services);

            if (_settings.ShowGround.Value)
                _gameObjectManager.Objects.Add(whiteGroundObject);

            //Add the white ground
            _settings.ShowGround.Changed += delegate (ApplicationField<bool> field, bool value, bool newValue)
            {
                if (newValue)
                    _gameObjectManager.Objects.Add(whiteGroundObject);
                else
                    _gameObjectManager.Objects.Remove(whiteGroundObject);

                _render3DControl.ShouldRender = true;
            };


            //show or hide the reference axis cross
            var axisCross = new AxisCross(_debugRenderer, _scene)
            {
                Enabled = _settings.ShowAxis.Value
            };

            _gameObjectManager.Objects.Add(axisCross);
            _settings.ShowAxis.Changed += delegate (ApplicationField<bool> field, bool value, bool newValue)
            {
                axisCross.Enabled = newValue;
                _render3DControl.ShouldRender = true;
            };
        }
    }
}
