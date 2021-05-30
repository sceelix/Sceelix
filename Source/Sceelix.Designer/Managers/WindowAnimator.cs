using System;
using System.ComponentModel;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Managers
{
    public class WindowAnimator
    {
        private IAnimationService _animationService;

        private IUIService _uiService;
        //private UIScreen _uiScreen;


        public WindowAnimator(IAnimationService animationService, IUIService uiService)
        {
            _animationService = animationService;
            _uiService = uiService;
        }


        public void Show(AnimatedWindow window)
        {
            var uiScreen = _uiService.Screens.First();

            bool isClosing = false;

            if (window.LoadingAnimation != null)
            {
                // Start the animation.
                var animationController = _animationService.StartAnimation(window.LoadingAnimation, window);

                // Note: The animation effectively starts when AnimationManager.Update() and Apply() are
                // called. To start the animation immediately we can call UpdateAndApply() manually.
                animationController.UpdateAndApply();
            }

            window.Closing += (object sender, CancelEventArgs eventArgs) =>
            {
                if (!isClosing && window.ClosingAnimation != null)
                {
                    // Start the closing animation.
                    var transitionOutController = _animationService.StartAnimation(window.ClosingAnimation, window);
                    transitionOutController.Completed += delegate { window.Close(); };

                    // Remember that we are closing. (The ClosingAnimation is playing.)
                    isClosing = true;

                    // Cancel the close operation. We want to keep this window opened until the closing
                    // animation is finished.
                    eventArgs.Cancel = true;
                }
            };

            window.Show(uiScreen);
        }


        private void OnClosing(object sender, CancelEventArgs e)
        {
            
        }

        public void CenterToScreen(Window window)
        {
            //window.Properties.Add();
            var _uiScreen = _uiService.Screens.First();

            window.Measure(new Vector2F(Single.PositiveInfinity));

            window.X = _uiScreen.Width / 2f - window.DesiredWidth / 2f;
            window.Y = _uiScreen.Height / 2f - window.DesiredHeight / 2f;
        }
    }
}
