using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Sceelix.Actors.Data;
using Sceelix.Annotations;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resources;
using Sceelix.Extensions;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Materials;
using Color = System.Drawing.Color;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Saves meshes to disk in standard formats.
    /// </summary>
    [Procedure("ac217afa-a4f7-4aec-bcd3-9eb26f8a4aae", Label = "Mesh Save", Category = "Mesh")]
    public class MeshSaveProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (circle) input means that the node will be executed once per mesh. Useful when different meshes are meant to be sent to different files, in which case the file path should be set as an expression. <br/>
        /// Setting a <b>Collective</b> (square) input means that the node will be executed once for all meshes. Useful to export all meshes into one file.
        /// </summary>
        private readonly SingleOrCollectiveInputChoiceParameter<MeshEntity> _parameterInput = new SingleOrCollectiveInputChoiceParameter<MeshEntity>("Inputs", "Collective");

        /// <summary>
        /// The mesh that was saved to file.
        /// </summary>
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// The file format to export to.
        /// </summary>
        private readonly SelectListParameter<MeshSaveParameter> _parameterFormat = new SelectListParameter<MeshSaveParameter>("Format", "Obj");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterFormat.SubParameterLabels);



        protected override void Run()
        {
            var meshEntityList = _parameterInput.Read().ToList();

            foreach (MeshSaveParameter meshSaveParameter in _parameterFormat.Items)
                meshSaveParameter.Save(meshEntityList);

            _output.Write(meshEntityList);
        }



        #region Abstract Parameter

        public abstract class MeshSaveParameter : CompoundParameter
        {
            /// <summary>
            /// Indicates if the faces should be flipped. This may be required for exporting to certain platforms
            /// where the face orientation is different.
            /// </summary>
            protected readonly BoolParameter _parameterFlipFaces = new BoolParameter("Flip Faces", true);



            protected MeshSaveParameter(string label)
                : base(label)
            {
            }



            public abstract void Save(List<MeshEntity> meshEntities);
        }

        #endregion

        #region OBJ Save

        /// <summary>
        /// Saves meshes to the .OBJ file format.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshSaveProcedure.MeshSaveParameter" />
        public class ObjSaveParameter : MeshSaveParameter
        {
            /// <summary>
            /// Relative folder where the textures are saved.
            /// </summary>
            private const string TextureFolderName = "Textures";

            /// <summary>
            /// Specific functions that export materials
            /// </summary>
            private static readonly Dictionary<Type, IObjMaterialExporter> MaterialExporters = AttributeReader.OfTypeKeyAttribute<TypeKeyAttribute>().GetInstancesOfType<IObjMaterialExporter>();

            /// <summary>
            /// The attribute group name for mesh aggregation. Different values can be set for each mesh using the @@attributeName notation.
            /// </summary>
            private readonly StringParameter _attributeGroupName = new StringParameter("Group Name", "") {EntityEvaluation = true};

            /// <summary>
            /// Indicates if the materials should be exported, too (as a .MTL file).
            /// </summary>
            private readonly BoolParameter _exportMaterialsParameter = new BoolParameter("Export Materials", true);


            /// <summary>
            /// Location where to store the file.
            /// </summary>
            private readonly FileParameter _parameterFileLocation = new FileParameter("File Location", Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "MyFile.obj")) {ExtensionFilter = new[] {".obj"}, IOOperation = IOOperation.Save}; //, 


            /// <summary>
            /// Indicates if the Y/Z coordinates should be flipped. Useful when exporting to platforms where the Up axis is different.
            /// </summary>
            protected readonly BoolParameter _parameterFlipYZ = new BoolParameter("Flip YZ", true);



            static ObjSaveParameter()
            {
                /*MaterialExporters.Register<ColorMaterial>(x => new ColorNormalExporter());
                MaterialExporters.Register<SingleTextureMaterial>(x => new SingleTextureExporter());
                MaterialExporters.Register<ImportedMaterial>(x => new ImportedMaterialExporter());*/
            }



            protected ObjSaveParameter()
                : base("Obj")
            {
            }



            private void CreateMtlFle(string filePath, string texturesDirPath, Dictionary<Material, string> materials)
            {
                StringBuilder materialCode = new StringBuilder();
                IResourceManager resources = ProcedureEnvironment.GetService<IResourceManager>();

                foreach (KeyValuePair<Material, string> keyValuePair in materials)
                {
                    //TODO: Review this once it's done
                    IObjMaterialExporter objMaterialExporter = MaterialExporters[keyValuePair.Key.GetType()];

                    string code = objMaterialExporter.Export(keyValuePair.Key, resources, texturesDirPath, TextureFolderName);

                    materialCode.AppendLine("newmtl " + keyValuePair.Value);
                    materialCode.Append(code);
                    materialCode.AppendLine();
                }

                File.WriteAllText(filePath, materialCode.ToString());
            }



            public void Export(List<MeshEntity> meshEntities, string filePath)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string directoryPath = Path.GetDirectoryName(filePath);
                string texturesDirPath = Path.Combine(directoryPath, TextureFolderName);
                string materialFileName = fileName + ".mtl";


                if (!Directory.Exists(texturesDirPath))
                    Directory.CreateDirectory(texturesDirPath);

                string code = "#Sceelix OBJ Exporter - " + fileName + "\n";

                if (_exportMaterialsParameter.Value)
                    code += "mtllib " + materialFileName + "\n";

                StringBuilder positionsCode = new StringBuilder();
                StringBuilder normalsCode = new StringBuilder();
                StringBuilder textureUVsCode = new StringBuilder();
                StringBuilder facesCode = new StringBuilder();

                Dictionary<Vector3D, int> customPositionIndices = new Dictionary<Vector3D, int>();
                Dictionary<Vector3D, int> customNormalIndices = new Dictionary<Vector3D, int>();
                Dictionary<Vector2D, int> customTextureUVIndices = new Dictionary<Vector2D, int>();

                Dictionary<Material, string> _materials = new Dictionary<Material, string>();

                List<string> groupNames = new List<string>();

                int faceCount = 0;

                foreach (MeshEntity meshEntity in meshEntities)
                {
                    //get the name of the group, but make sure it isn't black
                    //string groupName = "Group";
                    string groupName = _attributeGroupName.Get(meshEntity);
                    if (string.IsNullOrWhiteSpace(groupName))
                        groupName = "Group";

                    //or make sure that it isn't repeated
                    groupName = GetNameSuggestion(groupNames, groupName);
                    groupNames.Add(groupName);

                    facesCode.AppendLine("g " + groupName);

                    faceCount += meshEntity.Faces.Count;


                    foreach (IGrouping<Material, Face> grouping in meshEntity.Faces.GroupBy(val => val.Material))
                    {
                        if (_exportMaterialsParameter.Value)
                        {
                            string materialName = string.Empty;

                            //save the material
                            if (!_materials.TryGetValue(grouping.Key, out materialName))
                            {
                                materialName = "Material_" + _materials.Count;
                                _materials.Add(grouping.Key, materialName);
                            }


                            facesCode.AppendLine("usemtl " + materialName);
                        }

                        //Load the vertices and triangles from each face
                        foreach (Face face in grouping)
                        {
                            if (face.Normal.IsNaN)
                                continue;

                            facesCode.Append("f");

                            List<Vertex> vertices = face.Vertices.ToList();

                            if (_parameterFlipFaces.Value)
                                vertices.Reverse();

                            foreach (Vertex vertex in vertices)
                            {
                                facesCode.Append(" ");

                                Vector3D position = vertex.Position;
                                Vector3D normal = vertex[face].Normal;
                                Vector2D textureUV = vertex[face].UV0; //.Fix()

                                if (_parameterFlipYZ.Value)
                                {
                                    position = position.FlipYZ();
                                    normal = normal.FlipYZ();
                                }

                                facesCode.Append(ListBuilder("v", vertex.Position, customPositionIndices, positionsCode, position.X, position.Y, position.Z));
                                facesCode.Append("/");
                                facesCode.Append(ListBuilder("vt", textureUV, customTextureUVIndices, textureUVsCode, textureUV.X, textureUV.Y));
                                facesCode.Append("/");
                                facesCode.Append(ListBuilder("vn", normal, customNormalIndices, normalsCode, normal.X, normal.Y, normal.Z));
                            }

                            facesCode.AppendLine();
                        }
                    }

                    facesCode.AppendLine();
                }

                code += "\n# " + customPositionIndices.Count + " Vertices\n" + positionsCode;
                code += "\n# " + customNormalIndices.Count + " Normals\n" + normalsCode;
                code += "\n# " + customTextureUVIndices.Count + " Texture Coordinates\n" + textureUVsCode;
                code += "\n# " + faceCount + " Faces\n" + facesCode;

                File.WriteAllText(_parameterFileLocation.Value, code);

                if (_exportMaterialsParameter.Value)
                    CreateMtlFle(Path.Combine(directoryPath, materialFileName), texturesDirPath, _materials);
            }



            public string GetNameSuggestion(List<string> groupNames, string mainSugggestion)
            {
                int index = 1;

                string modifiedSuggestion = mainSugggestion;
                while (groupNames.Any(val => val == modifiedSuggestion)) modifiedSuggestion = mainSugggestion + "0" + index++;

                return modifiedSuggestion;
            }



            private static int ListBuilder<T>(string initial, T vector, Dictionary<T, int> vector3DIndexer, StringBuilder codeBuilder, params float[] values)
            {
                int index;

                if (!vector3DIndexer.TryGetValue(vector, out index))
                {
                    index = vector3DIndexer.Count + 1;

                    vector3DIndexer.Add(vector, index);
                    codeBuilder.Append(initial);
                    //codeBuilder.Append(" ");

                    foreach (float value in values)
                    {
                        codeBuilder.Append(" ");
                        codeBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
                    }

                    codeBuilder.AppendLine();
                }

                return index;
            }



            public override void Save(List<MeshEntity> meshEntity)
            {
                Export(meshEntity, _parameterFileLocation.Value);
            }



            public interface IObjMaterialExporter
            {
                string Export(Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath);
            }

            [TypeKey(typeof(Material))]
            public class DefaultObjExporter : IObjMaterialExporter
            {
                public string Export(Material material, IResourceManager environment, string absoluteExportDirPath,
                    string relativeTextureFolderPath)
                {
                    throw new NotImplementedException(string.Format("The export of the material '{0}' is not (yet) supported.", material.Type));
                }
            }

            [TypeKey(typeof(ColorMaterial))]
            public class ColorNormalExporter : IObjMaterialExporter
            {
                public string Export(Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath)
                {
                    ColorMaterial textureMaterial = (ColorMaterial) material;
                    StringBuilder materialCode = new StringBuilder();

                    materialCode.AppendLine("Ka 1.000000 1.000000 1.000000");
                    materialCode.AppendLine("Kd 1.000000 1.000000 1.000000");
                    materialCode.AppendLine("Ks 0.000000 0.000000 0.000000");
                    //materialCode.AppendLine("Tr 0.000000");
                    materialCode.AppendLine("illum 1");
                    materialCode.AppendLine("Ns 0.000000");

                    string fileName = "ColorMaterial_" + textureMaterial.DefaultColor.R + "_" + textureMaterial.DefaultColor.G + "_" + textureMaterial.DefaultColor.B + ".png";

                    materialCode.AppendLine("map_Kd " + Path.Combine(relativeTextureFolderPath, fileName));

                    Bitmap bitmap = new Bitmap(1, 1);
                    bitmap.SetPixel(0, 0, Color.FromArgb(textureMaterial.DefaultColor.A, textureMaterial.DefaultColor.R, textureMaterial.DefaultColor.G, textureMaterial.DefaultColor.B));
                    bitmap.Save(Path.Combine(absoluteExportDirPath, fileName), ImageFormat.Png);


                    return materialCode.ToString();
                }
            }

            [TypeKey(typeof(SingleTextureMaterial))]
            public class SingleTextureExporter : IObjMaterialExporter
            {
                public string Export(Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath)
                {
                    SingleTextureMaterial textureMaterial = (SingleTextureMaterial) material;

                    StringBuilder materialCode = new StringBuilder();

                    materialCode.AppendLine("Ka 1.000000 1.000000 1.000000");
                    materialCode.AppendLine("Kd 1.000000 1.000000 1.000000");
                    materialCode.AppendLine("Ks 0.000000 0.000000 0.000000");
                    //materialCode.AppendLine("Tr 0.000000");
                    materialCode.AppendLine("illum 1");
                    materialCode.AppendLine("Ns 0.000000");

                    string fileName = Path.GetFileName(textureMaterial.Texture);
                    if (fileName != null)
                    {
                        materialCode.AppendLine("map_Kd " + Path.Combine(relativeTextureFolderPath, fileName));

                        File.Copy(environment.GetFullPath(textureMaterial.Texture), Path.Combine(absoluteExportDirPath, fileName), true);
                    }

                    return materialCode.ToString();
                }
            }

            [TypeKey(typeof(ImportedMaterial))]
            public class ImportedMaterialExporter : IObjMaterialExporter
            {
                public string Export(Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath)
                {
                    var importedMaterial = (ImportedMaterial) material;

                    if (importedMaterial.HasDiffuseTexture)

                    {
                        var singletextureHandler = new SingleTextureExporter();
                        return singletextureHandler.Export(new SingleTextureMaterial(importedMaterial.DiffuseTexturePath), environment, absoluteExportDirPath, relativeTextureFolderPath);
                    }

                    var colorMaterialHandler = new ColorNormalExporter();
                    return colorMaterialHandler.Export(new ColorMaterial(importedMaterial.ColorDiffuse), environment, absoluteExportDirPath, relativeTextureFolderPath);
                }
            }
        }

        #endregion

        #region FBX

        /// <summary>
        /// Saves meshes to the .FBX file format.
        /// </summary>
        public class FbxSaveParameter : MeshSaveParameter
        {
            /// <summary>
            /// Relative folder where the textures are saved.
            /// </summary>
            private const string TextureFolderName = "Textures";


            /// <summary>
            /// Specific functions that export materials
            /// </summary>
            private static readonly Dictionary<Type, IFbxSaveDataHandler> _materialExporterHandlers = AttributeReader.OfTypeKeyAttribute<TypeKeyAttribute>().GetInstancesOfType<IFbxSaveDataHandler>();

            /// <summary>
            /// Location where to store the file.
            /// </summary>
            private readonly FileParameter _parameterFileLocation = new FileParameter("File Location", Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "MyFile.fbx")) {ExtensionFilter = new[] {".fbx"}, IOOperation = IOOperation.Save}; //, 



            public FbxSaveParameter()
                : base("Fbx")
            {
            }



            public override void Save(List<MeshEntity> meshEntities)
            {
                string exportDirPath = Path.GetDirectoryName(_parameterFileLocation.Value);
                var resources = ProcedureEnvironment.GetService<IResourceManager>();
                //string texturesDirPath = Path.Combine(exportDirPath, TextureFolderName);

                StringBuilder documentBuilder = new StringBuilder();

                //the header comments
                documentBuilder.AppendLine("; FBX 7.4.0 project file");
                documentBuilder.AppendLine("; Copyright (C) 1997-2016 Autodesk Inc. and/or its licensors");
                documentBuilder.AppendLine("; All rights reserved.");
                documentBuilder.AppendLine("; ----------------------------------------------------");
                documentBuilder.AppendLine();

                //now, the header extension
                documentBuilder.AppendLine("FBXHeaderExtension:  {");
                documentBuilder.AppendTabs(1).AppendLine("FBXHeaderVersion: 1003");
                documentBuilder.AppendTabs(1).AppendLine("FBXVersion: 7400");

                var creationTime = DateTime.Now;
                documentBuilder.AppendTabs(1).AppendLine("CreationTimeStamp:  {");
                documentBuilder.AppendTabs(2).AppendLine("Version: 1000");
                documentBuilder.AppendTabs(2).AppendLine("Year: " + creationTime.Year);
                documentBuilder.AppendTabs(2).AppendLine("Month: " + creationTime.Month);
                documentBuilder.AppendTabs(2).AppendLine("Day: " + creationTime.Day);
                documentBuilder.AppendTabs(2).AppendLine("Hour: " + creationTime.Hour);
                documentBuilder.AppendTabs(2).AppendLine("Minute: " + creationTime.Minute);
                documentBuilder.AppendTabs(2).AppendLine("Second: " + creationTime.Second);
                documentBuilder.AppendTabs(2).AppendLine("Millisecond: " + creationTime.Millisecond);
                documentBuilder.AppendTabs(1).AppendLine("}");
                documentBuilder.AppendTabs(1).AppendLine("Creator: " + "FBX SDK/FBX Plugins version 2014.0.1".Quote());
                documentBuilder.AppendTabs(1).AppendLine("SceneInfo: \"SceneInfo::GlobalInfo\", \"UserData\" {");
                documentBuilder.AppendTabs(2).AppendLine("Type: \"UserData\"");
                documentBuilder.AppendTabs(2).AppendLine("Version: 100");
                documentBuilder.AppendTabs(2).AppendLine("MetaData:  {");
                documentBuilder.AppendTabs(3).AppendLine("Version: 100");
                documentBuilder.AppendTabs(3).AppendLine("Title: \"\"");
                documentBuilder.AppendTabs(3).AppendLine("Subject: \"\"");
                documentBuilder.AppendTabs(3).AppendLine("Author: \"\"");
                documentBuilder.AppendTabs(3).AppendLine("Keywords: \"\"");
                documentBuilder.AppendTabs(3).AppendLine("Revision: \"\"");
                documentBuilder.AppendTabs(3).AppendLine("Comment: \"\"");
                documentBuilder.AppendTabs(2).AppendLine("}");
                documentBuilder.AppendTabs(1).AppendLine("}");


                /*SceneInfo: "SceneInfo::GlobalInfo", "UserData" {
            Type: "UserData"
            Version: 100
            MetaData:  {
                Version: 100
                Title: ""
                Subject: ""
                Author: ""
                Keywords: ""
                Revision: ""
                Comment: ""
            }
            Properties70:  {
                P: "DocumentUrl", "KString", "Url", "", "E:\Desktop\CubeTextured3Ds.fbx"
                P: "SrcDocumentUrl", "KString", "Url", "", "E:\Desktop\CubeTextured3Ds.fbx"
                P: "Original", "Compound", "", ""
                P: "Original|ApplicationVendor", "KString", "", "", "Autodesk"
                P: "Original|ApplicationName", "KString", "", "", "3ds Max"
                P: "Original|ApplicationVersion", "KString", "", "", "2014"
                P: "Original|DateTime_GMT", "DateTime", "", "", "15/07/2016 15:54:54.633"
                P: "Original|FileName", "KString", "", "", "E:\Desktop\CubeTextured3Ds.fbx"
                P: "LastSaved", "Compound", "", ""
                P: "LastSaved|ApplicationVendor", "KString", "", "", "Autodesk"
                P: "LastSaved|ApplicationName", "KString", "", "", "3ds Max"
                P: "LastSaved|ApplicationVersion", "KString", "", "", "2014"
                P: "LastSaved|DateTime_GMT", "DateTime", "", "", "15/07/2016 15:54:54.633"
            }
        }
                 */
                documentBuilder.AppendLine("}");
                documentBuilder.AppendLine();

                documentBuilder.AppendLine("GlobalSettings:  {");
                documentBuilder.AppendTabs(1).AppendLine("Version: 1000");
                documentBuilder.AppendTabs(1).AppendLine("Properties70:  {");
                documentBuilder.AppendTabs(2).AppendLine("P: \"UpAxis\", \"int\", \"Integer\", \"\",2");
                documentBuilder.AppendTabs(2).AppendLine("P: \"UpAxisSign\", \"int\", \"Integer\", \"\",1");
                documentBuilder.AppendTabs(2).AppendLine("P: \"FrontAxis\", \"int\", \"Integer\", \"\",1");
                documentBuilder.AppendTabs(2).AppendLine("P: \"FrontAxisSign\", \"int\", \"Integer\", \"\",-1");
                documentBuilder.AppendTabs(2).AppendLine("P: \"CoordAxis\", \"int\", \"Integer\", \"\",0");
                documentBuilder.AppendTabs(2).AppendLine("P: \"CoordAxisSign\", \"int\", \"Integer\", \"\",1");
                documentBuilder.AppendTabs(2).AppendLine("P: \"OriginalUpAxis\", \"int\", \"Integer\", \"\",2");
                documentBuilder.AppendTabs(2).AppendLine("P: \"OriginalUpAxisSign\", \"int\", \"Integer\", \"\",1");
                documentBuilder.AppendTabs(2).AppendLine("P: \"UnitScaleFactor\", \"double\", \"Number\", \"\",0.1");
                documentBuilder.AppendTabs(2).AppendLine("P: \"OriginalUnitScaleFactor\", \"double\", \"Number\", \"\",0.1");
                documentBuilder.AppendTabs(2).AppendLine("P: \"AmbientColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
                documentBuilder.AppendTabs(2).AppendLine("P: \"DefaultCamera\", \"KString\", \"\", \"\", \"Producer Perspective\"");
                documentBuilder.AppendTabs(2).AppendLine("P: \"TimeMode\", \"enum\", \"\", \"\",6");
                documentBuilder.AppendTabs(2).AppendLine("P: \"TimeProtocol\", \"enum\", \"\", \"\",2");
                documentBuilder.AppendTabs(2).AppendLine("P: \"SnapOnFrameMode\", \"enum\", \"\", \"\",0");
                documentBuilder.AppendTabs(2).AppendLine("P: \"TimeSpanStart\", \"KTime\", \"Time\", \"\",0");
                documentBuilder.AppendTabs(2).AppendLine("P: \"TimeSpanStop\", \"KTime\", \"Time\", \"\",153953860000");
                documentBuilder.AppendTabs(2).AppendLine("P: \"CustomFrameRate\", \"double\", \"Number\", \"\",-1");
                documentBuilder.AppendTabs(2).AppendLine("P: \"TimeMarker\", \"Compound\", \"\", \"\"");
                documentBuilder.AppendTabs(2).AppendLine("P: \"CurrentTimeMarker\", \"int\", \"Integer\", \"\",-1");
                documentBuilder.AppendTabs(1).AppendLine("}");
                documentBuilder.AppendLine("}");
                documentBuilder.AppendLine();

                //the description of the scene and the root node
                documentBuilder.AppendLine("; Documents Description");
                documentBuilder.AppendLine(";------------------------------------------------------------------");
                documentBuilder.AppendLine();
                documentBuilder.AppendLine("Documents:  {");
                documentBuilder.AppendTabs(1).AppendLine("Count: 1");
                documentBuilder.AppendTabs(1).AppendLine("Document: 1054946992, \"\", \"Scene\" {");
                documentBuilder.AppendTabs(2).AppendLine("Properties70:  {");
                documentBuilder.AppendTabs(3).AppendLine("P: \"SourceObject\", \"object\", \"\", \"\"");
                documentBuilder.AppendTabs(3).AppendLine("P: \"ActiveAnimStackName\", \"KString\", \"\", \"\", \"\"");
                documentBuilder.AppendTabs(2).AppendLine("}");
                documentBuilder.AppendTabs(2).AppendLine("RootNode: 0");
                documentBuilder.AppendTabs(1).AppendLine("}");
                documentBuilder.AppendLine("}");
                documentBuilder.AppendLine();

                //document references, empty for now
                documentBuilder.AppendLine("; Document References");
                documentBuilder.AppendLine(";------------------------------------------------------------------");
                documentBuilder.AppendLine();
                documentBuilder.AppendLine("References:  {");
                documentBuilder.AppendLine("}");
                documentBuilder.AppendLine();

                //FBX makes us count the number of objects of a given kind
                //let's first calculate how many items we have
                var materialGroups = meshEntities.SelectMany(x => x.Faces).GroupBy(val => val.Material).ToList();

                documentBuilder.AppendLine("; Object definitions");
                documentBuilder.AppendLine(";------------------------------------------------------------------");
                documentBuilder.AppendLine();
                documentBuilder.AppendLine("Definitions:  {");
                documentBuilder.AppendTabs(1).AppendLine("Version: 100");
                documentBuilder.AppendTabs(1).AppendLine("Count: " + (materialGroups.Count * 4 + 1)); //*2 because we add number of Models and Geometries, +1 for the GlobalSettings

                documentBuilder.AppendTabs(1).AppendLine("ObjectType: \"GlobalSettings\" {");
                documentBuilder.AppendTabs(2).AppendLine("Count: 1");
                documentBuilder.AppendTabs(1).AppendLine("}");

                documentBuilder.AppendTabs(1).AppendLine("ObjectType: \"Model\" {");
                documentBuilder.AppendTabs(2).AppendLine("Count: " + materialGroups.Count);
                documentBuilder.AppendTabs(1).AppendLine("}");

                documentBuilder.AppendTabs(1).AppendLine("ObjectType: \"Geometry\" {");
                documentBuilder.AppendTabs(2).AppendLine("Count: " + materialGroups.Count);
                documentBuilder.AppendTabs(1).AppendLine("}");

                documentBuilder.AppendTabs(1).AppendLine("ObjectType: \"Material\" {");
                documentBuilder.AppendTabs(2).AppendLine("Count: " + materialGroups.Count);
                documentBuilder.AppendTabs(1).AppendLine("}");

                documentBuilder.AppendTabs(1).AppendLine("ObjectType: \"Texture\" {");
                documentBuilder.AppendTabs(2).AppendLine("Count: " + materialGroups.Count);
                documentBuilder.AppendTabs(1).AppendLine("}");

                documentBuilder.AppendLine("}");
                documentBuilder.AppendLine();


                //now we need to export the objects themselves
                documentBuilder.AppendLine("; Object properties");
                documentBuilder.AppendLine(";------------------------------------------------------------------");
                documentBuilder.AppendLine();
                documentBuilder.AppendLine("Objects:  {");

                int startingModelId = 1000;
                int startingGeometryId = startingModelId + materialGroups.Count;
                int startingMaterialId = startingModelId + materialGroups.Count * 2;
                int startingTextureId = startingModelId + materialGroups.Count * 3;
                int startingVideoId = startingModelId + materialGroups.Count * 4;

                //use these ids for incrementally set ids
                int baseId = 0;
                /*int modelId = startingModelId;
                int materialId = startingMaterialId;*/

                foreach (IGrouping<Material, Face> grouping in materialGroups)
                {
                    var material = grouping.Key;

                    //add the models, which are the main thing below the root
                    documentBuilder.AppendTabs(1).AppendLine("Model: " + (startingModelId + baseId) + ",\"Model::Model" + (startingModelId + baseId) + "\", \"Mesh\" {");
                    documentBuilder.AppendTabs(2).AppendLine("Version: 232");
                    documentBuilder.AppendTabs(2).AppendLine("Properties70:  {");
                    documentBuilder.AppendTabs(3).AppendLine("P:\"InheritType\", \"enum\", \"\", \"\",1");
                    documentBuilder.AppendTabs(3).AppendLine("P:\"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
                    documentBuilder.AppendTabs(3).AppendLine("P:\"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");
                    documentBuilder.AppendTabs(3).AppendLine("P:\"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",0,0,0");
                    documentBuilder.AppendTabs(3).AppendLine("P:\"MaxHandle\", \"int\", \"Integer\", \"UH\",3");
                    documentBuilder.AppendTabs(2).AppendLine("}");
                    documentBuilder.AppendTabs(2).AppendLine("Shading: T");
                    documentBuilder.AppendTabs(2).AppendLine("Culling: \"CullingOff\"");
                    documentBuilder.AppendTabs(1).AppendLine("}");


                    //the geometry will be child of the model and contains the actual 3d data
                    documentBuilder.AppendTabs(1).AppendLine("Geometry: " + (startingGeometryId + baseId) + ",\"Geometry::Geometry" + (startingGeometryId + baseId) + "\", \"Mesh\" {");
                    documentBuilder.AppendTabs(2).AppendLine("Properties70: {");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"Color\", \"ColorRGB\", \"Color\", \"\",0.72156862745098,0.894117647058823,0.6");
                    documentBuilder.AppendTabs(2).AppendLine("}");


                    List<Vector3D> positions = new List<Vector3D>();
                    //List<Color> colors = new List<Color>();
                    List<Vector3D> normals = new List<Vector3D>();
                    List<Vector2D> textureCoords = new List<Vector2D>();
                    List<Vector3D> tangents = new List<Vector3D>();
                    List<Vector3D> binormals = new List<Vector3D>();
                    List<int> indices = new List<int>();

                    foreach (Face face in grouping)
                    {
                        var halfVertices = face.HalfVertices.ToList();
                        if (_parameterFlipFaces.Value)
                            halfVertices.Reverse();

                        foreach (HalfVertex halfVertex in halfVertices)
                        {
                            indices.Add(positions.Count);

                            positions.Add(halfVertex.Vertex.Position);
                            //colors.Add(halfVertex.Color);
                            normals.Add(halfVertex.Normal);
                            textureCoords.Add(halfVertex.UV0);
                            tangents.Add(halfVertex.Tangent);
                            binormals.Add(halfVertex.Binormal);
                        }

                        //set the last item negate, but add 1 (as the fbx formats works -.-')
                        indices[indices.Count - 1] = -(indices.Last() + 1);
                    }

                    //add the vertex positions
                    documentBuilder.AppendTabs(2).AppendLine("Vertices: *" + positions.Count * 3 + " {");
                    documentBuilder.AppendTabs(3).Append("a: ").AppendLine(string.Join(",", positions.Select(val => val.ToString("{0},{1},{2}"))));
                    documentBuilder.AppendTabs(2).AppendLine("}");


                    documentBuilder.AppendTabs(2).AppendLine("PolygonVertexIndex: *" + indices.Count + " {");
                    documentBuilder.AppendTabs(3).Append("a: ").AppendLine(string.Join(",", indices));
                    documentBuilder.AppendTabs(2).AppendLine("}");

                    documentBuilder.AppendTabs(2).AppendLine("GeometryVersion: 124");

                    documentBuilder.AppendTabs(2).AppendLine("LayerElementNormal: 0 {");
                    documentBuilder.AppendTabs(3).AppendLine("Version: 101");
                    documentBuilder.AppendTabs(3).AppendLine("MappingInformationType: \"ByPolygonVertex\"");
                    documentBuilder.AppendTabs(3).AppendLine("ReferenceInformationType: \"Direct\"");
                    documentBuilder.AppendTabs(3).AppendLine("Normals: *" + normals.Count * 3 + " {");
                    documentBuilder.AppendTabs(4).Append("a: ").AppendLine(string.Join(",", normals.Select(val => val.ToString("{0},{1},{2}"))));
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(2).AppendLine("}");

                    documentBuilder.AppendTabs(2).AppendLine("LayerElementUV: 0 {");
                    documentBuilder.AppendTabs(3).AppendLine("Version: 101");
                    documentBuilder.AppendTabs(3).AppendLine("MappingInformationType: \"ByPolygonVertex\"");
                    documentBuilder.AppendTabs(3).AppendLine("ReferenceInformationType: \"Direct\"");
                    documentBuilder.AppendTabs(3).AppendLine("UV: *" + textureCoords.Count * 2 + " {");
                    documentBuilder.AppendTabs(4).Append("a: ").AppendLine(string.Join(",", textureCoords.Select(val => val.ToString("{0},{1}"))));
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(2).AppendLine("}");

                    documentBuilder.AppendTabs(2).AppendLine("LayerElementTangent: 0 {");
                    documentBuilder.AppendTabs(3).AppendLine("Version: 101");
                    documentBuilder.AppendTabs(3).AppendLine("MappingInformationType: \"ByPolygonVertex\"");
                    documentBuilder.AppendTabs(3).AppendLine("ReferenceInformationType: \"Direct\"");
                    documentBuilder.AppendTabs(3).AppendLine("Tangents: *" + tangents.Count * 3 + " {");
                    documentBuilder.AppendTabs(4).Append("a: ").AppendLine(string.Join(",", tangents.Select(val => val.ToString("{0},{1},{2}"))));
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(3).AppendLine("TangentsW: *" + tangents.Count + " {");
                    documentBuilder.AppendTabs(4).Append("a: ").AppendLine(string.Join(",", Enumerable.Repeat(0, tangents.Count)));
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(2).AppendLine("}");

                    documentBuilder.AppendTabs(2).AppendLine("LayerElementBinormal: 0 {");
                    documentBuilder.AppendTabs(3).AppendLine("Version: 101");
                    documentBuilder.AppendTabs(3).AppendLine("MappingInformationType: \"ByPolygonVertex\"");
                    documentBuilder.AppendTabs(3).AppendLine("ReferenceInformationType: \"Direct\"");
                    documentBuilder.AppendTabs(3).AppendLine("Binormals: *" + binormals.Count * 3 + " {");
                    documentBuilder.AppendTabs(4).Append("a: ").AppendLine(string.Join(",", binormals.Select(val => val.ToString("{0},{1},{2}"))));
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(3).AppendLine("BinormalsW: *" + binormals.Count + " {");
                    documentBuilder.AppendTabs(4).Append("a: ").AppendLine(string.Join(",", Enumerable.Repeat(0, binormals.Count)));
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(2).AppendLine("}");

                    documentBuilder.AppendTabs(2).AppendLine("LayerElementMaterial: 0 {");
                    documentBuilder.AppendTabs(3).AppendLine("Version: 101");
                    documentBuilder.AppendTabs(3).AppendLine("MappingInformationType: \"AllSame\"");
                    documentBuilder.AppendTabs(3).AppendLine("ReferenceInformationType: \"IndexToDirect\"");
                    documentBuilder.AppendTabs(3).AppendLine("Materials: *1 {");
                    documentBuilder.AppendTabs(4).AppendLine("a: 0");
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(2).AppendLine("}");

                    //layers of the mesh
                    documentBuilder.AppendTabs(2).AppendLine("Layer: 0 {");
                    documentBuilder.AppendTabs(3).AppendLine("Version: 100");
                    documentBuilder.AppendTabs(3).AppendLine("LayerElement:  {");
                    documentBuilder.AppendTabs(4).AppendLine("Type: \"LayerElementNormal\"");
                    documentBuilder.AppendTabs(4).AppendLine("TypedIndex: 0");
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(3).AppendLine("LayerElement:  {");
                    documentBuilder.AppendTabs(4).AppendLine("Type: \"LayerElementMaterial\"");
                    documentBuilder.AppendTabs(4).AppendLine("TypedIndex: 0");
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(3).AppendLine("LayerElement:  {");
                    documentBuilder.AppendTabs(4).AppendLine("Type: \"LayerElementBinormal\"");
                    documentBuilder.AppendTabs(4).AppendLine("TypedIndex: 0");
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(3).AppendLine("LayerElement:  {");
                    documentBuilder.AppendTabs(4).AppendLine("Type: \"LayerElementTangent\"");
                    documentBuilder.AppendTabs(4).AppendLine("TypedIndex: 0");
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(3).AppendLine("LayerElement:  {");
                    documentBuilder.AppendTabs(4).AppendLine("Type: \"LayerElementTexture\"");
                    documentBuilder.AppendTabs(4).AppendLine("TypedIndex: 0");
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(3).AppendLine("LayerElement:  {");
                    documentBuilder.AppendTabs(4).AppendLine("Type: \"LayerElementUV\"");
                    documentBuilder.AppendTabs(4).AppendLine("TypedIndex: 0");
                    documentBuilder.AppendTabs(3).AppendLine("}");
                    documentBuilder.AppendTabs(2).AppendLine("}");

                    //closes  geometry
                    documentBuilder.AppendTabs(1).AppendLine("}");


                    //documentBuilder.AppendTabs(2).AppendLine("Shape {");

                    //the material definition comes here
                    //documentBuilder.AppendTabs(3).AppendLine("appearance Appearance {");

                    /*IFbxSaveDataHandler objMaterialExporter = MaterialExporters.Instanciate(grouping.Key);
                    if (objMaterialExporter == null)
                        throw new Exception("<No IFbxSaveDataHandler has been defined for material '" + grouping.Key.GetType().Name + "'.");

                    objMaterialExporter.Export(documentBuilder, grouping.Key, Procedure.Environment, texturesDirPath, TextureFolderName);*/

                    //documentBuilder.AppendTabs(3).AppendLine("}");


                    documentBuilder.AppendTabs(1).AppendLine("Material: " + (startingMaterialId + baseId) + ",\"Material::Material" + (startingMaterialId + baseId) + "\",\"\" {");
                    documentBuilder.AppendTabs(2).AppendLine("Version: 102");
                    documentBuilder.AppendTabs(2).AppendLine("ShadingModel: \"phong\"");
                    documentBuilder.AppendTabs(2).AppendLine("MultiLayer: 0");
                    documentBuilder.AppendTabs(2).AppendLine("Properties70:  {");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"AmbientColor\", \"Color\", \"\", \"A\",0,0,0");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"DiffuseColor\", \"Color\", \"\", \"A\",0.47843137254902,0.47843137254902,0.6");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"TransparentColor\", \"Color\", \"\", \"A\",1,1,1");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"SpecularColor\", \"Color\", \"\", \"A\",0.33,0.33,0.33");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"ReflectionFactor\", \"Number\", \"\", \"A\",0");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"Emissive\", \"Vector3D\", \"Vector\", \"\",0,0,0");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"Ambient\", \"Vector3D\", \"Vector\", \"\",0,0,0");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"Diffuse\", \"Vector3D\", \"Vector\", \"\",0.47843137254902,0.47843137254902,0.6");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"Specular\", \"Vector3D\", \"Vector\", \"\",0.33,0.33,0.33");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"Shininess\", \"double\", \"Number\", \"\",20");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"Opacity\", \"double\", \"Number\", \"\",1");
                    documentBuilder.AppendTabs(3).AppendLine("P: \"Reflectivity\", \"double\", \"Number\", \"\",0");
                    documentBuilder.AppendTabs(2).AppendLine("}");
                    documentBuilder.AppendTabs(1).AppendLine("}");


                    var materialExporter = _materialExporterHandlers[material.GetType()];


                    var relativeFilePath = materialExporter.Export(documentBuilder, material, resources, exportDirPath, TextureFolderName);

                    var texturePath = string.Empty;


                    //hardcoded for now, we need to find a solution for this
                    /*if (material is SingleTextureMaterial)
                    {
                        texturePath = ((SingleTextureMaterial)material).Texture;
                    }
                    else if(material is ColorMaterial)
                    {
                        var colorMaterial = (ColorMaterial)material;
                        texturePath = Path.GetTempFileName();


                    }*/

                    //var textureMaterial = (SingleTextureMaterial)grouping.Key;


                    //var relativeFilePath = Procedure.Environment.ExportTexture(texturePath, TextureFolderName, texturesDirPath);

                    documentBuilder.AppendTabs(1).AppendLine("Texture: " + (startingTextureId + baseId) + ",\"Texture::Texture" + (startingTextureId + baseId) + "\",\"\" {");
                    documentBuilder.AppendTabs(2).AppendLine("Type: \"TextureVideoClip\"");
                    documentBuilder.AppendTabs(2).AppendLine("Version: 202");
                    documentBuilder.AppendTabs(2).AppendLine("TextureName: \"Texture::Texture" + (startingTextureId + baseId) + "\"");
                    documentBuilder.AppendTabs(2).AppendLine("FileName: " + relativeFilePath.Quote());
                    documentBuilder.AppendTabs(2).AppendLine("RelativeFilename:" + relativeFilePath.Quote());
                    documentBuilder.AppendTabs(1).AppendLine("}");


                    baseId++;
                }

                documentBuilder.AppendLine("}");
                documentBuilder.AppendLine();


                //now we need to export the objects themselves
                documentBuilder.AppendLine("; Object connections");
                documentBuilder.AppendLine(";------------------------------------------------------------------");
                documentBuilder.AppendLine();
                documentBuilder.AppendLine("Connections:  {");

                //add all models as children of the rootnode
                for (int i = 0; i < materialGroups.Count; i++)
                    documentBuilder.AppendTabs(1).AppendLine("C: \"OO\"," + (startingModelId + i) + ", 0");

                //make meshes children of models
                for (int i = 0; i < materialGroups.Count; i++)
                    documentBuilder.AppendTabs(1).AppendLine("C: \"OO\"," + (startingGeometryId + i) + ", " + (startingModelId + i));

                //make materials children of models
                for (int i = 0; i < materialGroups.Count; i++)
                    documentBuilder.AppendTabs(1).AppendLine("C: \"OO\"," + (startingMaterialId + i) + ", " + (startingModelId + i));

                //make textures children of materials
                for (int i = 0; i < materialGroups.Count; i++)
                    documentBuilder.AppendTabs(1).AppendLine("C: \"OP\"," + (startingTextureId + i) + ", " + (startingMaterialId + i) + ", \"DiffuseColor\"");

                documentBuilder.AppendLine("}");

                File.WriteAllText(_parameterFileLocation.Value, documentBuilder.ToString());
            }



            public interface IFbxSaveDataHandler
            {
                string Export(StringBuilder documentBuilder, Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath);
            }


            [TypeKey(typeof(Material))]
            public class DefaultFbxMaterialHandler : IFbxSaveDataHandler
            {
                public string Export(StringBuilder documentBuilder, Material material, IResourceManager environment,
                    string absoluteExportDirPath, string relativeTextureFolderPath)
                {
                    throw new NotImplementedException(string.Format("The export of the material '{0}' is not (yet) supported.", material.Type));
                }
            }


            [TypeKey(typeof(ColorMaterial))]
            public class FbxSaveColorMaterialHandler : IFbxSaveDataHandler
            {
                public string Export(StringBuilder documentBuilder, Material material, IResourceManager resourceManager, string absoluteExportDirPath, string relativeTextureFolderPath)
                {
                    var defaultColor = ((ColorMaterial) material).DefaultColor;

                    string fileName = "ColorMaterial_" + defaultColor.R + "_" + defaultColor.G + "_" + defaultColor.B + ".png";

                    var relativeFilePath = Path.Combine(relativeTextureFolderPath, fileName);
                    var absoluteTextureFolderPath = Path.Combine(absoluteExportDirPath, relativeTextureFolderPath);
                    var absoluteFilePath = Path.Combine(absoluteTextureFolderPath, fileName);

                    //create the directory if it doesn't exist
                    if (!Directory.Exists(absoluteTextureFolderPath))
                        Directory.CreateDirectory(absoluteTextureFolderPath);

                    //and copy the textures there
                    Bitmap bitmap = new Bitmap(1, 1);
                    bitmap.SetPixel(0, 0, Color.FromArgb(defaultColor.A, defaultColor.R, defaultColor.G, defaultColor.B));
                    bitmap.Save(absoluteFilePath, ImageFormat.Png);

                    return PathHelper.ToUniversalPath(relativeFilePath);
                }
            }

            [TypeKey(typeof(SingleTextureMaterial))]
            public class FbxSaveSingleTextureMaterialHandler : IFbxSaveDataHandler
            {
                public string Export(StringBuilder documentBuilder, Material material, IResourceManager resourceManager, string absoluteExportDirPath,
                    string relativeTextureFolderPath)
                {
                    var filepath = ((SingleTextureMaterial) material).Texture;

                    var fileName = Path.GetFileName(filepath);
                    var relativeFilePath = Path.Combine(relativeTextureFolderPath, fileName);
                    var absoluteTextureFolderPath = Path.Combine(absoluteExportDirPath, relativeTextureFolderPath);
                    var absoluteFilePath = Path.Combine(absoluteTextureFolderPath, fileName);

                    //create the directory if it doesn't exist
                    if (!Directory.Exists(absoluteTextureFolderPath))
                        Directory.CreateDirectory(absoluteTextureFolderPath);

                    //and copy the textures there
                    File.Copy(resourceManager.GetFullPath(filepath), absoluteFilePath, true);

                    return PathHelper.ToUniversalPath(relativeFilePath);
                }
            }

            [TypeKey(typeof(ImportedMaterial))]
            public class FbxSaveImportedMaterialHandler : IFbxSaveDataHandler
            {
                public string Export(StringBuilder documentBuilder, Material material, IResourceManager resourceManager, string absoluteExportDirPath,
                    string relativeTextureFolderPath)
                {
                    var importedMaterial = (ImportedMaterial) material;

                    if (importedMaterial.HasDiffuseTexture)

                    {
                        var singletextureHandler = new FbxSaveSingleTextureMaterialHandler();
                        return singletextureHandler.Export(documentBuilder, new SingleTextureMaterial(importedMaterial.DiffuseTexturePath), resourceManager, absoluteExportDirPath, relativeTextureFolderPath);
                    }

                    var colorMaterialHandler = new FbxSaveColorMaterialHandler();
                    return colorMaterialHandler.Export(documentBuilder, new ColorMaterial(importedMaterial.ColorDiffuse), resourceManager, absoluteExportDirPath, relativeTextureFolderPath);
                }
            }
        }

        #endregion

        #region VRML

        /// <summary>
        /// Saves meshes to the .VRML file format.
        /// </summary>
        public class VRMLSaveParameter : MeshSaveParameter
        {
            /// <summary>
            /// Relative folder where the textures are saved.
            /// </summary>
            private const string TextureFolderName = "Textures";

            /// <summary>
            /// Specific functions that export materials
            /// </summary>
            //public static Realizer<IVrmlMaterialExporter> MaterialExporters = new Realizer<IVrmlMaterialExporter>();
            public static Dictionary<Type, IVrmlMaterialExporter> _materialExporters = AttributeReader.OfTypeKeyAttribute().GetInstancesOfType<IVrmlMaterialExporter>();

            /// <summary>
            /// Location where to store the file.
            /// </summary>
            private readonly FileParameter _parameterFileLocation = new FileParameter("File Location", Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "MyFile.wrl")) {ExtensionFilter = new[] {".wrl"}, IOOperation = IOOperation.Save}; //, 



            public VRMLSaveParameter()
                : base("VRML")
            {
            }



            public override void Save(List<MeshEntity> meshEntities)
            {
                string texturesDirPath = Path.Combine(Path.GetDirectoryName(_parameterFileLocation.Value), TextureFolderName);

                StringBuilder documentBuilder = new StringBuilder();

                documentBuilder.AppendLine("#VRML V2.0 utf8");
                documentBuilder.AppendLine("#Generated by Sceelix, (C) Copyright Sceelix " + DateTime.Now.Year);
                documentBuilder.AppendLine("#" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));


                //create a group that will encompass everything
                documentBuilder.AppendLine("Group {");
                documentBuilder.AppendTabs(1).AppendLine("children [");

                foreach (IGrouping<Material, Face> grouping in meshEntities.SelectMany(x => x.Faces).GroupBy(val => val.Material))
                {
                    documentBuilder.AppendTabs(2).AppendLine("Shape {");

                    //the material definition comes here
                    documentBuilder.AppendTabs(3).AppendLine("appearance Appearance {");

                    IVrmlMaterialExporter objMaterialExporter = _materialExporters[grouping.Key.GetType()];

                    objMaterialExporter.Export(documentBuilder, grouping.Key, ProcedureEnvironment.GetService<IResourceManager>(), texturesDirPath, TextureFolderName);

                    documentBuilder.AppendTabs(3).AppendLine("}");


                    List<Vector3D> positions = new List<Vector3D>();
                    List<Color> colors = new List<Color>();
                    List<Vector3D> normals = new List<Vector3D>();
                    List<Vector2D> textureCoords = new List<Vector2D>();
                    List<int> indices = new List<int>();


                    foreach (Face face in grouping)
                    {
                        foreach (HalfVertex halfVertex in face.HalfVertices)
                        {
                            indices.Add(positions.Count);

                            positions.Add(halfVertex.Vertex.Position);
                            colors.Add(halfVertex.Color);
                            normals.Add(halfVertex.Normal);
                            textureCoords.Add(halfVertex.UV0);
                        }

                        indices.Add(-1);
                    }

                    //and here goes the geometry definition
                    documentBuilder.AppendTabs(3).AppendLine("geometry IndexedFaceSet {");
                    documentBuilder.AppendTabs(4).AppendLine("solid FALSE");
                    documentBuilder.AppendTabs(4).AppendLine("ccw FALSE");
                    documentBuilder.AppendTabs(4).AppendLine("normalPerVertex TRUE");

                    if (grouping.All(x => x.IsConvex))
                        documentBuilder.AppendTabs(4).AppendLine("convex TRUE");

                    //add the vertex positions
                    documentBuilder.AppendTabs(4).AppendLine("coord Coordinate {");
                    documentBuilder.AppendTabs(5).Append("point [").Append(string.Join(",", positions.Select(val => string.Format("{0} {1} {2}", val.X.ToString(CultureInfo.InvariantCulture), val.Y.ToString(CultureInfo.InvariantCulture), val.Z.ToString(CultureInfo.InvariantCulture))))).AppendLine("]");
                    documentBuilder.AppendTabs(4).AppendLine("}");

                    //add the vertex colors
                    /*documentBuilder.AppendTabs(4).AppendLine("color Color {");
                    documentBuilder.AppendTabs(5).Append("color [").Append(String.Join(",", colors.Select(val => String.Format("{0} {1} {2}", val.R.ToString(CultureInfo.InvariantCulture), val.G.ToString(CultureInfo.InvariantCulture), val.B.ToString(CultureInfo.InvariantCulture))))).AppendLine("]");
                    documentBuilder.AppendTabs(4).AppendLine("}");*/

                    //add the vertex normals
                    documentBuilder.AppendTabs(4).AppendLine("normal Normal {");
                    documentBuilder.AppendTabs(5).Append("vector [").Append(string.Join(",", normals.Select(val => string.Format("{0} {1} {2}", val.X.ToString(CultureInfo.InvariantCulture), val.Y.ToString(CultureInfo.InvariantCulture), val.Z.ToString(CultureInfo.InvariantCulture))))).AppendLine("]");
                    documentBuilder.AppendTabs(4).AppendLine("}");

                    //add the vertex textures
                    documentBuilder.AppendTabs(4).AppendLine("texCoord TextureCoordinate {");
                    documentBuilder.AppendTabs(5).Append("point [").Append(string.Join(",", textureCoords.Select(val => string.Format("{0} {1}", val.X.ToString(CultureInfo.InvariantCulture), val.Y.ToString(CultureInfo.InvariantCulture))))).AppendLine("]");
                    documentBuilder.AppendTabs(4).AppendLine("}");

                    //add the indices
                    string indicesString = string.Join(",", indices);
                    documentBuilder.AppendTabs(4).Append("coordIndex [").Append(indicesString).AppendLine("]");
                    //documentBuilder.AppendTabs(4).Append("colorIndex [").Append(indicesString).AppendLine("]");
                    documentBuilder.AppendTabs(4).Append("normalIndex [").Append(indicesString).AppendLine("]");
                    documentBuilder.AppendTabs(4).Append("texCoordIndex [").Append(indicesString).AppendLine("]");


                    //close the geometry
                    documentBuilder.AppendTabs(3).AppendLine("}");

                    //close the shape
                    documentBuilder.AppendTabs(2).AppendLine("}");
                }

                //close the children
                documentBuilder.AppendTabs(1).AppendLine("]");

                //close the group
                documentBuilder.AppendLine("}");

                File.WriteAllText(_parameterFileLocation.Value, documentBuilder.ToString());
            }



            public interface IVrmlMaterialExporter
            {
                void Export(StringBuilder documentBuilder, Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath);
            }

            [TypeKey(typeof(Material))]
            public class DefaultVrmlExporter : IVrmlMaterialExporter
            {
                public void Export(StringBuilder documentBuilder, Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath)
                {
                    throw new NotImplementedException(string.Format("The export of the material '{0}' is not (yet) supported.", material.Type));
                }
            }

            [TypeKey(typeof(ColorMaterial))]
            public class ColorNormalExporter : IVrmlMaterialExporter
            {
                public void Export(StringBuilder documentBuilder, Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath)
                {
                    ColorMaterial textureMaterial = (ColorMaterial) material;

                    string fileName = "ColorMaterial_" + textureMaterial.DefaultColor.R + "_" + textureMaterial.DefaultColor.G + "_" + textureMaterial.DefaultColor.B + ".png";
                    var relativeFilePath = Path.Combine(relativeTextureFolderPath, fileName);
                    var absoluteFilePath = Path.Combine(absoluteExportDirPath, fileName);

                    documentBuilder.AppendTabs(4).AppendLine("material Material { diffuseColor 1 1 1 }");
                    documentBuilder.AppendTabs(4).AppendLine("texture ImageTexture {");
                    documentBuilder.AppendTabs(5).AppendLine("url \"" + PathHelper.ToUniversalPath(relativeFilePath) + "\"");
                    documentBuilder.AppendTabs(4).AppendLine("}");

                    //create the directory if it doesn't exist
                    if (Directory.Exists(absoluteExportDirPath))
                        Directory.CreateDirectory(absoluteExportDirPath);

                    Bitmap bitmap = new Bitmap(1, 1);
                    bitmap.SetPixel(0, 0, Color.FromArgb(textureMaterial.DefaultColor.A, textureMaterial.DefaultColor.R, textureMaterial.DefaultColor.G, textureMaterial.DefaultColor.B));
                    bitmap.Save(absoluteFilePath, ImageFormat.Png);
                }
            }

            [TypeKey(typeof(SingleTextureMaterial))]
            public class SingleTextureExporter : IVrmlMaterialExporter
            {
                public void Export(StringBuilder documentBuilder, Material material, IResourceManager environment, string absoluteExportDirPath, string relativeTextureFolderPath)
                {
                    SingleTextureMaterial textureMaterial = (SingleTextureMaterial) material;

                    var fileName = Path.GetFileName(textureMaterial.Texture);
                    var relativeFilePath = Path.Combine(relativeTextureFolderPath, fileName);
                    var absoluteFilePath = Path.Combine(absoluteExportDirPath, fileName);

                    documentBuilder.AppendTabs(4).AppendLine("material Material { diffuseColor 1 1 1 }");
                    documentBuilder.AppendTabs(4).AppendLine("texture ImageTexture {");
                    documentBuilder.AppendTabs(5).AppendLine("url \"" + PathHelper.ToUniversalPath(relativeFilePath) + "\"");
                    documentBuilder.AppendTabs(4).AppendLine("}");

                    //create the directory if it doesn't exist
                    if (!Directory.Exists(absoluteExportDirPath))
                        Directory.CreateDirectory(absoluteExportDirPath);

                    //and copy the textures there
                    File.Copy(environment.GetFullPath(textureMaterial.Texture), absoluteFilePath, true);
                }
            }
        }

        #endregion
    }
}