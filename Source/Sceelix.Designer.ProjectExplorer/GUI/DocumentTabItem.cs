using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Console = DigitalRune.Game.UI.Controls.Console;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    public class DocumentTabItem : TabItem
    {
        private readonly DocumentAreaWindow _documentArea;
        private readonly Synchronizer _synchronizer = new Synchronizer();



        public DocumentTabItem(DocumentAreaWindow documentArea, DocumentControl documentControl)
        {
            _documentArea = documentArea;
            Content = new TextBlock {Margin = new Vector4F(4)};
            TabPage = documentControl;

            //if the name changes, change the tab title
            documentControl.NameChanged += delegate(string str) { TextBlock.Text = str; };

            documentControl.ReviewFormName();

            Content.InputProcessing += ContentOnInputProcessing;
        }






        private void ContentOnInputProcessing(object sender, InputEventArgs inputEventArgs)
        {
            if (!InputService.IsMouseOrTouchHandled
                && IsMouseOver)
            {
                if (InputService.IsPressed(MouseButtons.Right, false))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    MultiContextMenu contextMenu = new MultiContextMenu();
                    contextMenu.MenuChildren.Add(new MenuChild(OnCloseItem) {Text = "Close"});
                    contextMenu.MenuChildren.Add(new MenuChild(OnCloseAllItems) {Text = "Close All"});
                    contextMenu.MenuChildren.Add(new MenuChild(OnCloseAllItemsButThis) {Text = "Close All But This"});

                    contextMenu.Open(Screen, InputService.MousePosition);
                }
                else if (InputService.IsPressed(MouseButtons.Middle, false))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    //because of the way digitalrune handles input, we have to postpose this action to the update function
                    _synchronizer.Enqueue(() => _documentArea.CloseTab(this));
                }
            }
        }



        private void OnCloseItem(MenuChild obj)
        {
            _documentArea.CloseTab(this);
        }



        private void OnCloseAllItems(MenuChild obj)
        {
            _documentArea.CloseAllTabs();
        }



        private void OnCloseAllItemsButThis(MenuChild obj)
        {
            _documentArea.CloseAllTabsBut(this);
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();
        }


        public void CheckModificationDate()
        {

        }


        public TextBlock TextBlock
        {
            get { return (TextBlock)Content; }
        }



        public DocumentControl DocumentControl
        {
            get { return (DocumentControl)TabPage; }
        }
    }
}