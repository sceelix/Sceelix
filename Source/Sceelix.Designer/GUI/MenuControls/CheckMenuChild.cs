using System;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.GUI.MenuControls
{
    public class CheckMenuChild : MenuChild
    {
        //private Texture2D _iconTexture;
        private bool _isChecked;



        public CheckMenuChild(Action<CheckMenuChild> clickAction)
            : base()
        {
            Click += delegate
            {
                IsChecked = !IsChecked;

                clickAction(this);
            };
        }



        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;

                UpdateIcon();
            }
        }



        private void UpdateIcon()
        {
            if (IsChecked)
            {
                //_iconTexture = Icon;
                Icon = EmbeddedResources.Load<Texture2D>("Resources/CheckmarkGrey.png");
            }
            else
            {
                Icon = null;
                //Icon = _iconTexture;
            }
        }
    }
}