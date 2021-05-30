using Sceelix.Designer.Annotations;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Settings.Types;

namespace Sceelix.Designer.Meshes.Controls
{
    [ApplicationSettings("Model Viewer")]
    public class ModelViewer3DSettings : ApplicationSettings
    {
        /// <summary>
        /// If enabled, renders a wireframe version of the model.
        /// </summary>
        public readonly BoolApplicationField RenderWireframe = new BoolApplicationField(true) { AllowsPreview = true };

        /// <summary>
        /// Toggles axis arrow drawing on the world's origin (0,0,0).
        /// </summary>
        public readonly BoolApplicationField ShowAxis = new BoolApplicationField(true) { AllowsPreview = true };


        public ModelViewer3DSettings() 
            : base("Model Viewer")
        {
        }
    }
}
