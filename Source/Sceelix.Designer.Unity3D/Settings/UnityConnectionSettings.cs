using Sceelix.Designer.Annotations;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Settings.Types;

namespace Sceelix.Designer.Unity3D.Settings
{
    [ApplicationSettings("Unity")]
    public class UnityConnectionSettings : ApplicationSettings
    {
        /// <summary>
        /// Address to which the the Unity Plugin should connect.
        /// This value should match the configuration in the Unity Plugin.
        /// </summary>
        public StringApplicationField Address = new StringApplicationField("127.0.0.1");

        /// <summary>
        /// Port through which the Unity Plugin should connect.
        /// This value should match the configuration in the Unity Plugin.
        /// </summary>
        public IntApplicationField Port = new IntApplicationField(3500);



        public UnityConnectionSettings()
            : base("UnityConnection")
        {
        }
    }
}