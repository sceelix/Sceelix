using System;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class DoubleParameter : PrimitiveParameter<double>
    {
        private double? _maxValue;
        private double? _minValue;



        public DoubleParameter(string label, double defaultValue)
            : base(label, defaultValue)
        {
        }



        internal DoubleParameter(DoubleParameterInfo doubleParameterInfo) : base(doubleParameterInfo)
        {
            _minValue = doubleParameterInfo.MinValue;
            _maxValue = doubleParameterInfo.MaxValue;
            Increment = doubleParameterInfo.Increment;
            DecimalDigits = doubleParameterInfo.DecimalDigits;
        }



        /// <summary>
        /// The number of decimal digits that will be 
        /// shown by default in the numeric control.
        /// </summary>
        public int? DecimalDigits
        {
            get;
            set;
        }


        /// <summary>
        /// The amount that will be incremented/decremented when a user
        /// clicks to increase/decrease the value.
        /// </summary>
        public double Increment
        {
            get;
            set;
        } = 0.1f;



        /// <summary>
        /// Optional: The maximum value accepted by this parameter.
        /// If a higher value attempts to be set, this value will be used instead.
        /// </summary>
        public double? MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }



        /// <summary>
        /// Optional: The minimum value accepted by this parameter.
        /// If a lower value attempts to be set, this value will be used instead.
        /// </summary>
        public double? MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }



        protected override void SetData(object value)
        {
            base.SetData(value);

            if (_minValue.HasValue)
                _value = Math.Max(_value, _minValue.Value);

            if (_maxValue.HasValue)
                _value = Math.Min(_value, _maxValue.Value);
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            return new DoubleParameterInfo(this);
        }
    }
}