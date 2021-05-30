using System;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Managers
{
    // Displays the frame rate in FPS (frames per second).
    public class FpsCounter
    {
        private readonly string _originalTitle;
        // This class is a TextBlock.
        // In OnRender the time is measured, and every x seconds the average frame rate is 
        // computed and displayed as the text of this TextBlock.

        private readonly TimeSpan _sampleInterval = new TimeSpan(0, 0, 0, 1);
        private float _numberOfFrames;

        private TimeSpan _sampleTime;
        private readonly DesignerSettings _settings;
        private readonly GameWindow _gameWindow;



        public FpsCounter(GameWindow gameWindow, DesignerSettings settings)
        {
            _gameWindow = gameWindow;
            _originalTitle = _gameWindow.Title;
            _settings = settings;
            _settings.ShowFps.Changed += delegate(ApplicationField<bool> field, bool value, bool newValue)
            {
                //reset the title
                if (!newValue)
                    _gameWindow.Title = _originalTitle;
            };
        }



        public void Draw(GameTime gameTime)
        {

            //run only if the setting is active
            if (!_settings.ShowFps.Value)
            {
                //In MacOS, this is apparently noy updated on initialization, so...
                if(BuildPlatform.IsMacOS)
                    _gameWindow.Title = _originalTitle;

                return;
            }

            _sampleTime += gameTime.ElapsedGameTime;
            _numberOfFrames++;

            if (_sampleTime > _sampleInterval)
            {
                String fps = string.Format(" - FPS: {0}", (int) (_numberOfFrames/(float) _sampleTime.TotalSeconds + 0.5f));
                _gameWindow.Title = _originalTitle + fps;

                _sampleTime = TimeSpan.Zero;
                _numberOfFrames = 0;
            }
        }
    }
}