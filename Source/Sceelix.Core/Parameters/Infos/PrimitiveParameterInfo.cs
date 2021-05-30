using System.Xml;
using Sceelix.Conversion;
using Sceelix.Core.Environments;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public abstract class PrimitiveParameterInfo<T> : ParameterInfo
    {
        protected PrimitiveParameterInfo(string label)
            : base(label)
        {
            FixedValue = default(T);
        }



        protected PrimitiveParameterInfo(string label, T fixedValue)
            : base(label)
        {
            FixedValue = fixedValue;
        }



        protected PrimitiveParameterInfo(string label, string description, bool isPublic, T fixedValue)
            : base(label, description, isPublic)
        {
            FixedValue = fixedValue;
        }



        protected PrimitiveParameterInfo(PrimitiveParameter<T> parameter)
            : base(parameter)
        {
            FixedValue = parameter.Value;
        }



        protected PrimitiveParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            FixedValue = ConvertHelper.Convert<T>(xmlNode.Attributes["FixedValue"].InnerText);
        }



        public T FixedValue
        {
            get;
            set;
        }


        public override string MetaType => typeof(T).Name;


        public override string ValueLiteral => FixedValue.SafeToString();



        public override object Clone()
        {
            var parameterInfo = (PrimitiveParameterInfo<T>) base.Clone();
            parameterInfo.FixedValue = (T) FixedValue.Clone();
            return parameterInfo;
        }



        public override void ReadArgumentXML(XmlNode xmlNode, IProcedureEnvironment procedureEnvironment)
        {
            base.ReadArgumentXML(xmlNode, procedureEnvironment);

            FixedValue = ConvertHelper.Convert<T>(xmlNode.Attributes["FixedValue"].InnerText);
        }



        public override void WriteArgumentXML(XmlWriter writer)
        {
            WriteFixedValue(writer);

            base.WriteArgumentXML(writer);
        }



        protected virtual void WriteFixedValue(XmlWriter writer)
        {
            writer.WriteAttributeString("FixedValue", ConvertHelper.Convert<string>(FixedValue));
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            base.WriteXMLDefinition(writer);

            WriteFixedValue(writer);
        }



        /*public override bool IsStructurallyEqual(ParameterInfo parameterInfo)
        {
            if (base.IsStructurallyEqual(parameterInfo))
            {
                
            }
            
            return this.GetType() == parameterInfo.GetType();
        }*/
    }
}