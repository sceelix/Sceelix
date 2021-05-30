using System;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    public abstract class MaterialProcessor
    {
        public abstract Material Process(IGenerationContext context, JToken jtoken);
    }
}
