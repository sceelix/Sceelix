using System;
using System.Globalization;
using System.Xml;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class IntParameterInfo : PrimitiveParameterInfo<int>
    {
        private int? _maxValue;
        private int? _minValue;



        public IntParameterInfo(string label)
            : base(label)
        {
        }



        public IntParameterInfo(IntParameter parameter)
            : base(parameter)
        {
            _minValue = parameter.MinValue;
            _maxValue = parameter.MaxValue;
            Increment = parameter.Increment;
        }



        public IntParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            _minValue = xmlNode.GetAttributeOrNullable<int>("MinValue");
            _maxValue = xmlNode.GetAttributeOrNullable<int>("MaxValue");
            Increment = xmlNode.GetAttributeOrDefault("Increment", Increment);
        }



        public int Increment
        {
            get;
            set;
        } = 1;



        public int? MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }



        public override string MetaType => "Int";



        public int? MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }



        public override Parameter ToParameter()
        {
            return new IntParameter(this);
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            if (_minValue.HasValue)
                writer.WriteAttributeString("MinValue", Convert.ToString(_minValue.Value, CultureInfo.InvariantCulture));

            if (_maxValue.HasValue)
                writer.WriteAttributeString("MaxValue", Convert.ToString(_maxValue.Value, CultureInfo.InvariantCulture));

            writer.WriteAttributeString("Increment", Convert.ToString(Increment, CultureInfo.InvariantCulture));

            base.WriteXMLDefinition(writer);
        }
    }
}