using Assets.Sceelix.Contexts;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    public abstract class ComponentProcessor
    {
        public abstract void Process(IGenerationContext context, GameObject gameObject, JToken jtoken);
    }
}
