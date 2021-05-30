using System;
using DigitalRune.Graphics.SceneGraph;

namespace Sceelix.Designer.Renderer3D.SceneNodes
{
    public class DynamicNode : SceneNode
    {
        public delegate void UpdateFunction(TimeSpan deltaTime);

        public event UpdateFunction Updated = delegate { };



        internal void Update(TimeSpan deltaTime)
        {
            Updated.Invoke(deltaTime);
        }
    }
}