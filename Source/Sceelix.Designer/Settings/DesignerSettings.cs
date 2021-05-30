using System;
using System.Diagnostics;
using Sceelix.Designer.Settings.Types;

namespace Sceelix.Designer.Settings
{
    public class DesignerSettings : ApplicationSettings
    {
#if MACOS
        public readonly ApplicationField<bool> LoadedCache = new ApplicationField<bool>(false);
        #endif
        
        /// <summary>
        /// Toggles Framerate on the application title.
        /// </summary>
        public readonly BoolApplicationField ShowFps = new BoolApplicationField(false);

        /// <summary>
        /// Toggles frame limiting to 60 frames per second.
        /// If disabled, the highest possible framerates will be attempted, with its corresponding CPU overhead.
        /// </summary>
        public readonly BoolApplicationField Use60FpsLimit = new BoolApplicationField(true);

        /// <summary>
        /// Determines how much the UI controls should be scaled. Same as using the CTRL + '+' or CTRL + '-'.
        /// Changing the UI scale is probably advisable on monitors whose resolutions largely differ from the standard FullHD resolution (1920x1080).
        /// </summary>
        public readonly ChoiceApplicationField UIScale = new ChoiceApplicationField("100%", "25%", "50%", "75%", "100%", "125%", "150%", "175%", "200%", "300%", "400%");

        /// <summary>
        /// Maximum number of log files that will be stored. Leave 0 to not remove log files.
        /// </summary>
        public readonly IntApplicationField MaxLogFiles = new IntApplicationField(10) {Minimum = 0};

        /// <summary>
        /// File and Folder names/extensions to exclude when mass importing existing Folders and Contents.
        /// </summary>
        public readonly StringApplicationField ImportExclusions = new StringApplicationField("*.slxp,Thumbs.db");

        /// <summary>
        /// Used for identifying the source of statistical data. 
        /// </summary>
        public readonly ApplicationField<String> LocalGuid = new ApplicationField<String>(Guid.NewGuid().ToString());


        public DesignerSettings()
            : base("Designer")
        {
        }



        public bool IsUIScaled
        {
            get { return UIScale.Value != "100%"; }
        }



        public float UIScaleValue
        {
            get { return ParseRenderScale(UIScale.Value); }
        }



        public float ParseRenderScale(String renderScale)
        {
            return 1/(Int32.Parse(renderScale.Replace("%", ""))/100f);
        }
    }
}