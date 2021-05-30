using System;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Utils;

#if LINUX
using System.Drawing.Text;
#elif MACOS
using System.Drawing;
#endif

namespace Sceelix.Designer.GUI.Controls
{
    public class ToolTipControl : ContentControl
    {
        private static readonly int defaultWidth = 400;

        private static readonly String _baseHtml = EmbeddedResources.Load<String>("Resources/BaseHtml.html");
        
        private readonly string _html;


        public ToolTipControl(String text)
        {
            _html = CreateHtml("<span>" + text + " </span>");
        }


        public ToolTipControl(String title, String text)
        {
            _html = CreateHtml("<h1>" + title + "</h1><span>" + text + "</span>");
        }



        public ToolTipControl(String title, String text, String bottomText)
            : this(title, text)
        {
            _html = CreateHtml("<h1>" + title + "</h1><span>" + text + "</span><span>" + text + "</span>");
        }



        private string CreateHtml(string bodyhtml)
        {
            return _baseHtml.Replace("<body id=\"REPLACEME\"/>", "<body>" + bodyhtml + "</body>");
        }



        protected override void OnLoad()
        {
            Padding = new Vector4F(0);
            Content = new DigitalRune.Game.UI.Controls.Image() {Texture = HtmlToImage(_html), Margin = new Vector4F(0)};
            VisualParent.Padding = new Vector4F(0);
            //Orientation = Orientation.Vertical;
        }



        private Texture2D HtmlToImage(String html)
        {
            int maxWidth = float.IsNaN(MaxWidth) ? defaultWidth : (int)MaxWidth;

#if WINDOWS
            System.Drawing.Image image = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImage(html, maxWidth: maxWidth);
            return image.ToTexture2D(this.Screen.Renderer.GraphicsDevice);
            //image.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Test" + val1++ + ".png", ImageFormat.Png);
#elif MACOS
            SizeF size;

            TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.AddFontFamily(new FontFamily("Arial"));
            TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.AddFontFamily(new FontFamily("Times New Roman"));

            using (var graphics = Graphics.FromImage(new Bitmap(1, 1)))
                size = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.MeasureGdiPlus(graphics, html, maxWidth: maxWidth);

            
            Bitmap bitmap;
            using (var graphics = Graphics.FromImage(bitmap = new Bitmap((int)size.Width, (int)size.Height)))
                TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderGdiPlus(graphics, html, maxWidth: maxWidth);

            var colorArray = bitmap.ToColorArray();
            Texture2D texture2D = new Texture2D(this.Screen.Renderer.GraphicsDevice, bitmap.Width, bitmap.Height);
            texture2D.SetData(colorArray);

            return texture2D;
#else

            System.Drawing.Image image = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImageGdiPlus(html, maxWidth: maxWidth, textRenderingHint: TextRenderingHint.SystemDefault);
            return image.ToTexture2D(this.Screen.Renderer.GraphicsDevice);
#endif
        }


    };
}