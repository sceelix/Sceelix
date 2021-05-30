using System;
using System.Collections.Generic;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Components;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Entities
{
    [Processor("MeshEntity")]
    public class MeshEntityProcessor : EntityProcessor
    {
        readonly MeshFilterProcessor _meshFilterProcessor = new MeshFilterProcessor();
        readonly MeshRendererProcessor _meshRendererProcessor = new MeshRendererProcessor();

        public override IEnumerable<GameObject> Process(IGenerationContext context, JToken entityToken)
        {
            GameObject gameObject = new GameObject("Mesh Entity");
            gameObject.isStatic = true;

            //fill in the name, static, enabled, tag and layer fields
            ProcessCommonUnityAttributes(context, gameObject, entityToken);

            //use the processors already defined for the component
            _meshFilterProcessor.Process(context, gameObject, entityToken["MeshFilter"]);
            _meshRendererProcessor.Process(context, gameObject, entityToken["MeshRenderer"]);

            yield return gameObject;
        }
    }
}