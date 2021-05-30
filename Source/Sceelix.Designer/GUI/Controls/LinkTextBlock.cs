using System.Diagnostics;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Helpers;

namespace Sceelix.Designer.GUI.Controls
{
    public class LinkTextBlock : TextBlock
    {
        public LinkTextBlock()
        {
            Foreground = Color.DeepSkyBlue;
        }



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            if (!InputService.IsMouseOrTouchHandled && IsMouseOver
                && InputService.IsPressed(MouseButtons.Left, false))
            {
                UrlHelper.OpenUrlInBrowser(Text);
            }
        }
    }
}