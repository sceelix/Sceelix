using System;
using System.Globalization;
using System.Xml;
using Sceelix.Conversion;
using Sceelix.Core.Environments;
using Sceelix.Extensions;
using Sceelix.Helpers;

namespace Sceelix.Core.Parameters.Infos
{
    public class StringParameterInfo : PrimitiveParameterInfo<string>
    {
        public StringParameterInfo(string label)
            : base(label)
        {
            FixedValue = string.Empty;
        }



        public StringParameterInfo(string label, string description, bool isPublic, string fixedValue, bool isPassword, int maxLength)
            : base(label, description, isPublic, fixedValue)
        {
            IsPassword = isPassword;
            MaxLength = maxLength;
        }



        public StringParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            IsPassword = xmlNode.GetAttributeOrDefault("IsPassword", IsPassword);
            MaxLength = xmlNode.GetAttributeOrDefault("MaxLength", MaxLength);

            //if this parameter is marked as a password, decrypt it
            if (IsPassword)
                FixedValue = DataProtectionHelper.DecryptData(FixedValue, "Holy shit, a string parameter with a password!");
        }



        public StringParameterInfo(StringParameter parameter)
            : base(parameter)
        {
            IsPassword = parameter.IsPassword;
            MaxLength = parameter.MaxLength;
        }



        /// <summary>
        /// Indicates if the value should be treated as a password
        /// (it will be hidden and encrypted).
        /// </summary>
        public bool IsPassword
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum size that the input string allows. 0 means no limit.
        /// The input value will be trimmed if larger than the allowed value.
        /// </summary>
        public int MaxLength
        {
            get;
            set;
        }


        public override string ValueLiteral => FixedValue?.Quote();



        public override void ReadArgumentXML(XmlNode xmlNode, IProcedureEnvironment procedureEnvironment)
        {
            base.ReadArgumentXML(xmlNode, procedureEnvironment);

            //if this parameter is marked as a password, decrypt it
            if (IsPassword)
                FixedValue = DataProtectionHelper.DecryptData(FixedValue, "Holy shit, a string parameter with a password!");
        }



        public override Parameter ToParameter()
        {
            return new StringParameter(this);
        }



        protected override void WriteFixedValue(XmlWriter writer)
        {
            //if this parameter is marked as a password, encrypt it
            if (IsPassword)
                writer.WriteAttributeString("FixedValue", DataProtectionHelper.EncryptData(ConvertHelper.Convert<string>(FixedValue), "Holy shit, a string parameter with a password!"));
            else
                writer.WriteAttributeString("FixedValue", ConvertHelper.Convert<string>(FixedValue));
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            if (IsPassword)
                writer.WriteAttributeString("IsPassword", Convert.ToString(IsPassword, CultureInfo.InvariantCulture));

            if (MaxLength > 0)
                writer.WriteAttributeString("MaxLength", Convert.ToString(MaxLength, CultureInfo.InvariantCulture));

            base.WriteXMLDefinition(writer);
        }
    }
}