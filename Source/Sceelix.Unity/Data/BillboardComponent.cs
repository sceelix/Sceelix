using Sceelix.Meshes.Data;

namespace Sceelix.Unity.Data
{
    public class BillboardComponent : UnityComponent
    {
        public BillboardComponent(BillboardEntity billboardEntity)
        {
            BillboardEntity = billboardEntity;
        }



        public BillboardEntity BillboardEntity
        {
            get;
        }
    }
}