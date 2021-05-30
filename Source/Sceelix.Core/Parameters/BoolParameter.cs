using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class BoolParameter : PrimitiveParameter<bool>
    {
        public BoolParameter(string label, bool defaultValue)
            : base(label, defaultValue)
        {
        }



        public BoolParameter(BoolParameterInfo boolParameterInfo)
            : base(boolParameterInfo)
        {
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            return new BoolParameterInfo(this);
        }
    }
}