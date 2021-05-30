using System.Collections.Generic;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Components;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Entities
{
    [Processor("SurfaceEntity")]
    public class SurfaceEntityProcessor : EntityProcessor
    {
        private TerrainProcessor _terrainProcessor = new TerrainProcessor();

        public override IEnumerable<GameObject> Process(IGenerationContext context, JToken entityToken)
        {
            GameObject gameObject = new GameObject("Surface Entity");

            //fill in the name, static, enabled, tag and layer fields
            ProcessCommonUnityAttributes(context, gameObject, entityToken);

            gameObject.transform.position = entityToken["Position"].ToVector3();

            //use the processors already defined for the component
            _terrainProcessor.Process(context, gameObject, entityToken);

            yield return gameObject;
        }
    }
}