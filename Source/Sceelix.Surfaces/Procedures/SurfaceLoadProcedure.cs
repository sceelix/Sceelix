using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using ImageProcessor.Imaging;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resources;
using Sceelix.Extensions;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Loads surface layers from disk from standard image formats.
    /// </summary>
    [Procedure("e20f3064-d958-4d7c-841d-f6b39f3981c4", Label = "Surface Load", Category = "Surface")]
    public class SurfaceLoadProcedure : SystemProcedure
    {
        /// <summary>
        /// The loaded surface entity.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");


        /// <summary>
        /// Square size of each terrain cell.
        /// </summary>
        private readonly FloatParameter _parameterCellSize = new FloatParameter("Cell Size", 1);


        /// <summary>
        /// Images to be loaded into layers. Images with different resolutions can be loaded, whereas the surface resolution will be determined by the first.
        /// </summary>
        private readonly ListParameter<SurfaceLoadParameter> _parameterLayers = new ListParameter<SurfaceLoadParameter>("Layers");



        public SurfaceLoadProcedure()
        {
            _parameterLayers.Set("Heightmap");
        }



        public override IEnumerable<string> Tags => base.Tags.Union(_parameterLayers.SubParameterLabels);



        protected override void Run()
        {
            SurfaceEntity surfaceEntity = null;

            foreach (var surfaceLoadParameter in _parameterLayers.Items)
            {
                var newSurfaceEntity = surfaceLoadParameter.Load(_parameterCellSize.Value);

                if (surfaceEntity == null)
                    surfaceEntity = newSurfaceEntity;
                else
                    surfaceEntity.Merge(newSurfaceEntity);
            }

            if (surfaceEntity != null)
                _output.Write(surfaceEntity);
        }



        #region Abstract Parameter

        public abstract class SurfaceLoadParameter : CompoundParameter
        {
            protected SurfaceLoadParameter(string label)
                : base(label)
            {
            }



            protected internal abstract SurfaceEntity Load(float cellSize);
        }

        #endregion

        #region Heightmap

        /// <summary>
        /// Loads a height map image into a height layer.
        /// </summary>
        /// <seealso cref="SurfaceLoadParameter" />
        public class HeightmapSurfaceLoadParameter : SurfaceLoadParameter
        {
            /// <summary>
            /// Image with the heights of the surface. A grayscale image is expected, or else only the red component will be used.
            /// </summary>
            private readonly FileParameter _parameterHeightmap = new FileParameter("File", "", BitmapExtension.SupportedFileExtensions) {IOOperation = IOOperation.Load};


            /// <summary>
            /// Scale of the height values. Since the images value will be in the 0-1 range, this indicates the multiplication factor.
            /// </summary>
            private readonly FloatParameter _parameterHeightScale = new FloatParameter("Height Scale", 1);


            /// <summary>
            /// The type of height data interpolation. Because surfaces are often rendered as triangle or quad meshes, there is more than one possible setup.
            /// Different target platforms may use different setups, which could affect, for instance, the positioning of the objects on the surface.<br/>
            /// <b>Top Left</b> means that the quads are split from the top left corner to the bottom right corner.<br/>
            /// <b>Top Right</b> means that the quads are split from the top right corner to the bottom left corner.<br/>
            /// <b>Bilinear</b> assumes that the quad is not split and the height values are bilinearly interpolated.
            /// </summary>
            private readonly EnumChoiceParameter<SurfaceInterpolation> _parameterSurfaceInterpolation = new EnumChoiceParameter<SurfaceInterpolation>("Interpolation", SurfaceInterpolation.TopLeft);



            protected HeightmapSurfaceLoadParameter()
                : base("Heightmap")
            {
            }



            protected internal override SurfaceEntity Load(float cellSize)
            {
                Bitmap bitmap = ProcedureEnvironment.GetService<IResourceManager>().Load<Bitmap>(_parameterHeightmap.Value).ConvertFormat(PixelFormat.Format32bppArgb);

                var surfaceEntity = new SurfaceEntity(bitmap.Width, bitmap.Height, cellSize);

                //the heightmap decides the sizes
                int columns = bitmap.Width;
                int rows = bitmap.Height;

                float[,] heights = new float[columns, rows];
                var heightScale = _parameterHeightScale.Value / 255f;

                using (FastBitmap heightFastBitmap = new FastBitmap(bitmap))
                {
                    ParallelHelper.For(0, columns, i =>
                    {
                        for (int j = 0; j < rows; j++) heights[i, j] = heightFastBitmap.GetPixel(i, j).R * heightScale;
                    });
                }

                bitmap.Dispose();

                surfaceEntity.AddLayer(new HeightLayer(heights) {Interpolation = _parameterSurfaceInterpolation.Value});

                return surfaceEntity;
            }
        }

        #endregion

        #region Splatmap

        /// <summary>
        /// Loads a splat map image into four blend layers.
        /// </summary>
        /// <seealso cref="SurfaceLoadParameter" />
        public class SplatmapSurfaceLoadParameter : SurfaceLoadParameter
        {
            /// <summary>
            /// Image with the colors of the surface (used for coloring or splat mapping). A color image is expected, whereas each component will be mapped into a different blend layer, in the RGBA order.
            /// </summary>
            private readonly FileParameter _parameterSplatMap = new FileParameter("File", "", BitmapExtension.SupportedFileExtensions) {IOOperation = IOOperation.Load};

            /// <summary>
            /// The starting texture index that these blend maps apply to.
            /// </summary>
            private readonly IntParameter _parameterTextureIndex = new IntParameter("Texture Index", 0);



            protected SplatmapSurfaceLoadParameter()
                : base("Splatmap")
            {
            }



            protected internal override SurfaceEntity Load(float cellSize)
            {
                var resources = ProcedureEnvironment.GetService<IResourceManager>();
                Bitmap bitmap = resources.Load<Bitmap>(_parameterSplatMap.Value).ConvertFormat(PixelFormat.Format32bppArgb);

                var surfaceEntity = new SurfaceEntity(bitmap.Width, bitmap.Height, cellSize);

                //the heightmap decides the sizes
                int columns = bitmap.Width;
                int rows = bitmap.Height;


                var blendValues = new float[4][,];
                for (int i = 0; i < 4; i++) blendValues[i] = new float[columns, rows];


                using (Bitmap splatmapImage = resources.Load<Bitmap>(_parameterSplatMap.Value).ConvertFormat(PixelFormat.Format32bppArgb))
                {
                    using (FastBitmap splatFastBitmap = new FastBitmap(splatmapImage))
                    {
                        ParallelHelper.For(0, columns, i =>
                        {
                            for (int j = 0; j < rows; j++)
                            {
                                var systemColor = splatFastBitmap.GetPixel(i, j);

                                blendValues[0][i, j] = systemColor.R / 255f;
                                blendValues[1][i, j] = systemColor.G / 255f;
                                blendValues[2][i, j] = systemColor.B / 255f;
                                blendValues[3][i, j] = systemColor.A / 255f;
                            }
                        });
                    }
                }

                for (int i = 0; i < 4; i++) surfaceEntity.AddLayer(new BlendLayer(blendValues[i], _parameterTextureIndex.Value + i));

                //SurfaceBlendExtensions.FixTextureIndices()

                return surfaceEntity;
            }
        }

        #endregion

        #region Normal

        /// <summary>
        /// Loads a normal map image into a normal layer.
        /// </summary>
        public class NormalSurfaceLoadParameter : SurfaceLoadParameter
        {
            /// <summary>
            /// Image with surface normals.
            /// </summary>
            private readonly FileParameter _parameterNormalMap = new FileParameter("File", "", BitmapExtension.SupportedFileExtensions) {IOOperation = IOOperation.Load};



            protected NormalSurfaceLoadParameter()
                : base("Normalmap")
            {
            }



            protected internal override SurfaceEntity Load(float cellSize)
            {
                var resources = ProcedureEnvironment.GetService<IResourceManager>();
                Bitmap bitmap = resources.Load<Bitmap>(_parameterNormalMap.Value).ConvertFormat(PixelFormat.Format32bppArgb);

                var surfaceEntity = new SurfaceEntity(bitmap.Width, bitmap.Height, cellSize);

                int columns = bitmap.Width;
                int rows = bitmap.Height;

                var normals = new Vector3D[columns, rows];

                using (Bitmap normalMapImage = resources.Load<Bitmap>(_parameterNormalMap.Value).ConvertFormat(PixelFormat.Format32bppArgb))
                {
                    using (FastBitmap normalFastBitmap = new FastBitmap(normalMapImage))
                    {
                        ParallelHelper.For(0, columns, i =>
                        {
                            for (int j = 0; j < rows; j++)
                            {
                                var systemColor = normalFastBitmap.GetPixel(i, j);

                                normals[i, j] = new Vector3D(systemColor.R / 255f, systemColor.G / 255f, systemColor.B / 255f);
                            }
                        });
                    }
                }

                surfaceEntity.AddLayer(new NormalLayer(normals));

                return surfaceEntity;
            }
        }

        #endregion

        #region BlendMap

        /// <summary>
        /// Loads a blend map into a blend layer.
        /// </summary>
        public class BlendMapSurfaceLoadParameter : SurfaceLoadParameter
        {
            /// <summary>
            /// Image with the blend values. A grayscale image is expected, or else only the red component will be used.
            /// </summary>
            private readonly FileParameter _parameterFile = new FileParameter("File", "", BitmapExtension.SupportedFileExtensions) {IOOperation = IOOperation.Load};

            /// <summary>
            /// The index of the texture that this blend map applies to.
            /// </summary>
            private readonly IntParameter _parameterTextureIndex = new IntParameter("Texture Index", 0);



            protected BlendMapSurfaceLoadParameter()
                : base("Blendmap")
            {
            }



            protected internal override SurfaceEntity Load(float cellSize)
            {
                var resources = ProcedureEnvironment.GetService<IResourceManager>();
                Bitmap bitmap = resources.Load<Bitmap>(_parameterFile.Value).ConvertFormat(PixelFormat.Format32bppArgb);

                var surfaceEntity = new SurfaceEntity(bitmap.Width, bitmap.Height, cellSize);

                //the heightmap decides the sizes
                int columns = bitmap.Width;
                int rows = bitmap.Height;

                float[,] intensities = new float[columns, rows];

                using (FastBitmap heightFastBitmap = new FastBitmap(bitmap))
                {
                    ParallelHelper.For(0, columns, i =>
                    {
                        for (int j = 0; j < rows; j++) intensities[i, j] = heightFastBitmap.GetPixel(i, j).R / 255f;
                    });
                }

                bitmap.Dispose();

                //var layerIndex = surfaceEntity.Layers.OfType<BlendLayer>().Count();

                surfaceEntity.AddLayer(new BlendLayer(intensities, _parameterTextureIndex.Value));

                return surfaceEntity;
            }
        }

        #endregion
    }
}