using Sceelix.Meshes.Data;

namespace Sceelix.Unity.Data
{
    public class MeshInstanceComponent : UnityComponent
    {
        public MeshInstanceComponent(MeshInstanceEntity meshInstanceEntity)
        {
            MeshInstanceEntity = meshInstanceEntity;
        }



        public MeshInstanceEntity MeshInstanceEntity
        {
            get;
        }
    }
}