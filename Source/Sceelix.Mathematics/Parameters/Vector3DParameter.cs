using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters.Infos;

namespace Sceelix.Mathematics.Parameters
{
    public class Vector3DParameter : PrimitiveParameter<Vector3D>
    {
        /*public FloatParameter X = new FloatParameter("X", 0);
        public FloatParameter Y = new FloatParameter("Y", 0);
        public FloatParameter Z = new FloatParameter("Z", 0);*/



        public Vector3DParameter(string label)
            : base(label, Vector3D.Zero)
        {
        }



        public Vector3DParameter(string label, Vector3D vector3D)
            : base(label, vector3D)
        {
            //Value = vector3D;
        }



        protected override ParameterInfo ToParameterInfo()
        {
            return new Vector3DParameterInfo(this);
        }



        /*public new Vector3D Value
        {
            get { return new Vector3D(X.Value, Y.Value, Z.Value); }
            set
            {
                X.Value = value.X;
                Y.Value = value.Y;
                Z.Value = value.Z;
            }
        }*/

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
    }
}