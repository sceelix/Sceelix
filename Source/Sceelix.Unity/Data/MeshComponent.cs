using Sceelix.Meshes.Data;

namespace Sceelix.Unity.Data
{
    public class MeshComponent : UnityComponent
    {
        public MeshComponent(MeshEntity meshEntity)
        {
            MeshEntity = meshEntity;
        }



        public MeshEntity MeshEntity
        {
            get;
        }
    }
}