using System;
using System.Globalization;
using System.Xml;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class DoubleParameterInfo : PrimitiveParameterInfo<double>
    {
        private int? _decimalDigits;
        private double? _maxValue;
        private double? _minValue;



        public DoubleParameterInfo(string label)
            : base(label)
        {
        }



        public DoubleParameterInfo(DoubleParameter parameter)
            : base(parameter)
        {
            _minValue = parameter.MinValue;
            _maxValue = parameter.MaxValue;
            Increment = parameter.Increment;
            _decimalDigits = parameter.DecimalDigits;
        }



        public DoubleParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            _minValue = xmlNode.GetAttributeOrNullable<double>("MinValue");
            _maxValue = xmlNode.GetAttributeOrNullable<double>("MaxValue");
            Increment = xmlNode.GetAttributeOrDefault("Increment", Increment);
            _decimalDigits = xmlNode.GetAttributeOrNullable<int>("DecimalDigits");
        }



        public int? DecimalDigits
        {
            get { return _decimalDigits; }
            set { _decimalDigits = value; }
        }



        public double Increment
        {
            get;
            set;
        } = 0.1f;



        public double? MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }



        public double? MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }



        public override string ValueLiteral => FixedValue.ToString();



        public override Parameter ToParameter()
        {
            return new DoubleParameter(this);
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