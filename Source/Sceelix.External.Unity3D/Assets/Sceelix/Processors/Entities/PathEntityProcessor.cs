using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Entities;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Processor("PathEntity")]
public class PathEntityProcessor : EntityProcessor
{
    public override IEnumerable<GameObject> Process(IGenerationContext context, JToken entityToken)
    {
        GameObject gameObject = new GameObject("Path Entity");
        gameObject.isStatic = true;

        //fill in the name, static, enabled, tag and layer fields
        ProcessCommonUnityAttributes(context, gameObject, entityToken);

        var pathVertices = entityToken["PathVertices"].Children().Select(x => x.ToVector3()).ToArray();
        
        List<KeyValuePair<int, int>> edges = new List<KeyValuePair<int, int>>();
        foreach (var pathEdgeToken in entityToken["PathEdges"].Children())
        {
            int sourceIndex = Convert.ToInt32(pathEdgeToken["Source"], CultureInfo.InvariantCulture);
            int targetIndex = Convert.ToInt32(pathEdgeToken["Target"], CultureInfo.InvariantCulture);
            edges.Add(new KeyValuePair<int, int>(sourceIndex, targetIndex));
        }
        
        var lineDrawComponent = gameObject.AddComponent<LineDrawComponent>();
        lineDrawComponent.Vertices = pathVertices;
        lineDrawComponent.Edges = edges;
        
        yield return gameObject;
    }
}
