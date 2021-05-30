using DigitalRune.Game;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    // Adds distance and height-based fog. Fog is disabled by default.
    public class FogObject : GameObject
    {
        private readonly IServiceLocator _services;

        // Optionally, we can move the fog node with the camera node. If camera and 
        // fog are independent, then the camera can fly up and "escape" the height-based 
        // fog. If camera and fog move together, then the fog will always have the
        // same height at the horizon (e.g. to hide the horizon).
        private readonly bool _attachToCamera;



        public FogObject(IServiceLocator services)
            : this(services, false)
        {
        }



        public FogObject(IServiceLocator services, bool attachToCamera)
        {
            _services = services;
            _attachToCamera = attachToCamera;
            Name = "Fog";
        }



        public FogNode FogNode
        {
            get;
            private set;
        }



        // OnLoad() is called when the GameObject is added to the IGameObjectService.
        protected override void OnLoad()
        {
            FogNode = new FogNode(new Fog())
            {
                IsEnabled = true,
                Name = "Fog",
            };

            var scene = _services.Get<IScene>();
            if (!_attachToCamera)
            {
                scene.Children.Add(FogNode);
            }
            else
            {
                var cameraNode = ((Scene) scene).GetSceneNode("PlayerCamera");
                if (cameraNode.Children == null)
                    cameraNode.Children = new SceneNodeCollection();

                cameraNode.Children.Add(FogNode);
            }

            // Add GUI controls to the Options window.
            /*var sampleFramework = _services.Get<SampleFramework>();
            var optionsPanel = sampleFramework.AddOptions("Game Objects");
            var panel = SampleHelper.AddGroupBox(optionsPanel, "FogObject");

            SampleHelper.AddCheckBox(
              panel,
              "Enable fog",
              FogNode.IsEnabled,
              isChecked => FogNode.IsEnabled = isChecked);

            SampleHelper.AddSlider(
              panel,
              "Fog ramp start",
              "F2",
              0,
              100,
              FogNode.Fog.Start,
              value => FogNode.Fog.Start = value);

            SampleHelper.AddSlider(
              panel,
              "Fog ramp end",
              "F2",
              0,
              100,
              FogNode.Fog.End,
              value => FogNode.Fog.End = value);

            SampleHelper.AddSlider(
              panel,
              "Density",
              "F2",
              0.01f,
              2,
              FogNode.Fog.Density,
              value => FogNode.Fog.Density = value);

            SampleHelper.AddSlider(
              panel,
              "Height falloff",
              "F2",
              -5,
              5,
              FogNode.Fog.HeightFalloff,
              value => FogNode.Fog.HeightFalloff = value);

            SampleHelper.AddSlider(
              panel,
              "Height Y",
              "F2",
              0,
              3,
              FogNode.PoseWorld.Position.Y,
              value =>
              {
                var pose = FogNode.PoseWorld;
                pose.Position.Y = value;
                FogNode.PoseWorld = pose;
              });*/
        }



        // OnUnload() is called when the GameObject is removed from the IGameObjectService.
        protected override void OnUnload()
        {
            FogNode.Parent.Children.Remove(FogNode);
            FogNode.Dispose(false);
            FogNode = null;
        }
    }
}