using System;
using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.Settings
{
    public interface IVisualApplicationField
    {
        String Category
        {
            get;
            set;
        }



        string Label
        {
            get;
        }



        //this returns the control with data set
        UIControl GetControl();

        void ApplyProposal();

        void RejectProposal();
    }


    public abstract class PrimitiveApplicationField<T> : ApplicationField<T>, IVisualApplicationField
    {
        private object _proposedValue;
        private object _previousValue;



        protected PrimitiveApplicationField(T defaultValue)
            : base(defaultValue)
        {
        }


        /// <summary>
        /// Indicates if changing this option in the Settings Windows
        /// should immediately trigger the change/changing event.
        ///
        /// The change is rolled back if the user presses "Cancel". If the user presses
        /// "Okay", the events are NOT called again.
        /// 
        /// This is disabled by default.
        /// </summary>
        public bool AllowsPreview
        {
            get;
            set;
        }



        protected Object ProposedValue
        {
            get { return _proposedValue; }
            set
            {
                if (AllowsPreview)
                {
                    //if the user changes this more than once,
                    //we shouldn't this
                    if(_previousValue == null)
                        _previousValue = Value;

                    Value = (T)value;
                }

                _proposedValue = value;
            }
        }



        public abstract UIControl GetControl();



        public void ApplyProposal()
        {
            //if previewed is allowed, the value has already been assigned
            //otherwise we assign it now
            if (!AllowsPreview)
            {
                if (_proposedValue != null)
                {
                    Value = (T)_proposedValue;
                }
            }

            _proposedValue = _previousValue = null;
        }



        public void RejectProposal()
        {
            //if we had set a preview value and now we've canceled it,
            //we rollback the change
            if (AllowsPreview && _previousValue != null)
            {
                Value = (T)_previousValue;
            }

            _proposedValue = _previousValue = null;
        }



        public String Category
        {
            get;
            set;
        }
    }
}