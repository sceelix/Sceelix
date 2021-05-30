namespace Sceelix.Collections
{
    public delegate bool NotifyPropertyChanging<T>(NotifyProperty<T> field, T oldValue, T newValue);

    public delegate void NotifyPropertyChanged<T>(NotifyProperty<T> field, T oldValue, T newValue);


    public class NotifyProperty<T>
    {
        private T _value;



        public NotifyProperty()
        {
            _value = default(T);
        }



        public NotifyProperty(T defaultValue)
        {
            _value = defaultValue;
        }



        public T Value
        {
            get { return _value; }
            set
            {
                //get the old value
                object oldValue = _value;

                //only proceed if the value has actually changed
                if (!oldValue.Equals(value))
                    if (Changing == null || Changing.Invoke(this, (T) oldValue, value))
                    {
                        //set a new value
                        _value = value;

                        //tell anyone interested
                        if (Changed != null)
                            Changed.Invoke(this, (T) oldValue, value);
                    }
            }
        }



        public event NotifyPropertyChanged<T> Changed;
        public event NotifyPropertyChanging<T> Changing;
    }
}