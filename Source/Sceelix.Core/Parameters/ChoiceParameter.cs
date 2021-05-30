using System.Linq;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class ChoiceParameter : PrimitiveParameter<string>
    {
        public ChoiceParameter(string label, string defaultValue)
            : base(label, defaultValue)
        {
        }



        /*public ChoiceParameter(string label, params String[] choices)
            : base(label, choices[0])
        {
            Choices = choices.ToArray();
        }*/



        public ChoiceParameter(string label, string defaultValue, params string[] choices)
            : base(label, defaultValue)
        {
            //if (!choices.Contains(defaultValue))


            Choices = choices.ToArray();
        }



        internal ChoiceParameter(ChoiceParameterInfo choiceParameterInfo)
            : base(choiceParameterInfo)
        {
            Choices = (string[]) choiceParameterInfo.Choices.Clone();
        }



        public string[] Choices
        {
            get;
            set;
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            return new ChoiceParameterInfo(this);
        }



        /*public override object Clone()
        {
            var clone = (ChoiceParameter)base.Clone();
            clone.Choices = Choices.Select(x => x).ToArray();

            return clone;
        }*/
    }
}