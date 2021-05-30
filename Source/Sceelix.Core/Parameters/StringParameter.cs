using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class StringParameter : PrimitiveParameter<string>
    {
        public StringParameter(string label, string defaultValue)
            : base(label, defaultValue)
        {
        }



        internal StringParameter(StringParameterInfo stringParameterInfo)
            : base(stringParameterInfo)
        {
            IsPassword = stringParameterInfo.IsPassword;
            MaxLength = stringParameterInfo.MaxLength;
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



        protected override void SetData(object value)
        {
            base.SetData(value);

            //if there is a limit on the length of the string, trim it
            if (MaxLength > 0)
                _value = _value.Substring(0, MaxLength);
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            return new StringParameterInfo(this);
        }
    }
}