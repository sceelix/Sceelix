using DigitalRune.Graphics.SceneGraph;

namespace Sceelix.Designer.Renderer3D.GameObjects.Components
{
    
    public class SceneComponent : EntityObjectComponent
    {
        private readonly SceneNode _sceneNode;



        public SceneComponent(SceneNode sceneNode)
        {
            _sceneNode = sceneNode;
        }



        public override void OnLoad()
        {
            EntityObjectDomain.SceneNodes.Add(_sceneNode);
        }



        public bool IsStatic
        {
            get { return _sceneNode.IsStatic; }
        }



        public SceneNode SceneNode
        {
            get { return _sceneNode; }
        }
    }
}