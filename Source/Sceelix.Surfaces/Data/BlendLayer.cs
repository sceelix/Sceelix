using Sceelix.Core.Annotations;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;

namespace Sceelix.Surfaces.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Sceelix.Surfaces.Data.FloatLayer" />
    [Entity("Blend Layer", TypeBrowsable = false)]
    public class BlendLayer : FloatLayer
    {
        //it is better to keep this index, otherwise subselection, division and merging could become complicated.



        public BlendLayer(float[,] values, int textureIndex)
            : base(values)
        {
            TextureIndex = textureIndex;
        }



        [EntityProperty]
        public int TextureIndex
        {
            get;
            set;
        }



        public override bool CanCombineWith(SurfaceLayer otherLayer)
        {
            return base.CanCombineWith(otherLayer)
                   && ((BlendLayer) otherLayer).TextureIndex == TextureIndex;
        }



        public override SurfaceLayer CreateEmpty(int numColumns, int numRows)
        {
            return new BlendLayer(new float[numColumns, numRows], TextureIndex);
        }



        protected override float InvertValue(float value)
        {
            return 1 - value;
        }



        public override void SetValue(Coordinate layerCoordinate, object value)
        {
            base.SetValue(layerCoordinate, MathHelper.Clamp((float) value, 0, 1));
        }
    }
}