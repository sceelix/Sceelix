using System;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Particles;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.Services
{
    [Renderer3DService]
    public class ParticleManager : IServiceable, IUpdateableElement, IInputHandlerElement
    {
        private ParticleSystemManager _particleSystemManager;

        private IInputService _inputService;
        private bool _isParticleSimulationPaused;


        public ParticleManager()
        {
            _particleSystemManager = new ParticleSystemManager();
        }


        public ParticleSystemManager ParticleSystemService
        {
            get { return _particleSystemManager; }
        }


        public void Initialize(IServiceLocator services)
        {
            _inputService = services.Get<IInputService>();
        }

        public void Update(TimeSpan deltaTime)
        {
            if (!_isParticleSimulationPaused || _inputService.IsPressed(Keys.T, true))
            {
                // Update physics simulation.
                _particleSystemManager.Update(deltaTime);
            }
        }


        public void HandleInput(InputContext context)
        {
            if (_inputService.IsPressed(Keys.P, true))
                _isParticleSimulationPaused = !_isParticleSimulationPaused;
        }
    }
}
