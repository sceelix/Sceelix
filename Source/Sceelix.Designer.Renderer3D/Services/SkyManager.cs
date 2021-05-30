using System;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.Renderer3D.Services
{

    [Renderer3DService]
    public class SkyManager: IServiceable, IUpdateableElement
    {
        
        private Sky3DSettings _skySettings;
        private IGameObjectService _gameObjectManager;
        private IServiceLocator _services;
        private RenderTargetControl _renderTargetControl;
        private IInputService _inputService;
        private GameObject _currentSky;


        public void Initialize(IServiceLocator services)
        {
            _services = services;

            _inputService = _services.Get<IInputService>();
            _gameObjectManager = services.Get<IGameObjectService>();
            _renderTargetControl = _services.Get<RenderTargetControl>();

            if (BuildPlatform.IsWindows)
            {
                _skySettings = services.Get<SettingsManager>().Get<Sky3DSettings>();
                _skySettings.EnableSky.Changed += delegate { ReloadSky(); };
                _skySettings.EnableClouds.Changed += delegate { ReloadSky(); };
                _skySettings.EnableCloudShadows.Changed += delegate { ReloadSky(); };
                _skySettings.CloudSeed.Changed += delegate { ReloadSky(); };
                _skySettings.Hour.Changed += delegate { UpdateSkyTime(); };
                _skySettings.Minutes.Changed += delegate { UpdateSkyTime(); };

                LoadSky();
            }
            else
            {
                _gameObjectManager.Objects.Add(new StaticWhiteSkyObject(_services));
            }
        }


        private void LoadSky()
        {
            _gameObjectManager.Objects.Add(_currentSky  = 
                _skySettings.EnableSky.Value ? 
                (GameObject)new DynamicSkyObject(_services, _skySettings.EnableClouds.Value, _skySettings.EnableCloudShadows.Value, _skySettings.CloudSeed.Value)
                : new StaticWhiteSkyObject(_services));

            UpdateSkyTime();
        }


        private void UpdateSkyTime()
        {
            if (_currentSky is DynamicSkyObject)
                _currentSky.CastTo<DynamicSkyObject>().UpdateTime(_skySettings.Hour.Value, _skySettings.Minutes.Value);
        }



        private void ReloadSky()
        {
            if(_currentSky != null)
                _gameObjectManager.Objects.Remove(_currentSky);

            LoadSky();

            _renderTargetControl.ShouldRender = true;
        }



        public void Update(TimeSpan deltaTime)
        {
            var dynamicSkyObject = _currentSky as DynamicSkyObject;
            if (dynamicSkyObject != null)
            {
                var dateDelta = TimeSpan.FromHours(0.02);

                if (_inputService.IsDown(Keys.PageUp))
                {
                    var newTime = dynamicSkyObject.Time + dateDelta;

                    _skySettings.Hour.Value = newTime.Hour;
                    _skySettings.Minutes.Value = newTime.Minute;
                }
                else if (_inputService.IsDown(Keys.PageDown))
                {
                    var newTime = dynamicSkyObject.Time - dateDelta;

                    _skySettings.Hour.Value = newTime.Hour;
                    _skySettings.Minutes.Value = newTime.Minute;
                }
            }
        }
    }
}