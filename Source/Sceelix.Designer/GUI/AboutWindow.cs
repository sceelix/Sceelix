using System.Reflection;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.GUI
{
    public class AboutWindow : DialogWindow
    {
        public AboutWindow()
        {
            Title = "About Sceelix Designer";
            //Width = 420;
        }



        public string AssemblyVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }



        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }



        protected override void OnLoad()
        {
            /*FlexibleStackPanel windowPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };*/

            FlexibleStackPanel contentPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            contentPanel.Children.Add(new Image
            {
                Width = 170,
                Style = "AboutWindowLogo",
                Foreground = Color.White,
                Margin = new Vector4F(10),
                MinHeight = 286
            });

            var rightStackPanel = new FlexibleStackPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Orientation = Orientation.Vertical,
                Margin = new Vector4F(10),

                //Width = 200,
            };

            var textStackPanel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Orientation = Orientation.Vertical,
                Margin = new Vector4F(0),
                //Background = Color.Yellow
            };

            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(10, 0, 10, 10),
                Text = "Product:"
            });

            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(20, 0, 0, 5),
                Text = "Sceelix Designer"
            });

            var programVersion = OSVersionInfo.ProgramBits == OSVersionInfo.SoftwareArchitecture.Bit32
                ? "32 bit"
                : "64 bit";
            

            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(20, 0, 0, 5),
                Text = "Version " + AssemblyVersion + " (" + BuildPlatform.Enum + " " + programVersion + ")"
            });


            if (BuildDistribution.IsSteam)
            {
                textStackPanel.Children.Add(new TextBlock
                {
                    Margin = new Vector4F(20, 0, 0, 5),
                    Text = "Steam Edition"
                });
            }

            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(20, 0, 0, 5),
                Text = AssemblyCopyright
            });

            textStackPanel.Children.Add(new LinkTextBlock
            {
                Margin = new Vector4F(20, 0, 0, 30),
                Text = "https://sceelix.com"
            });

            rightStackPanel.Children.Add(textStackPanel);

            contentPanel.Children.Add(rightStackPanel);

            DialogContent = contentPanel;

            AddOKButton();

            base.OnLoad();
        }
    }
}