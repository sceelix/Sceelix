using System;
using System.ComponentModel;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.GUI.Windows
{
    public class AnimatedWindow : Window
    {
        //private readonly IAnimationService _animationService;
        //private readonly IServiceLocator _services;

        // True if the window is currently closing (playing the ClosingAnimation).
        //private bool _closing;

        // The animation controller of the ClosingAnimation animation.
        //private AnimationController _transitionOutController;



        public AnimatedWindow()
        {
            // Catch closing event.
            //Closing += OnClosing;


            LoadingAnimation = new SingleFromToByAnimation
            {
                TargetProperty = "Opacity", // Transition the property UIControl.Opacity 
                From = 0, // from 0 to its actual value
                Duration = TimeSpan.FromSeconds(0.3), // over a duration of 0.3 seconds.
            };

            ClosingAnimation = new SingleFromToByAnimation
            {
                TargetProperty = "Opacity", // Transition the property UIControl.Opacity
                To = 0, // from its current value to 0
                Duration = TimeSpan.FromSeconds(0.3), // over a duration 0.3 seconds.
            };

            InputProcessed += OnInputProcessed;
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            
            /*if (LoadingAnimation != null)
            {
                // Start the animation.
                var animationController = _animationService.StartAnimation(LoadingAnimation, this);

                // Note: The animation effectively starts when AnimationManager.Update() and Apply() are
                // called. To start the animation immediately we can call UpdateAndApply() manually.
                animationController.UpdateAndApply();
            }*/

            if (KeepCentered)
                CenterToScreen();

            //keep the style of the tooltip of the close button consistent with the rest
            var closeButton = this.VisualChildren.OfType<Button>().FirstOrDefault(x => x.Name == "CloseButton");
            if(closeButton != null && closeButton.ToolTip is String)
                closeButton.ToolTip = new ToolTipControl((string)closeButton.ToolTip);
        }



        private void OnInputProcessed(object sender, InputEventArgs inputEventArgs)
        {
            if (InputService != null && InputService.IsPressed(Keys.Tab, false))
            {
                var uiControls = this.GetSubtree().Where(x => x.Focusable).ToList();
                if (uiControls.Count > 0)
                {
                    //find out what control is focused
                    var focusedControlIndex = uiControls.IndexOf(x => x.IsFocused);

                    //if it's the last one on the list, pick the first one again
                    if (focusedControlIndex + 1 == uiControls.Count)
                    {
                        Screen.FocusManager.Focus(uiControls[0]);
                    }
                    else if (focusedControlIndex >= 0)
                    {
                        //otherwise, just pick the first one
                        Screen.FocusManager.Focus(uiControls[focusedControlIndex + 1]);
                    }
                }
            }
        }


        


        /// <summary>
        /// When the window is resized, unfortunately the screen object sizes are not update immediately.
        /// Because of that we need this hack to compensate and adjust the loading window location.
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        public void CenterToScreen()
        {
            Measure(new Vector2F(Single.PositiveInfinity));
            
            X = Screen.Width/2f - DesiredWidth/2f;
            Y = Screen.Height/2f - DesiredHeight/2f;
        }


        public void Show(WindowAnimator windowAnimator)
        {
            windowAnimator.Show(this);
        }



        // The animation that is played when the window is loaded.
        public ITimeline LoadingAnimation
        {
            get;
            set;
        }



        // The animation that is played when the window is closing.
        public ITimeline ClosingAnimation
        {
            get;
            set;
        }


        public bool KeepCentered
        {
            get;
            set;
        }
    }
}