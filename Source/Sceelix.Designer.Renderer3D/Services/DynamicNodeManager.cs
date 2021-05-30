using System;
using System.Linq;
using DigitalRune.Graphics.SceneGraph;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.GraphicsScreens;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.SceneNodes;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.Services
{
    [Renderer3DService]
    public class DynamicNodeManager : IServiceable, IUpdateableElement
    {
        private ICustomGraphicsScreen _graphicsScreen;


        public void Initialize(IServiceLocator services)
        {
            _graphicsScreen = services.Get<ICustomGraphicsScreen>();
        }
        


        public void Update(TimeSpan deltaTime)
        {
            foreach (DynamicNode dynamicNode in _graphicsScreen.Scene.GetSubtree().OfType<DynamicNode>())
                dynamicNode.Update(deltaTime);
        }
    }
}
