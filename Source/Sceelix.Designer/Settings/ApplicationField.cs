using System;
using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.Settings
{
    public abstract class ApplicationField
    {
        private ApplicationSettings _applicationSettings;
        private string _label;
        private Object _value;



        public string Label
        {
            get { return _label; }
            internal set { _label = value; }
        }



        internal ApplicationSettings ApplicationSettings
        {
            get { return _applicationSettings; }
            set { _applicationSettings = value; }
        }



        protected abstract Type Type
        {
            get;
        }



        internal void Set(Object value)
        {
            _value = typeof(IConvertible).IsAssignableFrom(Type) ? Convert.ChangeType(value, Type) : value;
        }



        internal Object Get()
        {
            return _value;
        }

        public event ApplicationFieldChanged Changed = delegate { };
        public event ApplicationFieldChanging Changing = delegate { return true; };


        protected virtual void InvokeChanged(Object oldValue, Object newValue)
        {
            Changed(this, oldValue, newValue);
        }

        protected virtual bool InvokeChanging(Object oldValue, Object newValue)
        {
            return Changing(this, oldValue, newValue);
        }
    }

    public delegate bool ApplicationFieldChanging(ApplicationField field, Object oldValue, Object newValue);

    public delegate void ApplicationFieldChanged(ApplicationField field, Object oldValue, Object newValue);

    public delegate bool ApplicationFieldChanging<T>(ApplicationField<T> field, T oldValue, T newValue);

    public delegate void ApplicationFieldChanged<T>(ApplicationField<T> field, T oldValue, T newValue);


    public class ApplicationField<T> : ApplicationField
    {
        public ApplicationField(T defaultValue)
        {
            Set(defaultValue);
        }



        public T Value
        {
            get { return (T) Get(); }
            set
            {
                //get the old value
                Object oldValue = Get();

                //only proceed if the value has actually changed
                if (!oldValue.Equals(value))
                {
                    if (InvokeChanging( oldValue, value))
                    {
                        //set a new value
                        Set(value);

                        //tell anyone interested
                        InvokeChanged((T) oldValue, value);

                        //save the data to disk
                        ApplicationSettings.SaveData();
                    }
                }
            }
        }



        protected override void InvokeChanged(object oldValue, object newValue)
        {
            base.InvokeChanged(oldValue, newValue);
            Changed(this, (T) oldValue, (T) newValue);
        }



        protected override bool InvokeChanging(object oldValue, object newValue)
        {
            return base.InvokeChanging(oldValue, newValue) 
                   && Changing(this, (T)oldValue, (T)newValue);
        }



        protected override Type Type
        {
            get { return typeof(T); }
        }


        public new event ApplicationFieldChanged<T> Changed = delegate { };
        public new event ApplicationFieldChanging<T> Changing = delegate { return true; };
    }
}