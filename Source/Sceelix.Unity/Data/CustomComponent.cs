namespace Sceelix.Unity.Data
{
    public class CustomComponent : UnityComponent
    {
        public CustomComponent()
            : base("Custom")
        {
        }



        public string ComponentName
        {
            get;
            set;
        }
    }
}