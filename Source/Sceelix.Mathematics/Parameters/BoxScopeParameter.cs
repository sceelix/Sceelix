using Sceelix.Core.Parameters;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Parameters
{
    public class BoxScopeParameter : CompoundParameter
    {
        private readonly Vector3DParameter _sizeParameter = new Vector3DParameter("Size", Vector3D.One);
        private readonly Vector3DParameter _translationParameter = new Vector3DParameter("Translation", Vector3D.Zero);
        private readonly Vector3DParameter _xAxisParameter = new Vector3DParameter("XAxis", Vector3D.XVector);
        private readonly Vector3DParameter _yAxisParameter = new Vector3DParameter("YAxis", Vector3D.YVector);
        private readonly Vector3DParameter _zAxisParameter = new Vector3DParameter("ZAxis", Vector3D.ZVector);



        public BoxScopeParameter(string label)
            : base(label)
        {
        }



        public BoxScopeParameter(string label, BoxScope boxScope)
            : base(label)
        {
            Value = boxScope;
        }



        public BoxScope Value
        {
            get { return new BoxScope(_xAxisParameter.Value, _yAxisParameter.Value, _zAxisParameter.Value, _translationParameter.Value, _sizeParameter.Value); }
            set
            {
                _xAxisParameter.Value = value.XAxis;
                _yAxisParameter.Value = value.YAxis;
                _zAxisParameter.Value = value.ZAxis;
                _sizeParameter.Value = value.Sizes;
                _translationParameter.Value = value.Translation;
            }
        }
    }
}