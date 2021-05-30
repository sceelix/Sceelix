using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ImageProcessor.Imaging;
using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Logging;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Serialization;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Extensions;
using Color = System.Drawing.Color;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Saves surfaces to disk.
    /// </summary>
    [Procedure("88daf46e-8bdb-408e-b445-3d0f633edf8c", Label = "Surface Save", Category = "Surface")]
    public class SurfaceSaveProcedure : TransferProcedure<SurfaceEntity>
    {
        /// <summary>
        /// The files to export to the surface to.
        /// </summary>
        private readonly ListParameter<SurfaceSaveParameter> _parameterSurfaceSave = new ListParameter<SurfaceSaveParameter>("Files");



        protected override SurfaceEntity Process(SurfaceEntity entity)
        {
            foreach (var surfaceSaveParameter in _parameterSurfaceSave.Items)
                surfaceSaveParameter.Save(entity);

            return entity;
        }



        #region Abstract Parameter

        public abstract class SurfaceSaveParameter : CompoundParameter
        {
            protected SurfaceSaveParameter(string label)
                : base(label)
            {
            }



            protected ImageFormat GetImageFormat(string fileName)
            {
                var extension = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(extension))
                {
                    extension = extension.ToLower();

                    if (extension == ".jpeg" || extension == ".jpg")
                        return ImageFormat.Jpeg;
                    if (extension == ".bmp")
                        return ImageFormat.Bmp;
                    if (extension == ".tiff")
                        return ImageFormat.Tiff;
                    if (extension == ".gif")
                        return ImageFormat.Gif;
                }

                return ImageFormat.Png;
            }



            protected internal abstract void Save(SurfaceEntity entity);
        }

        #endregion

        #region Heightmap

        /// <summary>
        /// Saves the surface height layer as a height map to an image file.
        /// </summary>
        public class HeightSaveParameter : SurfaceSaveParameter
        {
            /// <summary>
            /// Location where to store the file.
            /// </summary>
            private readonly FileParameter _parameterFile = new FileParameter("File", "", BitmapExtension.SupportedFileExtensions.Union(new[] {".raw"}).ToArray()) {IOOperation = IOOperation.Save};

            /// <summary>
            /// Indicates which resolution (image size in columns/rows) the file should take. This choice is not important if the resolution of the layer and surface are the same.<br/>
            /// <b>Surface</b> means that the surface resolution will be used. The values of the layer will be interpolated according to the defined interpolation.<br/>
            /// <b>Layer</b> means that the layer resolution will be used. 
            /// </summary>
            private readonly ChoiceParameter _parameterResolution = new ChoiceParameter("Resolution", "Surface", "Surface", "Layer");

            /// <summary>
            /// Indicates how the relative height values should be stored.<br/>
            /// <b>Minimum</b> means that the stored height values will be relative to the surface minimum height value. Essential if the surface has negative height values.<br/>
            /// <b>World</b> means that the stored height values will be relative to the world's zero plane. Makes import easier, as it stored absolute values, without the need for extra offset information. Cannot store negative values, though. 
            /// </summary>
            private readonly ChoiceParameter _parameterBaseHeight = new ChoiceParameter("Base Height", "World", "Minimum", "World");



            public HeightSaveParameter()
                : base("Heightmap")
            {
            }



            protected internal override void Save(SurfaceEntity entity)
            {
                var heightLayer = entity.GetLayer<HeightLayer>();
                if (heightLayer == null)
                {
                    ProcedureEnvironment.GetService<ILogger>().Log("The current surface does not have a height layer to save.");
                    return;
                }

                int numColumns = entity.NumColumns;
                int numRows = entity.NumRows;
                Func<int, int, float> action = (column, row) => heightLayer.GetGenericValue(new Coordinate(column, row));

                if (_parameterResolution.Value == "Layer")
                {
                    numColumns = heightLayer.NumColumns;
                    numRows = heightLayer.NumRows;
                    action = (column, row) => heightLayer.GetGenericValue(new Coordinate(column, row));
                }

                //the saved height values are always relative to 
                var minHeight = _parameterBaseHeight.Value == "Minimum" ? heightLayer.MinHeight : 0;
                var maxHeight = heightLayer.MaxHeight;
                var height = maxHeight - minHeight;


                //depending if we want to calculate the distance to the zero level
                //or to the minimum surface height
                if (_parameterBaseHeight.Value == "Minimum")
                {
                    var baseAction = action;
                    action = (i, j) => (baseAction(i, j) - minHeight) / height;
                }


                if (Path.GetExtension(_parameterFile.Value) == ".raw")
                {
                    using (FileStream stream = new FileStream(_parameterFile.Value, FileMode.Create))
                    {
                        for (int j = entity.NumRows - 1; j >= 0; j--)
                        for (int i = 0; i < entity.NumColumns; i++)
                        {
                            var fraction = heightLayer.GetGenericValue(new Coordinate(i, j)) / height;
                            fraction = MathHelper.Clamp(fraction, 0f, 1f);
                            stream.WriteByte((byte) (255 * fraction));
                        }
                    }
                }
                else
                {
                    Bitmap bitmap = new Bitmap(numColumns, numRows, PixelFormat.Format32bppArgb);
                    using (FastBitmap fastBitmap = new FastBitmap(bitmap))
                    {
                        for (int i = 0; i < numColumns; i++)
                        for (int j = 0; j < numRows; j++)
                        {
                            var fraction = heightLayer.GetGenericValue(new Coordinate(i, j)) / height;
                            fraction = MathHelper.Clamp(fraction, 0f, 1f);
                            //var fraction = action(i, j);
                            fastBitmap.SetPixel(i, j, Color.FromArgb(255, (byte) (255f * fraction), (byte) (255 * fraction), (byte) (255 * fraction)));
                        }
                    }

                    ImageFormat format = GetImageFormat(_parameterFile.Value);
                    bitmap.Save(_parameterFile.Value, format);
                }
            }
        }

        #endregion

        #region Splatmap

        /// <summary>
        /// Saves the surface height layer as a height map to an image file.
        /// </summary>
        public class SplatSaveParameter : SurfaceSaveParameter
        {
            /// <summary>
            /// Location where to store the file.
            /// </summary>
            private readonly FileParameter _parameterFile = new FileParameter("File", "", BitmapExtension.SupportedFileExtensions) {IOOperation = IOOperation.Save};

            /// <summary>
            /// Indicates which resolution (image size in columns/rows) the file should take. This choice is not important if the resolution of the layer and surface are the same.<br/>
            /// <b>Surface</b> means that the surface resolution will be used. The values of the layer will be interpolated according to the defined interpolation.
            /// <b>Layer</b> means that the layer resolution will be used. 
            /// </summary>
            private readonly ChoiceParameter _parameterResolution = new ChoiceParameter("Resolution", "Surface", "Surface", "Layer");



            public SplatSaveParameter()
                : base("Splatmap")
            {
            }



            protected internal override void Save(SurfaceEntity entity)
            {
                var fileName = Path.GetFileNameWithoutExtension(_parameterFile.Value);
                var directory = Path.GetDirectoryName(_parameterFile.Value);

                Mathematics.Data.Color[,] colorArray = entity.Layers.OfType<BlendLayer>().ToColorArray(entity.NumColumns, entity.NumRows);


                var heightLayer = entity.GetLayer<HeightLayer>();
                if (heightLayer == null)
                {
                    ProcedureEnvironment.GetService<ILogger>().Log("The current surface does not have a height layer to save.");
                    return;
                }

                int numColumns = entity.NumColumns;
                int numRows = entity.NumRows;
                Func<int, int, float> action = (column, row) => heightLayer.GetGenericValue(new Coordinate(column, row));

                if (_parameterResolution.Value == "Layer")
                {
                    numColumns = heightLayer.NumColumns;
                    numRows = heightLayer.NumRows;
                    action = (column, row) => heightLayer.GetGenericValue(new Coordinate(column, row));
                }

                Bitmap bitmap = new Bitmap(numColumns, numRows, PixelFormat.Format32bppArgb);
                using (FastBitmap fastBitmap = new FastBitmap(bitmap))
                {
                    for (int i = 0; i < numColumns; i++)
                    for (int j = 0; j < numRows; j++)
                        fastBitmap.SetPixel(i, j, colorArray[i, j]);
                }

                ImageFormat format = GetImageFormat(_parameterFile.Value);
                bitmap.Save(_parameterFile.Value, format);
            }
        }

        #endregion

        #region Normalmap

        /// <summary>
        /// Saves the surface height layer as a height map to an image file.
        /// </summary>
        public class NormalSaveParameter : SurfaceSaveParameter
        {
            /// <summary>
            /// Location where to store the file.
            /// </summary>
            private readonly FileParameter _parameterFile = new FileParameter("File", "", BitmapExtension.SupportedFileExtensions) {IOOperation = IOOperation.Save};

            /// <summary>
            /// Indicates which resolution (image size in columns/rows) the file should take. This choice is not important if the resolution of the layer and surface are the same.<br/>
            /// <b>Surface</b> means that the surface resolution will be used. The values of the layer will be interpolated according to the defined interpolation.
            /// <b>Layer</b> means that the layer resolution will be used. 
            /// </summary>
            private readonly ChoiceParameter _parameterResolution = new ChoiceParameter("Resolution", "Surface", "Surface", "Layer");



            public NormalSaveParameter()
                : base("Normalmap")
            {
            }



            protected internal override void Save(SurfaceEntity entity)
            {
                var normalLayer = entity.GetLayer<NormalLayer>();
                if (normalLayer == null)
                {
                    ProcedureEnvironment.GetService<ILogger>().Log("The current surface does not have a normal layer to save.");
                    return;
                }

                int numColumns = entity.NumColumns;
                int numRows = entity.NumRows;
                Func<int, int, Vector3D> action = (column, row) => normalLayer.GetGenericValue(new Coordinate(column, row));

                if (_parameterResolution.Value == "Layer")
                {
                    numColumns = normalLayer.NumColumns;
                    numRows = normalLayer.NumRows;
                    action = (column, row) => normalLayer.GetGenericValue(new Coordinate(column, row));
                }


                Bitmap bitmap = new Bitmap(numColumns, numRows, PixelFormat.Format32bppArgb);
                using (FastBitmap fastBitmap = new FastBitmap(bitmap))
                {
                    for (int i = 0; i < numColumns; i++)
                    for (int j = 0; j < numRows; j++)
                    {
                        var normal = action(i, j);
                        fastBitmap.SetPixel(i, j, Color.FromArgb(255, (byte) (255f * normal.X), (byte) (255 * normal.Y), (byte) (255 * normal.Z)));
                    }
                }

                ImageFormat format = GetImageFormat(_parameterFile.Value);
                bitmap.Save(_parameterFile.Value, format);
            }
        }

        #endregion

        #region BlendMap

        /// <summary>
        /// Saves the surface blend layers to a image files.
        /// </summary>
        public class BlendSaveParameter : SurfaceSaveParameter
        {
            /// <summary>
            /// Location where to store the file. The index of the blendlayer will be appended to the file name.
            /// </summary>
            private readonly FileParameter _parameterFile = new FileParameter("File", "", BitmapExtension.SupportedFileExtensions) {IOOperation = IOOperation.Save};

            /// <summary>
            /// Indicates which resolution (image size in columns/rows) the file should take. This choice is not important if the resolution of the layer and surface are the same.<br/>
            /// <b>Surface</b> means that the surface resolution will be used. The values of the layer will be interpolated according to the defined interpolation.<br/>
            /// <b>Layer</b> means that the layer resolution will be used. 
            /// </summary>
            private readonly ChoiceParameter _parameterResolution = new ChoiceParameter("Resolution", "Surface", "Surface", "Layer");



            public BlendSaveParameter()
                : base("Blendmap")
            {
            }



            protected internal override void Save(SurfaceEntity entity)
            {
                var blendLayers = entity.GetLayers<BlendLayer>();

                var fileName = Path.GetFileNameWithoutExtension(_parameterFile.Value);
                var extension = Path.GetExtension(_parameterFile.Value);
                var folderName = Path.GetDirectoryName(_parameterFile.Value);
                ImageFormat format = GetImageFormat(_parameterFile.Value);

                foreach (BlendLayer blendLayer in blendLayers)
                {
                    int numColumns = entity.NumColumns;
                    int numRows = entity.NumRows;
                    Func<int, int, float> action = (column, row) => blendLayer.GetGenericValue(new Coordinate(column, row));

                    if (_parameterResolution.Value == "Layer")
                    {
                        numColumns = blendLayer.NumColumns;
                        numRows = blendLayer.NumRows;
                        action = (column, row) => blendLayer.GetGenericValue(new Coordinate(column, row));
                    }


                    if (Path.GetExtension(_parameterFile.Value) == ".raw")
                    {
                        using (FileStream stream = new FileStream(_parameterFile.Value, FileMode.Create))
                        {
                            for (int j = entity.NumRows - 1; j >= 0; j--)
                            for (int i = 0; i < entity.NumColumns; i++)
                            {
                                var fraction = action(i, j);
                                stream.WriteByte((byte) (255 * fraction));
                            }
                        }
                    }
                    else
                    {
                        Bitmap bitmap = new Bitmap(numColumns, numRows, PixelFormat.Format32bppArgb);
                        using (FastBitmap fastBitmap = new FastBitmap(bitmap))
                        {
                            for (int i = 0; i < numColumns; i++)
                            for (int j = 0; j < numRows; j++)
                            {
                                var fraction = action(i, j);
                                //var fraction = action(i, j);
                                fastBitmap.SetPixel(i, j, Color.FromArgb(255, (byte) (255f * fraction), (byte) (255 * fraction), (byte) (255 * fraction)));
                            }
                        }


                        var actualFileName = Path.Combine(folderName, fileName + blendLayer.TextureIndex + extension);

                        bitmap.Save(actualFileName, format);
                    }
                }
            }
        }

        #endregion

        #region Meta

        /// <summary>
        /// Saves the surface metainformation (translation, grid size, etc.).
        /// </summary>
        public class MetaSaveParameter : SurfaceSaveParameter
        {
            /// <summary>
            /// Location where to store the file.
            /// </summary>
            private readonly FileParameter _parameterFile = new FileParameter("File", "", ".txt", ".json") {IOOperation = IOOperation.Save};



            public MetaSaveParameter()
                : base("Meta")
            {
            }



            protected internal override void Save(SurfaceEntity entity)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();

                properties.Add("NumColumns", entity.NumColumns);
                properties.Add("NumRows", entity.NumRows);
                properties.Add("CellSize", entity.CellSize);
                properties.Add("Origin", entity.Origin);
                properties.Add("Width", entity.Width);
                properties.Add("Height", entity.Height);
                properties.Add("Length", entity.Length);
                properties.Add("LayerCount", entity.Layers.Count());
                properties.Add("MinimumZ", entity.MinimumZ);
                properties.Add("MaximumZ", entity.MaximumZ);

                //TODO:make this extensible
                if (Path.GetExtension(_parameterFile.Value) == ".txt")
                    File.WriteAllLines(_parameterFile.Value, properties.Select(x => x.Key + ":" + x.Value));
                else if (Path.GetExtension(_parameterFile.Value) == ".json") JsonSerialization.SaveToFile(_parameterFile.Value, properties);
            }
        }

        #endregion
    }
}