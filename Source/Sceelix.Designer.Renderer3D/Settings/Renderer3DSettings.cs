using Sceelix.Designer.Annotations;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Settings.Types;

namespace Sceelix.Designer.Renderer3D.Settings
{
    [ApplicationSettings("Renderer 3D")]
    public class Renderer3DSettings : ApplicationSettings
    {
       /// <summary>
        /// Toggles axis arrow drawing on the world's origin (0,0,0).
        /// </summary>
        public readonly BoolApplicationField ShowAxis = new BoolApplicationField(true) {Category = "Objects", AllowsPreview = true};

        /// <summary>
        /// Toggles the ground plane, located at Z = 0.
        /// </summary>
        public readonly BoolApplicationField ShowGround = new BoolApplicationField(true) {Category = "Objects", AllowsPreview = true };

        /// <summary>
        /// Toggles edges drawing of mesh polygons.
        /// This is useful to better understand the boundaries of each polygon, but can carry a significant 
        /// rendering overhead for more larger result sets.
        /// </summary>
        public readonly BoolApplicationField ShowFaceEdges = new BoolApplicationField(true) { Category = "Meshes", AllowsPreview = true };

        /// <summary>
        /// Toggles vertex connectivity information in path vertices.
        /// This is useful to better understand the connectivity for each vertex, but may hard to visualize, 
        /// if many vertices are displayed.
        /// </summary>
        public readonly BoolApplicationField ShowPathConnectivity = new BoolApplicationField(true) {Category = "Paths", AllowsPreview = true };

        /// <summary>
        /// Toggles edge direction information in path vertices.
        /// This is useful to better understand the source and target vertex of each edge.
        /// </summary>
        public readonly BoolApplicationField ShowPathDirection = new BoolApplicationField(true) { Category = "Paths", AllowsPreview = true };

        /// <summary>
        /// Inverts mouse camera movement.
        /// </summary>
        public readonly BoolApplicationField InvertLook = new BoolApplicationField(false) { Category = "Camera" };

        /// <summary>
        /// Defines how fast the camera orientation changes (using the mouse).
        /// </summary>
        public readonly FloatApplicationField MouseSpeed = new FloatApplicationField(3) { Category = "Camera" };


        /// <summary>
        /// Defines how fast the camera position changes (using the keyboard) .
        /// </summary>
        public readonly FloatApplicationField WalkingSpeed = new FloatApplicationField(3) {Category = "Camera"};

        /// <summary>
        /// Determines if the camera should zoom on the results of a new source/graph.
        /// </summary>
        public readonly BoolApplicationField FrameNewData = new BoolApplicationField(true) { Category = "Camera" };



        public Renderer3DSettings()
            : base("Renderer3D")
        {
        }



#if WINDOWS
        /// <summary>
        /// Toggles the edge filter on the 3D View, which highlights silhouette and crease edges.
        /// </summary>
        public readonly BoolApplicationField Edge = new BoolApplicationField(true) {Category = "Filters", AllowsPreview = true };

        /// <summary>
        /// Toggles Screen Space Ambient Occlusion (SSAO) filter on the 3D View.
        /// </summary>
        public readonly BoolApplicationField Ssao = new BoolApplicationField(true) {Category = "Filters", AllowsPreview = true };

        /// <summary>
        /// Toggles the Fast Approximate Anti-Aliasing technique on the 3D View, reducing image aliasing.
        /// </summary>
        public readonly BoolApplicationField Fxaa = new BoolApplicationField(true) {Category = "Filters", AllowsPreview = true };

        /// <summary>
        /// Toggles the Enhanced Subpixel Morphological Anti-Aliasing on the 3D View, reducing image aliasing.
        /// </summary>
        public readonly BoolApplicationField Smaa = new BoolApplicationField(true) {Category = "Filters", AllowsPreview = true };

        /// <summary>
        /// Toggles a Vignette Image effect.
        /// </summary>
        public readonly BoolApplicationField Vignette = new BoolApplicationField(false) { Category = "Filters", AllowsPreview = true };


        public readonly BoolApplicationField ColorCorrection = new BoolApplicationField(false) { Category = "Filters", AllowsPreview = true };


        public readonly BoolApplicationField HDR = new BoolApplicationField(false) { Category = "Filters", AllowsPreview = true };
#endif
    }
}