using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters.Infos;

namespace Sceelix.Mathematics.Parameters
{
    public class Vector2DParameter : PrimitiveParameter<Vector2D>
    {
        /*public FloatParameter X = new FloatParameter("X", 0);
        public FloatParameter Y = new FloatParameter("Y", 0);*/



        public Vector2DParameter(string label)
            : base(label, Vector2D.Zero)
        {
        }



        public Vector2DParameter(string label, Vector2D vector2D)
            : base(label, vector2D)
        {
            Value = vector2D;
        }



        /*public Vector3D GetVector3F()
        {
            return new Vector3D(X.Value, Y.Value, Z.Value);
        }

        public void SetVector3F(Vector3D vector3F)
        {
            X.Value = vector3F.X;
            Y.Value = vector3F.Y;
            Z.Value = vector3F.Z;
        }*/



        protected override ParameterInfo ToParameterInfo()
        {
            return new Vector2DParameterInfo(this);
        }



        /*public new Vector2D Value
        {
            get { return new Vector2D(X.Value, Y.Value); }
            set
            {
                X.Value = value.X;
                Y.Value = value.Y;
            }
        }*/
    }
}