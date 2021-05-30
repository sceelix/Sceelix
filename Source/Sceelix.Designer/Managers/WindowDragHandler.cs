using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Managers
{
    public class WindowDragHandler
    {
        private readonly MessageManager _messageManager;
        private readonly Form _form;
        
        public WindowDragHandler(GameWindow gameWindow, MessageManager messageManager)
        {
            _messageManager = messageManager;
            if (BuildPlatform.IsWindows)
            {
                _form = (Form)Control.FromHandle(gameWindow.Handle);
                _form.Shown += (sender, args) =>
                {
                    _form.AllowDrop = true;
                    _form.DragEnter += Form1_DragEnter;
                    _form.DragOver += Form1_DragEnter;
                    _form.DragDrop += Form1_DragDrop;
                };
                
            }
        }
        
        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                _form.Focus();

                _messageManager.Publish(new FileDragEnter(e));
            }
        }



        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            _messageManager.Publish(new FileDragDrop((string[])e.Data.GetData(DataFormats.FileDrop)));
        }
    }
}
