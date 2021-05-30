using DigitalRune.Graphics.SceneGraph;
using Sceelix.Designer.Meshes;
using Sceelix.Designer.Meshes.SceneNodes;
using Sceelix.Designer.Renderer3D.GameObjects;

namespace Sceelix.Designer.Unity3D.GameObjects.Components
{
    public class StaticSceneComponent : EntityObjectComponent
    {
        private readonly SceneNode _sceneNode;

        public StaticSceneComponent(MeshRenderNode meshRenderNode)
        {
            _sceneNode = meshRenderNode;
        }



        public SceneNode SceneNode
        {
            get { return _sceneNode; }
        }
    }
}