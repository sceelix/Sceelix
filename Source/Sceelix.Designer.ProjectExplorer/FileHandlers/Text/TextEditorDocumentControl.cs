using System;
using System.IO;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;


namespace Sceelix.Designer.ProjectExplorer.FileHandlers.Text
{
    public class TextEditorDocumentControl : DocumentControl
    {
        private string[] _lines;
        private string _text;
        private TextBox _textBox;
        //private ScrollBar _verticalBar;


        protected override void OnFirstLoad()
        {
            _text = File.ReadAllText(FileItem.FullPath);

            _lines = File.ReadAllLines(FileItem.FullPath);

            var windowStackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            BarMenu barMenu = new BarMenu();
            barMenu.MenuChildren.Add(new MenuChild(Save) { Text = "Save", Icon = EmbeddedResources.Load<Texture2D>("Resources/save16x16.png") });
            windowStackPanel.Children.Add(barMenu);


            // ----- Multi-line text box
            windowStackPanel.Children.Add(_textBox = new ExtendedTextBox
            {
                Margin = new Vector4F(4),
                Text = _text,
                MinLines = 5,
                MaxLines = 5,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            });

            _textBox.PropertyChanged += TextBoxOnPropertyChanged;

            Content = windowStackPanel;
        }


        private void Save(MenuChild obj)
        {
            File.WriteAllText(FileItem.FullPath, _textBox.Text);
            AlertFileSave();
        }



        protected override void OnClose(bool shouldSave)
        {
            if (shouldSave)
                File.WriteAllText(FileItem.FullPath, _textBox.Text);
        }



        private void TextBoxOnPropertyChanged(object sender, GamePropertyEventArgs gamePropertyEventArgs)
        {
            if (gamePropertyEventArgs.Property.Name == "Text")
                AlertFileChange();
        }
    }
}