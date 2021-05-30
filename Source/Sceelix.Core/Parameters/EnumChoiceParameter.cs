using System;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class EnumChoiceParameter<TR> : PrimitiveParameter<string>
    {
        public EnumChoiceParameter(string label, Enum defaultValue)
            : base(label, defaultValue.ToString())
        {
        }



        public new TR Value
        {
            get { return (TR) Enum.Parse(typeof(TR), base.Value); }
            set { base.Value = value.ToString(); }
        }



        /*public TR ToEnum()
        {
            return (TR)Enum.Parse(typeof (TR), Value);
        }*/



        protected internal override ParameterInfo ToParameterInfo()
        {
            var array = Enum.GetNames(typeof(TR));

            return new ChoiceParameterInfo(this, array);
        }
    }
}