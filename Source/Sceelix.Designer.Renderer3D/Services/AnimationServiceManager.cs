using System;
using DigitalRune.Animation;
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
    public class AnimationIServiceLocator : IServiceable, IUpdateableElement, IInputHandlerElement
    {
        private AnimationManager _animationService;

        private IInputService _inputService;
        private bool _isAnimationPaused;


        public AnimationIServiceLocator()
        {
            _animationService = new AnimationManager();
        }


        public IAnimationService AnimationService
        {
            get { return _animationService; }
        }


        public void Initialize(IServiceLocator services)
        {
            _inputService = services.Get<IInputService>();
        }

        public void HandleInput(InputContext context)
        {
            if (_inputService.IsPressed(Keys.P, true))
                _isAnimationPaused = !_isAnimationPaused;
        }

        public void Update(TimeSpan deltaTime)
        {
            if (!_isAnimationPaused || _inputService.IsPressed(Keys.T, true))
            {
                // Update physics simulation.
                _animationService.Update(deltaTime);

                _animationService.ApplyAnimations();
            }
        }
    }
}
