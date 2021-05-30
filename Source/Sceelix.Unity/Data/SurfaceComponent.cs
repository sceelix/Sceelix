using System.Collections.Generic;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Unity.Data
{
    public class TreeInstance
    {
        public TreeInstance(BoxScope scope, string prefab)
        {
            Scope = scope;
            Prefab = prefab;
        }



        public float BendFactor
        {
            get;
            set;
        }


        public string Prefab
        {
            get;
        }


        public float Rotation
        {
            get;
            set;
        }


        public Vector2D Scale
        {
            get;
            set;
        }


        public BoxScope Scope
        {
            get;
        }
    }

    public class SurfaceComponent : UnityComponent
    {
        public SurfaceComponent(SurfaceEntity surfaceEntity) : base("Surface")
        {
            SurfaceEntity = surfaceEntity;
        }



        public SurfaceEntity SurfaceEntity
        {
            get;
        }


        public List<TreeInstance> TreeInstances
        {
            get;
        } = new List<TreeInstance>();
    }
}