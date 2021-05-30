using System;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class FloatParameter : PrimitiveParameter<float>
    {
        private float? _maxValue;
        private float? _minValue;



        public FloatParameter(string label, float defaultValue)
            : base(label, defaultValue)
        {
        }



        internal FloatParameter(FloatParameterInfo floatParameterInfo)
            : base(floatParameterInfo)
        {
            _minValue = floatParameterInfo.MinValue;
            _maxValue = floatParameterInfo.MaxValue;
            Increment = floatParameterInfo.Increment;
            DecimalDigits = floatParameterInfo.DecimalDigits;
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
        public float Increment
        {
            get;
            set;
        } = 0.1f;



        /// <summary>
        /// Optional: The maximum value accepted by this parameter.
        /// If a higher value attempts to be set, this value will be used instead.
        /// </summary>
        public float? MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }



        /// <summary>
        /// Optional: The minimum value accepted by this parameter.
        /// If a lower value attempts to be set, this value will be used instead.
        /// </summary>
        public float? MinValue
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
            return new FloatParameterInfo(this);
        }



        public TypedParameterReference<float> ToReference()
        {
            return new TypedParameterReference<float>(this);
        }
    }
}