using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.Renderer3D.GUI
{
    public class Render3DInspectorControl : ContentControl
    {
        public Render3DInspectorControl()
        {
            StackPanel verticalStackPanel = new StackPanel();

            verticalStackPanel.Children.Add(new GroupBox() {Title = "Image Filters"});
            //Edge filter
            //SSAO
            //SSMA

            verticalStackPanel.Children.Add(new GroupBox() {Title = "Scene Elements"});
            //Axis
            //Show Text

            verticalStackPanel.Children.Add(new GroupBox() {Title = "Ground Elements"});
            //Ground

            verticalStackPanel.Children.Add(new GroupBox() {Title = "Sky"});


            verticalStackPanel.Children.Add(new GroupBox() {Title = "Ocean"});


            verticalStackPanel.Children.Add(new GroupBox() {Title = "Stuff"});

            //Ocean
            //Color
            //Stuff

            //Sky
            //
            Content = verticalStackPanel;
        }
    }
}