namespace Sceelix.Surfaces.Data
{
    public interface I3DLayer
    {
        float MaxHeight
        {
            get;
        }


        float MinHeight
        {
            get;
        }


        void ScaleVertically(float amount);
        void TranslateVertically(float amount);
    }
}