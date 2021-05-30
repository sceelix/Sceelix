using System;

namespace Sceelix.Designer.Graphs.GUI.Model
{
    public interface ISelectableUnit
    {
        void Select();
        void Deselect();
    }

    public interface IHoverableUnit
    {
        Object Tooltip
        {
            get;
        }



        bool IsHovered
        {
            get;
            set;
        }
    }
}