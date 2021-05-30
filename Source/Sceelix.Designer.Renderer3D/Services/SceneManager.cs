using DigitalRune.Graphics.SceneGraph;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Renderer3D.Services
{
    public class SceneManager
    {
        private IScene scene;
        public static Synchronizer Synchronizer = new Synchronizer();

        public SceneManager(IScene scene)
        {
            this.scene = scene;
        }


        public void Add(SceneNode sceneNode)
        {
            Synchronizer.Enqueue(() => scene.Children.Add(sceneNode));
        }
    }
}
