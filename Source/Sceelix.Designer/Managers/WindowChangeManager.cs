using DigitalRune.Game.UI;
using Sceelix.Designer.Layouts;
using Sceelix.Designer.Messaging;

namespace Sceelix.Designer.Managers
{
    public class WindowChangeManager
    {
        private readonly LayoutManager _layoutManager;
        private readonly MainMenuManager _mainMenuManager;
        

        public WindowChangeManager(MessageManager messageManager, LayoutManager layoutManager, MainMenuManager mainMenuManager)
        {
            _layoutManager = layoutManager;
            _mainMenuManager = mainMenuManager;


            messageManager.Register<ClientSizeChanged>(OnClientSizeChanged);
        }



        private void OnClientSizeChanged(ClientSizeChanged obj)
        {
            if (_layoutManager != null && _mainMenuManager != null)
            {
                var actualHeight = _mainMenuManager.BarMenu.ActualHeight;

                RectangleF oldRectangle = new RectangleF(0, actualHeight, obj.OldWidth, obj.OldHeight - actualHeight);
                RectangleF newRectangle = new RectangleF(0, actualHeight, obj.NewWidth, obj.NewHeight - actualHeight);

                _layoutManager.Scale(oldRectangle, newRectangle);

                DesignerProgram.Log.Debug("Resizing " + oldRectangle + " |  " + newRectangle);
            }
        }
    }
}
