using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("RigidBody")]
    public class RigidBodyProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();

            //if a rigidbody already exists, this will be null, so don't overwrite it
            if (rigidbody == null)
                return;

            rigidbody.mass = jtoken["Properties"]["Mass"].ToObject<float>();
            rigidbody.drag = jtoken["Properties"]["Drag"].ToObject<float>();
            rigidbody.angularDrag = jtoken["Properties"]["Angular Drag"].ToObject<float>();
            rigidbody.useGravity = jtoken["Properties"]["Use Gravity"].ToObject<bool>();
            rigidbody.isKinematic = jtoken["Properties"]["Is Kinematic"].ToObject<bool>();
            rigidbody.interpolation = jtoken["Properties"]["Interpolate"].ToEnum<RigidbodyInterpolation>();
            rigidbody.collisionDetectionMode = jtoken["Properties"]["Collision Detection"].ToEnum<CollisionDetectionMode>();
        }
    }
}