using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class PanelResized
    {
        private readonly UIControl _control;



        public PanelResized(UIControl control)
        {
            _control = control;
        }



        public UIControl Control
        {
            get { return _control; }
        }
    }
}