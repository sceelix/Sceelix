using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Terrain")]
    public class TerrainProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            //if a Terrain already exists, don't overwrite it
            if (gameObject.GetComponent<Terrain>() != null)
                return;

            
            var resolution = jtoken["Resolution"].ToObject<int>();
            var sizes = jtoken["Size"].ToVector3();

            var heightsResolution = jtoken["HeightsResolution"].ToVector2();
            var heightsBytes = jtoken["Heights"].ToObject<byte[]>();
            var heights = heightsBytes.ToTArray<float>((int)heightsResolution.x, (int)heightsResolution.y);

            //initialize the terrain data instance and set height data
            //unfortunately unity terrain maps have to be square and the sizes must be powers of 2
            TerrainData terrainData = new TerrainData();

            
            terrainData.heightmapResolution = resolution;
            terrainData.alphamapResolution = resolution;
            terrainData.size = sizes;
            terrainData.SetHeights(0, 0, heights);
            

            var materialToken = jtoken["Material"];
            if (materialToken != null)
            {
                var defaultTexture = Texture2D.whiteTexture.ToMipmappedTexture();
                List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

                var splatMapSize = materialToken["SplatmapSize"].ToVector3();
                var splatMapBytes = materialToken["Splatmap"].ToObject<byte[]>();
                float[,,] splatMap = splatMapBytes.ToTArray<float>((int)splatMapSize.x, (int)splatMapSize.y, (int)splatMapSize.z);
                
                foreach (JToken layerToken in materialToken["Layers"].Children())
                {
                    var tileSize = layerToken["TileSize"].ToVector2();
                    var textureToken = layerToken["Texture"];

                    //var name = textureToken["Name"].ToObject<String>();

                    terrainLayers.Add(new TerrainLayer()
                    {
                        diffuseTexture = Texture2DExtensions.CreateOrGetTexture(context, layerToken["DiffuseMap"]) ?? defaultTexture,
                        normalMapTexture = Texture2DExtensions.CreateOrGetTexture(context, layerToken["NormalMap"], true),
                        tileSize = new Vector2(terrainData.size.x / tileSize.x, terrainData.size.z / tileSize.y)
                    });
                }

                terrainData.terrainLayers = terrainLayers.ToArray();

                terrainData.SetAlphamaps(0, 0, splatMap);
            }

            //finally, create the terrain components
            Terrain terrain = gameObject.AddComponent<Terrain>();
            TerrainCollider collider = gameObject.AddComponent<TerrainCollider>();
            
            terrain.materialTemplate = new Material(Shader.Find("Nature/Terrain/Standard"));
            
            terrain.terrainData = terrainData;
            collider.terrainData = terrainData;

            ReadTerrainParameters(context, terrain, jtoken);
            ReadTreeInstances(context, terrain, jtoken);
        }


        private void ReadTerrainParameters(IGenerationContext context, Terrain terrain, JToken jtoken)
        {
            var pixelErrorToken = jtoken["PixelError"];
            if(pixelErrorToken != null)
                terrain.heightmapPixelError = pixelErrorToken.ToTypeOrDefault<int>();

            var billboardStartToken = jtoken["BillboardStart"];
            if(billboardStartToken != null)
                terrain.treeBillboardDistance = billboardStartToken.ToTypeOrDefault<float>();
        }


        private void ReadTreeInstances(IGenerationContext context, Terrain terrain, JToken jtoken)
        {
            //add tree instances, if defined
            var treeInstancesToken = jtoken["TreeInstances"];
            if (treeInstancesToken != null)
            {
                List<TreePrototype> treePrototypes = new List<TreePrototype>();
                List<TreeInstance> treeInstances = new List<TreeInstance>();
                
                foreach (JToken treeInstanceToken in treeInstancesToken.Children())
                {
                    var prefabPath = treeInstanceToken["Prefab"].ToObject<String>();
                    var bendFactor = treeInstanceToken["BendFactor"].ToObject<float>();
                    
                    if (!prefabPath.StartsWith("Assets/"))
                        prefabPath = "Assets/" + prefabPath;

                    //make sure the extension is set
                    prefabPath = Path.ChangeExtension(prefabPath, ".prefab");

                    var gameObject = context.InstantiatePrefab(prefabPath);
                    
                    var currentPrototypeIndex = treePrototypes.Count;
                    
                    treePrototypes.Add(new TreePrototype
                    {
                        prefab = gameObject,
                        bendFactor = bendFactor
                    });
                    

                    foreach (var treeTransform in treeInstanceToken["Transforms"].Children())
                    {
                        var position = treeTransform["Position"].ToVector3();
                        var scale = treeTransform["Scale"].ToVector2();
                        var rotation = treeTransform["Rotation"].ToObject<float>();
                        treeInstances.Add(new TreeInstance()
                        {
                            rotation = rotation,
                            prototypeIndex = currentPrototypeIndex,
                            position = position,
                            widthScale = scale.x,
                            heightScale = scale.y,
                            color=Color.white,
                            lightmapColor = Color.white
                        });
                    }
                }

                terrain.terrainData.treePrototypes = treePrototypes.ToArray();
                
                foreach (var treeInstance in treeInstances)
                    terrain.AddTreeInstance(treeInstance);
            }
        }
    }
}