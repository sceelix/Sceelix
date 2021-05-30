using System;
using System.Globalization;
using System.Xml;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class FloatParameterInfo : PrimitiveParameterInfo<float>
    {
        private int? _decimalDigits;
        private float? _maxValue;
        private float? _minValue;



        public FloatParameterInfo(string label)
            : base(label)
        {
        }



        public FloatParameterInfo(FloatParameter parameter)
            : base(parameter)
        {
            _minValue = parameter.MinValue;
            _maxValue = parameter.MaxValue;
            Increment = parameter.Increment;
            _decimalDigits = parameter.DecimalDigits;
        }



        public FloatParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            _minValue = xmlNode.GetAttributeOrNullable<float>("MinValue");
            _maxValue = xmlNode.GetAttributeOrNullable<float>("MaxValue");
            Increment = xmlNode.GetAttributeOrDefault("Increment", Increment);
            _decimalDigits = xmlNode.GetAttributeOrNullable<int>("DecimalDigits");
        }



        public int? DecimalDigits
        {
            get { return _decimalDigits; }
            set { _decimalDigits = value; }
        }



        public float Increment
        {
            get;
            set;
        } = 0.1f;



        public float? MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }



        public override string MetaType => "Float";



        public float? MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }



        public override string ValueLiteral => FixedValue.SafeToString() + "f";



        public override Parameter ToParameter()
        {
            return new FloatParameter(this);
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            if (_minValue.HasValue)
                writer.WriteAttributeString("MinValue", Convert.ToString(_minValue.Value, CultureInfo.InvariantCulture));

            if (_maxValue.HasValue)
                writer.WriteAttributeString("MaxValue", Convert.ToString(_maxValue.Value, CultureInfo.InvariantCulture));

            writer.WriteAttributeString("Increment", Convert.ToString(Increment, CultureInfo.InvariantCulture));

            if (_decimalDigits.HasValue)
                writer.WriteAttributeString("DecimalDigits", Convert.ToString(_decimalDigits, CultureInfo.InvariantCulture));

            base.WriteXMLDefinition(writer);
        }
    }
}