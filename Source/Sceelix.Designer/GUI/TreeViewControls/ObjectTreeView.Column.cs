using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public partial class ObjectTreeView
    {
        public class Column
        {
            private bool _allowSorting = true;
            private bool _flexible;
            private string _name;
            private float _width;



            public Column()
            {
            }



            public Column(string name)
            {
                Name = name;
            }



            public Column(string name, float width, bool flexible)
            {
                Name = name;
                Width = width;
                Flexible = flexible;
            }



            public String Name
            {
                get { return _name; }
                set { _name = value; }
            }



            public float Width
            {
                get { return _width; }
                set { _width = value; }
            }



            public bool Flexible
            {
                get { return _flexible; }
                set { _flexible = value; }
            }



            public bool AllowSorting
            {
                get { return _allowSorting; }
                set { _allowSorting = value; }
            }
        }
    }
}