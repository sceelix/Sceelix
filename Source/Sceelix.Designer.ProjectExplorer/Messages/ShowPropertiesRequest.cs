using System;
using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class ShowPropertiesRequest
    {
        private readonly UIControl _controlToShow;
        private readonly Object _owner;



        public ShowPropertiesRequest(UIControl controlToShow, Object owner)
        {
            _controlToShow = controlToShow;
            _owner = owner;
        }



        public UIControl ControlToShow
        {
            get { return _controlToShow; }
        }



        public object Owner
        {
            get { return _owner; }
        }
    }
}