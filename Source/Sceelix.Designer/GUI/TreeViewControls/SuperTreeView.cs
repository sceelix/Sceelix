using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public class Person
    {
        public Person()
        {
            Children = new List<Person>();
        }



        public String Name
        {
            get;
            set;
        }



        public int Age
        {
            get;
            set;
        }



        public bool IsMarried
        {
            get;
            set;
        }



        public List<Person> Children
        {
            get;
            set;
        }
    }

    public class SuperTreeView : ContentControl
    {
        private List<Object> _objects = new List<Object>();

        public Func<Object, IEnumerable<Object>> GetChildren;

        public Func<Object, int, String> GetColumnValue;

        public Func<Object, bool> HasChildren;



        public SuperTreeView()
        {
        }



        public List<Column> Columns
        {
            get;
            set;
        }



        public void SetObjects(IEnumerable<Object> objects)
        {
            _objects = new List<Object>(objects);
        }



        public class Column
        {
            public String Name
            {
                get;
                set;
            }



            public int Width
            {
                get;
                set;
            }
        }

        public class Item
        {
        }
    }

    public class SuperTreeViewTest
    {
        public SuperTreeViewTest()
        {
            SuperTreeView treeView = new SuperTreeView();
            treeView.Columns.Add(new SuperTreeView.Column() {Name = "Name", Width = 20});
            treeView.Columns.Add(new SuperTreeView.Column() {Name = "Age", Width = 50});

            treeView.GetColumnValue = delegate(object o, int i)
            {
                var person = (Person) o;
                if (i == 0)
                    return person.Name;
                else if (i == 1)
                    return person.Age.ToString();

                return String.Empty;
            };
            treeView.HasChildren = delegate(object o) { return ((Person) o).Children.Count > 0; };

            treeView.GetChildren = delegate(object o) { return ((Person) o).Children; };


            List<Person> persons = new List<Person>();
            for (int i = 0; i < 10; i++)
            {
                var person = new Person() {Age = i*10, IsMarried = true, Name = "Person " + i};

                for (int j = 0; j < 10; j++)
                    person.Children.Add(new Person() {Age = j*5, IsMarried = false, Name = "Child " + i});

                persons.Add(person);
            }

            treeView.SetObjects(persons);
        }
    }
}