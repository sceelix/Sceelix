using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.IO
{
    /*public class IOClass
    {
        private string _label;
        private readonly Type _type;

        public Output(string label, Type type)
        {
            _label = label;
            _type = type;
        }

    }*/

    public class Output
    {
        //private readonly Queue<Queue<Entity>> _currentObjects = new Queue<<Entity>>();

        //private readonly Queue<Queue<Entity>> _executionObjects = new Queue<Queue<Entity>>();

        private readonly Queue<IEntity> _lastRoundObjects = new Queue<IEntity>();



        internal Output(string label, Type entityType)
        {
            //if (!Regex.IsMatch(label, "^[a-zA-Z][a-zA-Z0-9\\s]*$"))
            //    throw new ArgumentException("Label '" + label + "' is not valid for an output. Labels can only contain alphanumeric and whitespace characters and must start with a letter of the alphabet.");

            Label = label;
            EntityType = entityType;
        }



        protected internal Queue<IEntity> CurrentObjects
        {
            get;
        } = new Queue<IEntity>();


        public string Description
        {
            get;
            set;
        }


        public Type EntityType
        {
            get;
        }



        public string FullLabel
        {
            get
            {
                if (Parent is Parameter) return ((Parameter) Parent).FullLabel + "." + Label;

                return Label;
            }
        }



        public string Identifier => Parameter.GetIdentifier(Label);


        public bool IsEmpty => !CurrentObjects.Any();


        internal string Label
        {
            get;
            set;
        }


        protected internal object Parent
        {
            get;
            set;
        }



        /// <summary>
        /// Procedure to which this output belongs.
        /// </summary>
        public virtual Procedure Procedure
        {
            get
            {
                if (Parent is Procedure)
                    return (Procedure) Parent;

                return ((Parameter) Parent).Procedure;
            }
        }



        internal void Clear()
        {
            CurrentObjects.Clear();
        }



        internal IEntity Dequeue()
        {
            return CurrentObjects.Dequeue();
        }



        internal IEnumerable<IEntity> DequeueAll()
        {
            IEnumerable<IEntity> copy = new List<IEntity>(CurrentObjects);
            CurrentObjects.Clear();

            return copy;
        }



        internal IEnumerable<IEntity> DequeueRoundEntitites()
        {
            IEnumerable<IEntity> copy = new List<IEntity>(_lastRoundObjects);
            _lastRoundObjects.Clear();

            return copy;
        }



        internal IEntity Peek()
        {
            return IsEmpty ? null : CurrentObjects.Peek();
        }



        internal IEnumerable<IEntity> PeekAll()
        {
            return new List<IEntity>(CurrentObjects);
        }



        public OutputPort ToOutputPort(bool isParameterPort = false)
        {
            return new OutputPort(this) {IsParameterPort = isParameterPort, Description = Description};
        }



        internal void Write(IEntity obj)
        {
            CurrentObjects.Enqueue(obj);
            _lastRoundObjects.Enqueue(obj);
        }



        internal void Write(IEnumerable<IEntity> objects)
        {
            foreach (IEntity obj in objects)
            {
                CurrentObjects.Enqueue(obj);
                _lastRoundObjects.Enqueue(obj);
            }
        }



        /*public string UniqueLabel
        {
            get
            {
                if (Parent is Parameter)
                    return ((Parameter) Parent).FullName;

                return Label;
            }
        }*/
    }


    public class Output<T> : Output where T : IEntity
    {
        public Output(string label)
            : base(label, typeof(T))
        {
        }



        public void Write(T obj)
        {
            base.Write(obj);
        }



        public void Write(IEnumerable<T> objects)
        {
            base.Write(objects.Cast<IEntity>());
            //foreach (T obj in objects)
            //    CurrentObjects.Enqueue(obj);
        }



        /*public IEntity this[InputData data]
        {
            //get { }
            set { }
        }

        public IEnumerable<IEntity> this[InputData data]
        {
            //get { }
            set { }
        }*/


        /*public new T Pop()
        {
            return (T)base.Pop();
        }

        public new T Peek()
        {
            return (T)base.Peek();
        }

        public new IEnumerable<T> PopAll()
        {
            return base.PopAll().Cast<T>();
        }

        public new IEnumerable<T> PeekAll()
        {
            return base.PeekAll().Cast<T>();
        }*/
    }
}