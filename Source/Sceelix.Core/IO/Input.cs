using System;
using System.Collections.Generic;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.IO
{
    public enum InputNature
    {
        Single,
        Collective
    }

    public abstract class Input
    {
        //entities that are being read in this round are placed here



        protected Input(string label, Type entityType)
        {
            //if (!Regex.IsMatch(label, "^[a-zA-Z][a-zA-Z0-9\\s]*$"))
            //    throw new ArgumentException("Label '" + label + "' is not valid for an input. Labels can only contain alphanumeric and whitespace characters and must start with a letter of the alphabet.");

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


        /*public string UniqueLabel
        {
            get
            {
                if (Parent is Parameter)
                {
                    var uniqueLabel = ((Parameter)Parent).FullName;

                    return uniqueLabel;
                }

                return Label;
            }
        }*/


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


        public abstract InputNature InputNature
        {
            get;
        }


        public bool IsOptional
        {
            get;
            set;
        }


        public string Label
        {
            get;
            internal set;
        }


        protected internal object Parent
        {
            get;
            set;
        }



        /// <summary>
        /// Procedure to which this input belongs.
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



        internal int QueueCount => CurrentObjects.Count;


        protected internal List<IEntity> RoundEntities
        {
            get;
        } = new List<IEntity>();



        internal void ClearRoundData()
        {
            RoundEntities.Clear();
        }



        internal void Enqueue(IEntity obj)
        {
            CurrentObjects.Enqueue(obj);
        }



        internal void Enqueue(IEnumerable<IEntity> objects)
        {
            foreach (IEntity obj in objects)
                CurrentObjects.Enqueue(obj);
        }



        internal virtual bool HasDataReady()
        {
            return CurrentObjects.Count > 0;
        }



        protected abstract IEnumerable<IEntity> PrepareNextData();



        internal void PrepareRoundData()
        {
            RoundEntities.AddRange(PrepareNextData());
        }



        public abstract object Read();



        internal InputPort ToInputPort(bool isParameterPort = false)
        {
            return new InputPort(this, InputNature) {IsParameterPort = isParameterPort, Description = Description, IsOptional = IsOptional};
        }
    }
}