using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Unity.Data
{
    [Entity("Unity Entity")]
    public class UnityEntity : Entity, IActor
    {
        public UnityEntity()
        {
            BoxScope = BoxScope.Identity;
        }



        public BoxScope BoxScope
        {
            get;
            set;
        }


        public bool Enabled
        {
            get;
            set;
        } = true;


        public List<UnityComponent> GameComponents
        {
            get;
            private set;
        } = new List<UnityComponent>();


        public string Layer
        {
            get;
            set;
        }


        public string Name
        {
            get;
            set;
        } = "Sceelix Unity Entity";


        public string Positioning
        {
            get;
            set;
        }


        public string Prefab
        {
            get;
            set;
        }


        public Vector3D RelativeScale
        {
            get;
            set;
        } = new Vector3D(1, 1, 1);


        public Vector3D Scale => (RelativeScale * BoxScope.Sizes).ReplaceValue(0, 1);


        public string ScaleMode
        {
            get;
            set;
        }


        public bool Static
        {
            get;
            set;
        }


        public string Tag
        {
            get;
            set;
        }



        public override IEntity DeepClone()
        {
            UnityEntity unityEntity = (UnityEntity) base.DeepClone();

            unityEntity.GameComponents = GameComponents.Select(x => (UnityComponent) x.DeepClone()).ToList();

            return unityEntity;
        }



        public void InsertInto(BoxScope target)
        {
            BoxScope = target;
        }
    }
}