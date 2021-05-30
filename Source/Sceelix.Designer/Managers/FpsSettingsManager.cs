using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Managers
{
    public class FpsSettingsManager
    {
        private WindowAnimator _windowAnimator;
        private GraphicsWindowManager _graphicsWindowManager;



        public FpsSettingsManager(DesignerSettings designerSettings, WindowAnimator windowAnimator, GraphicsWindowManager graphicsWindowManager)
        {
            _graphicsWindowManager = graphicsWindowManager;
            _windowAnimator = windowAnimator;

            designerSettings.Use60FpsLimit.Changed += Use60FpsLimitOnChanged;
        }


        private void Use60FpsLimitOnChanged(ApplicationField<bool> field, bool oldValue, bool newValue)
        {
            if (BuildPlatform.IsMacOS)
            {
                MessageWindow window = new MessageWindow();
                window.Title = "Changes need restart";
                window.Text = "Your changes will be applied on your next restart.";
                window.Buttons = new[] { "OK" };
                window.MessageIcon = MessageWindowIcon.Information;
                window.Show(_windowAnimator);
            }
            else
            {
                _graphicsWindowManager.Game.IsFixedTimeStep = newValue;
                _graphicsWindowManager.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = newValue;
                _graphicsWindowManager.GraphicsDeviceManager.ApplyChanges();
            }
        }
    }
}
