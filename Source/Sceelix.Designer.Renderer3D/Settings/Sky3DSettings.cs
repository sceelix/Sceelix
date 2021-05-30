using Sceelix.Designer.Annotations;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Settings.Types;

namespace Sceelix.Designer.Renderer3D.Settings
{
    #if WINDOWS
    [ApplicationSettings("Renderer 3D/Sky")]
    #endif
    public class Sky3DSettings : ApplicationSettings
    {
        /// <summary>
        /// Toggles the sky box.
        /// </summary>
        public readonly BoolApplicationField EnableSky = new BoolApplicationField(false){AllowsPreview = true};

        /// <summary>
        /// Toggles cloud rendering on sky box.
        /// </summary>
        public readonly BoolApplicationField EnableClouds = new BoolApplicationField(true) { AllowsPreview = true };

        /// <summary>
        /// Random look of the skybox clouds, if enabled.
        /// </summary>
        public readonly IntApplicationField CloudSeed = new IntApplicationField(7777) { AllowsPreview = true };

        /// <summary>
        /// Toggles cloud shadow effects from the skybox clouds, if enabled.
        /// </summary>
        public readonly BoolApplicationField EnableCloudShadows = new BoolApplicationField(false) { AllowsPreview = true };



        /// <summary>
        /// Determines the time of day (the hours), affecting sun and light position and direction.
        /// Only applicable if the sky is enabled. 
        /// </summary>
        public readonly IntApplicationField Hour = new IntApplicationField(10) { AllowsPreview = true };

        /// <summary>
        /// Determines the time of day when (the minutes), affecting sun and light position and direction.
        /// Only applicable if the sky is enabled. 
        /// </summary>
        public readonly IntApplicationField Minutes = new IntApplicationField(0) { AllowsPreview = true };



        public Sky3DSettings()
            : base("Sky3D")
        {
        }
    }
}