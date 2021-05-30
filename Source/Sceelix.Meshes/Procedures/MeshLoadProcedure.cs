using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assimp;
using Assimp.Configs;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Logging;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Extensions;
using Sceelix.Meshes.IO;
using Sceelix.Meshes.Materials;
using Face = Sceelix.Meshes.Data.Face;
using Vector2D = Sceelix.Mathematics.Data.Vector2D;
using Vector3D = Sceelix.Mathematics.Data.Vector3D;

#if LINUX
using System.Reflection;
#endif

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Loads Meshes from disk from standard formats.
    /// </summary>
    /// <seealso cref="Sceelix.Core.Procedures.SystemProcedure" />
    [Procedure("3b9b4a33-8538-4760-85b1-b5e58e6aa148", Label = "Mesh Load", Category = "Mesh")]
    public class MeshLoadProcedure : SystemProcedure
    {
        /// <summary>
        /// The loaded mesh entity.
        /// </summary>
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// Name of the file to load.
        /// </summary>
        private readonly FileParameter _parameterFileName = new FileParameter("File Name", "")
        {
            ExtensionFilter = new[]
            {
                "3D GameStudio (3DGS Terrain) |.hmp",
                "3D GameStudio (3DGS) |.mdl",
                "3ds Max 3DS |.3ds",
                "3ds Max ASE |.ase",
                "AC3D |.ac",
                "AutoCAD DXF (*) |.dxf",
                "Autodesk |.fbx",
                "Biovision BVH |.bvh",
                "Blender 3D |.blend",
                "BlitzBasic 3D |.b3d",
                "CharacterStudio Motion (*) |.csm",
                "Collada |.dae",
                "DirectX X |.x",
                "Doom 3 |.md5",
                "glTF |.gltf, .glb",
                "Industry Foundation Classes (IFC/Step) |.ifc",
                "Irrlicht Mesh |.irrmesh",
                "Irrlicht Scene (*) |.irr",
                "Izware Nendo |.ndo",
                "LightWave |.lwo",
                "LightWave Scene |.lws",
                "Milkshape 3D |.ms3d",
                "Modo |.lxo",
                "Neutral File Format |.nff",
                "Object File Format |.off",
                "Ogre XML |.xml",
                "Open Game Engine Exchange |.ogex",
                "PovRAY Raw |.raw",
                "Quake I |.mdl",
                "Quake II |.md2",
                "Quake III Map/BSP |.pk3",
                "Quake III Mesh |.md3",
                "Quick3D |.q3d,.q3s",
                "Return to Castle Wolfenstein (*) |.mdc",
                "Sense8 WorldToolKit |.nff",
                "Stanford Polygon Library |.ply",
                "Stereolithography |.stl",
                "Terragen Terrain |.ter",
                "TrueSpace (*) |.cob,.scn",
                "Unreal (*) |.3d",
                "Valve Model (*) |.smd,.vta",
                "Wavefront Object |.obj",
                "XGL |.xgl,.zgl"
            }
        };


        /// <summary>
        /// Rotates the loaded meshes by 90º around the X-Axis. 
        /// Useful to compensate for differences in the Up vector, since many systems use Y-Up, while Sceelix uses Z-up. 
        /// </summary>
        private readonly BoolParameter _parameterRotateAxis = new BoolParameter("Rotate Axis", true);

        /// <summary>
        /// Flips all mesh faces. Useful to compensate for differences in face winding order (clockwise vs. counterclockwise).
        /// </summary>
        private readonly BoolParameter _parameterFlipFaces = new BoolParameter("Flip Faces", false);

        /// <summary>
        /// Flips the V coordinate for all texture coordinates. Useful to compensate for differences in UV setup (V-Up or V-Down).
        /// </summary>
        private readonly BoolParameter _parameterFlipTexture = new BoolParameter("Flip Texture", false);

        /// <summary>
        /// Indicates if the materials should also be loaded. Default is True.
        /// </summary>
        private readonly BoolParameter _parameterLoadMaterials = new BoolParameter("Load Materials", true);

        /// <summary>
        /// Indicates if the referenced files/textures, if defined with absolute paths, should be reset and made relative to the defined "Texture Folder" parameter.
        /// This is useful when the models were save with absolute paths referencing paths from other computers.
        /// </summary>
        private readonly BoolParameter _parameterResetAbsolutePaths = new BoolParameter("Reset Absolute Paths", false);

        /// <summary>
        /// Folder name/path, relative to the file, where the textures can be found. Useful to compensate for possible mismatches between
        /// the locations indicated in the imported file and the actual texture locations.
        /// For example: "Textures", "../Images", "Files/Textures", etc.
        /// </summary>
        private readonly StringParameter _textureFolder = new StringParameter("Texture Folder", "");


        /// <summary>
        /// Name of the attribute that should store the "Name" of the mesh.
        /// </summary>
        private readonly AttributeParameter<string> _attributeName = new AttributeParameter<string>("Name", AttributeAccess.Write);



        protected override void Run()
        {
#if LINUX
            LoadAssimpLibrary();
#endif

            //Create a new importer
            using (AssimpContext importer = new AssimpContext())
            {
                //This is how we add a configuration (each config is its own class)
                importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
                importer.SetConfig(new FBXImportAllMaterialsConfig(true));
                //importer.SetConfig(new FBXImportAllGeometryLayersConfig(true));
                //importer.SetConfig(new FBXPreservePivotsConfig(true));


                var extension = Path.GetExtension(_parameterFileName.Value);

                var textureDirectory = Path.Combine(Path.GetDirectoryName(_parameterFileName.Value), _textureFolder.Value);
                var file = Path.GetFileName(_parameterFileName.Value);

                //var directory = Path.Combine(Path.GetDirectoryName(Environment.GetFullPath(_parameterFileName.Value)), _textureFolder.Value);

                importer.SetIOSystem(new CustomIOSystem(Resources, Path.GetDirectoryName(_parameterFileName.Value)));
                //Import the model. All configs are set. The model
                //is imported, loaded into managed memory. Then the unmanaged memory is released, and everything is reset.
                //Scene scene = importer.ImportFileFromStream(new FileStream(Environment.GetFullPath(_parameterFileName.Value),FileMode.Open),".obj");//Environment.Load<Stream>(
                Scene scene = importer.ImportFile(file, PostProcessPreset.TargetRealTimeMaximumQuality);

                MeshEntity[] loadedMeshes = new MeshEntity[scene.MeshCount];
                BoxScope[] originalScopes = new BoxScope[scene.MeshCount];

                //determines the base transformation
                Matrix4x4 startMatrix = _parameterRotateAxis.Value ? Matrix4x4.FromRotationX(MathHelper.ToRadians(90)) : Matrix4x4.Identity;

                ImportNode(scene, loadedMeshes, originalScopes, scene.RootNode, startMatrix, textureDirectory, _parameterResetAbsolutePaths.Value, _parameterFlipTexture.Value);
            }
        }



        private void ImportNode(Scene scene, MeshEntity[] loadedMeshes, BoxScope[] originalScopes, Node node, Matrix4x4 cummulatedMatrix, string textureDirectory, bool resetAbsolutePaths, bool flipCoordinates)
        {
            var currentTransform = node.Transform * cummulatedMatrix;

            var sceelixMatrix = currentTransform.ToSceelixMatrix();

            if (node.HasMeshes)
                foreach (int meshIndex in node.MeshIndices)
                    try
                    {
                        Mesh mesh = scene.Meshes[meshIndex];
                        MeshEntity meshEntity = loadedMeshes[meshIndex];

                        //if the mesh has already been processed before, create a clone and transform it
                        if (meshEntity != null)
                        {
                            meshEntity = (MeshEntity) meshEntity.DeepClone();
                        }
                        else
                        {
                            meshEntity = loadedMeshes[meshIndex] = ProcessMesh(scene, mesh, textureDirectory, flipCoordinates, resetAbsolutePaths);

                            //the next line may change the boxscope, so let's store the value (it's a struct)
                            originalScopes[meshIndex] = meshEntity.BoxScope;

                            //set the name of the object
                            _attributeName[meshEntity] = node.Name;
                        }

                        //if the current transform is actually relevant
                        if (!currentTransform.IsIdentity)
                            meshEntity.InsertInto(originalScopes[meshIndex].Transform(sceelixMatrix));

                        _output.Write(meshEntity);
                    }
                    catch (Exception)
                    {
                        Logger.Log("Could not load some geometry meshes, as they were deemed invalid.", LogType.Warning);
                    }

            if (node.HasChildren)
                foreach (Node child in node.Children)
                    ImportNode(scene, loadedMeshes, originalScopes, child, currentTransform, textureDirectory, resetAbsolutePaths, flipCoordinates);
        }



        private MeshEntity ProcessMesh(Scene scene, Mesh mesh, string directory, bool flipCoordinates, bool resetAbsolutePaths)
        {
            var vertices = mesh.Vertices.Select(vertex => new Vertex(new Vector3D(vertex.X, vertex.Y, vertex.Z))).ToList(); //.Rotate(Vector3D.XVector, MathHelper.PiOver2)   .FlipYZ()
            var normals = mesh.Normals.Select(vector => new Vector3D(vector.X, vector.Y, vector.Z)).ToList();
            var bitangents = mesh.BiTangents.Select(vector => new Vector3D(vector.X, vector.Y, vector.Z)).ToList();
            var tangents = mesh.Tangents.Select(vector => new Vector3D(vector.X, vector.Y, vector.Z)).ToList();
            var texCoordChannels = mesh.TextureCoordinateChannels.Select(channel => channel.Select(texCoord => new Vector2D(texCoord.X, flipCoordinates ? 1 - texCoord.Y : texCoord.Y)).ToList()).ToList();

            var material = new ImportedMaterial(scene.Materials[mesh.MaterialIndex], directory, resetAbsolutePaths);

            List<Face> faces = new List<Face>(mesh.Faces.Count);

            foreach (var face in mesh.Faces)
                try
                {
                    var indices = face.Indices;

                    if (_parameterFlipFaces.Value)
                        indices.Reverse();

                    var sceelixFace = new Face(indices.Select(val => vertices[val]));

                    var halfVertexList = sceelixFace.HalfVertices.ToList();
                    for (int i = 0; i < halfVertexList.Count; i++)
                    {
                        var halfVertex = halfVertexList[i];
                        int assimpfaceIndex = indices[i];

                        if (mesh.HasNormals)
                            halfVertex.Normal = normals[assimpfaceIndex];

                        for (int j = 0; j < mesh.TextureCoordinateChannelCount; j++) halfVertex.UVs[j] = texCoordChannels[j][assimpfaceIndex];

                        if (mesh.HasTangentBasis)
                        {
                            halfVertex.Tangent = tangents[assimpfaceIndex];
                            halfVertex.Binormal = bitangents[assimpfaceIndex];
                        }
                    }

                    if (_parameterLoadMaterials.Value)
                        sceelixFace.Material = material;

                    faces.Add(sceelixFace);
                }
                catch (Exception)
                {
                    Logger.Log("Could not load some geometry faces, as they were deemed invalid.", LogType.Warning);
                }


            MeshEntity meshEntity = new MeshEntity(faces);

            return meshEntity;
        }



#if LINUX
        private void LoadAssimpLibrary()
        {
            var targetDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

            try
            {
                AssimpLibrary.Instance.LoadLibrary(Path.Combine(targetDir, "libassimp.so"));
                /*AssimpLibrary.Instance.LoadLibrary(
                    Path.Combine(targetDir, "libassimp_32.so.3.0.1"),
                    Path.Combine(targetDir, "libassimp_64.so.3.0.1"));*/
            }
            catch (AssimpException ex)
            {
                if (ex.Message == "Assimp library already loaded.")
                    return;
                throw;
            }
        }
#endif
    }
}