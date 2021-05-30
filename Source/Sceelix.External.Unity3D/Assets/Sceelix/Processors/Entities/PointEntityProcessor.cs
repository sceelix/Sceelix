using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Sceelix.Components;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Entities;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

[Processor("PointEntity")]
public class PointEntityProcessor : EntityProcessor
{
    public override IEnumerable<GameObject> Process(IGenerationContext context, JToken entityToken)
    {
        GameObject gameObject = new GameObject("Point Entity");
        gameObject.isStatic = true;

        //fill in the name, static, enabled, tag and layer fields
        ProcessCommonUnityAttributes(context, gameObject, entityToken);

        gameObject.transform.position = entityToken["Position"].ToVector3();

        var newMesh = context.CreateOrGetAssetOrResource<Mesh>("__PointEntityMesh__.asset", () => MeshHelper.CreateQuad(1,1));
        var newMaterial = context.CreateOrGetAssetOrResource<Material>("__PointEntityMaterial__.mat", () => new Material(Shader.Find("Sprites/Default")){color = Color.white});

        gameObject.AddComponent<MeshFilter>().mesh = newMesh;
        gameObject.AddComponent<MeshRenderer>().material = newMaterial;
        gameObject.AddComponent<BillboardComponent>();
        
        yield return gameObject;
    }
}
