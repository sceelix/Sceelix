using System;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class IntParameter : PrimitiveParameter<int>
    {
        private int? _maxValue;
        private int? _minValue;



        public IntParameter(string label, int defaultValue)
            : base(label, defaultValue)
        {
        }



        public IntParameter(IntParameterInfo intParameterInfo)
            : base(intParameterInfo)
        {
            _minValue = intParameterInfo.MinValue;
            _maxValue = intParameterInfo.MaxValue;
            Increment = intParameterInfo.Increment;
        }



        /// <summary>
        /// The amount that will be incremented/decremented when a user
        /// clicks to increase/decrease the value.
        /// </summary>
        public int Increment
        {
            get;
            set;
        } = 1;



        /// <summary>
        /// Optional: The maximum value accepted by this parameter.
        /// If a higher value attempts to be set, this value will be used instead.
        /// </summary>
        public int? MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }



        /// <summary>
        /// Optional: The minimum value accepted by this parameter.
        /// If a lower value attempts to be set, this value will be used instead.
        /// </summary>
        public int? MinValue
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
            return new IntParameterInfo(this);
        }
    }
}