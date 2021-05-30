using System.Collections.Generic;
using DigitalRune.Physics;
using Sceelix.Annotations;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Unity3D.GameObjects.Components;
using Sceelix.Unity.Data;

namespace Sceelix.Designer.Unity3D.Translators
{
    [StringKey("RigidBody")]
    public class PhysicsComponentTranslator : ComponentTranslator
    {
        public override IEnumerable<EntityObjectComponent> Process(UnityEntity unityEntity, UnityComponent unityComponent, EntityObjectDomain entityObjectDomain)
        {
            yield return new PhysicsComponent(new RigidBody() {MotionType = MotionType.Dynamic});
        }


        public override void PostProcess(IEnumerable<EntityObject> entityObjects, EntityObjectDomain contentLoader)
        {
        }


        public override bool IsDrawable
        {
            get { return false; }
        }
    }
}