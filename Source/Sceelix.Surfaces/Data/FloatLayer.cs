using Sceelix.Helpers;
using Sceelix.Mathematics.Data;

namespace Sceelix.Surfaces.Data
{
    public abstract class FloatLayer : GenericSurfaceLayer<float>
    {
        protected float _min, _max;



        protected FloatLayer(float[,] values) : base(values)
        {
            IsMinMaxDirty = true;
        }



        protected bool IsMinMaxDirty
        {
            get;
            set;
        } = true;



        public virtual float MaxValue
        {
            get
            {
                if (IsMinMaxDirty)
                    RecalculateMinMax();

                return _max;
            }
        }



        public virtual float MinValue
        {
            get
            {
                if (IsMinMaxDirty)
                    RecalculateMinMax();

                return _min;
            }
        }



        protected override float Add(float valueA, float valueB)
        {
            return valueA + valueB;
        }



        protected override float InvertValue(float value)
        {
            return _min + (_max - value);
        }



        protected override float Minus(float valueA, float valueB)
        {
            return valueA - valueB;
        }



        protected override float Multiply(float value1, float value2)
        {
            return value1 * value2;
        }



        protected override float MultiplyScalar(float value, float scalar)
        {
            return value * scalar;
        }



        protected virtual void RecalculateMinMax()
        {
            _min = float.MaxValue;
            _max = float.MinValue;

            ParallelHelper.For(0, NumColumns, i =>
            {
                for (int j = 0; j < NumRows; j++)
                {
                    var value = _values[i, j];
                    if (value < _min)
                        _min = value;

                    if (value > _max)
                        _max = value;
                }
            });

            IsMinMaxDirty = false;
        }



        public override void SetValue(Coordinate layerCoordinate, object value)
        {
            base.SetValue(layerCoordinate, value);

            IsMinMaxDirty = true;
        }

        
        public override void Update()
        {
            RecalculateMinMax();
        }
    }
}