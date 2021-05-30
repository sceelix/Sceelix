using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Extensions;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Applies modifications to the given input Surface.
    /// </summary>
    /// <remarks>Contains several operations.</remarks>
    [Procedure("c185cf6d-f351-48fc-b741-2fc53d2a5e77", Label = "Surface Modify", Category = "Surface")]
    public class SurfaceModifyProcedure : SystemProcedure
    {
        /// <summary>
        /// The surface to be modified.
        /// </summary>
        private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");

        /// <summary>
        /// The surface that has been modified.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");

        /// <summary>
        /// The operation to be applied to the surface.
        /// </summary>
        public SelectListParameter<SurfaceModifyParameter> ParameterOperations = new SelectListParameter<SurfaceModifyParameter>("Operation", "Smooth");


        public override IEnumerable<string> Tags => base.Tags.Union(ParameterOperations.SubParameterLabels);



        protected override void Run()
        {
            var surfaceEntity = _input.Read();

            foreach (var surfaceTransformParameter in ParameterOperations.Items)
                surfaceEntity = surfaceTransformParameter.Transform(surfaceEntity);

            _output.Write(surfaceEntity);
        }



        #region Abstract Parameter

        public abstract class SurfaceModifyParameter : CompoundParameter
        {
            protected SurfaceModifyParameter(string label)
                : base(label)
            {
            }



            public abstract SurfaceEntity Transform(SurfaceEntity surfaceEntity);
        }

        #endregion

        #region Surface Smooth

        /// <summary>
        /// Applies a 3x3 averaging matrix on the surface, creating a smoothing effect that can be applied several times.
        /// </summary>
        /// <seealso cref="Sceelix.Surfaces.Procedures.SurfaceModifyProcedure.SurfaceModifyParameter" />
        public class SurfaceSmoothModify : SurfaceModifyParameter
        {
            /// <summary>
            /// Index of the layer to convert.
            /// </summary>
            private readonly IntParameter _parameterLayerIndex = new IntParameter("Layer Index", 0);

            /// <summary>
            /// Intensity of the smoothing. Higher values will result in a smoother effect.
            /// </summary>
            private readonly FloatParameter _parameterIntensity = new FloatParameter("Intensity", 1) {MinValue = 0, MaxValue = 1};

            /// <summary>
            /// Number of smooth iterations. Higher values will result in a smoother effect.
            /// </summary>
            private readonly IntParameter _parameterIterations = new IntParameter("Iterations", 1) {MinValue = 0};

            /// <summary>
            /// Number of cells to skip when sampling. Higher values will result in greater smoothness at no performance cost, but sometimes
            /// can result in a pixelated effect.
            /// </summary>
            private readonly IntParameter _parameterSkip = new IntParameter("Skip", 0) {MinValue = 0};


            /// <summary>
            /// Number of border cells to skip in the smoothing process. This will avoid discontinuities when several surfaces are placed next to another.
            /// </summary>
            private readonly IntParameter _parameterBorder = new IntParameter("Border", 0) {MinValue = 0};



            protected SurfaceSmoothModify()
                : base("Smooth")
            {
            }



            public SurfaceLayer Smooth(SurfaceLayer surfaceLayer, int iterations, float intensity, int skip = 0, int border = 0)
            {
                //T[,] oldValues = RawValues;
                int numColumns = surfaceLayer.NumColumns;
                int numRows = surfaceLayer.NumRows;

                var currentLayer = surfaceLayer;
                //T[,] newValues = null; // = new float[surfaceEntity.NumColumns, surfaceEntity.NumRows];


                int distance = Math.Max(1, skip + 1);
                for (int i = 0; i < iterations; i++)
                {
                    var layer = currentLayer;
                    var newLayer = surfaceLayer.CreateEmpty(numColumns, numRows);


                    ParallelHelper.For(0, numColumns, x =>
                    {
                        for (int y = 0; y < numRows; y++)
                        {
                            object accumulator = newLayer.GetValue(new Coordinate(x, y));
                            var currentValue = layer.GetValue(new Coordinate(x, y));
                            int cells = 0;

                            //for the border cells, keep the original value
                            if (x < border || x >= numColumns - border || y < border || y >= numRows - border)
                            {
                                newLayer.SetValue(new Coordinate(x, y), currentValue);
                                continue;
                            }

                            if (x - distance > border)
                            {
                                accumulator = layer.Add(accumulator, layer.GetValue(new Coordinate(x - distance, y)));
                                cells++;

                                if (y - distance > border)
                                {
                                    accumulator = layer.Add(accumulator, layer.GetValue(new Coordinate(x - distance, y - distance)));
                                    cells++;
                                }

                                if (y + distance < numRows - border)
                                {
                                    accumulator = layer.Add(accumulator, layer.GetValue(new Coordinate(x - distance, y + distance)));
                                    cells++;
                                }
                            }

                            if (x + distance < numColumns - border)
                            {
                                accumulator = layer.Add(accumulator, layer.GetValue(new Coordinate(x + distance, y)));
                                cells++;

                                if (y - distance > border)
                                {
                                    accumulator = layer.Add(accumulator, layer.GetValue(new Coordinate(x + distance, y - distance)));
                                    cells++;
                                }

                                if (y + distance < numRows - border)
                                {
                                    accumulator = layer.Add(accumulator, layer.GetValue(new Coordinate(x + distance, y + distance)));
                                    cells++;
                                }
                            }

                            if (y - distance > border) // Check above
                            {
                                accumulator = layer.Add(accumulator, layer.GetValue(new Coordinate(x, y - distance)));
                                cells++;
                            }

                            if (y + distance < numRows - border) // Check below
                            {
                                accumulator = layer.Add(accumulator, layer.GetValue(new Coordinate(x, y + distance)));
                                cells++;
                            }

                            var newValue = surfaceLayer.MultiplyScalar(accumulator, 1f / cells);

                            var newMinusOld = layer.Minus(newValue, currentValue);
                            var scaledDifference = layer.MultiplyScalar(newMinusOld, intensity);
                            var interpolatedValue = layer.Add(currentValue, scaledDifference);

                            newLayer.SetValue(new Coordinate(x, y), interpolatedValue);
                        }
                    });

                    currentLayer = newLayer;
                }

                return currentLayer;
            }



            public override SurfaceEntity Transform(SurfaceEntity surfaceEntity)
            {
                var surfaceLayers = surfaceEntity.Layers.ToList();
                if (_parameterLayerIndex.Value >= surfaceLayers.Count)
                    throw new ArgumentException("The indicated layer index exceeds the amount of existing layers.");

                var layerToSmooth = surfaceLayers[_parameterLayerIndex.Value];
                var newLayer = Smooth(layerToSmooth, _parameterIterations.Value, _parameterIntensity.Value, _parameterSkip.Value, _parameterBorder.Value);

                surfaceEntity.ReplaceLayer(layerToSmooth, newLayer);

                /*foreach (SurfaceLayer surfaceEntityLayer in surfaceEntity.Layers)
                {
                    surfaceEntityLayer.Smooth(_parameterIterations.Value, _parameterIntensity.Value, _parameterSkip.Value);
                }*/

                return surfaceEntity;
            }
        }

        #endregion

        #region Surface Invert

        /// <summary>
        /// Inverts the values of the surface's height layer, turning the highest values into the lowest ones and lowest into the highest.
        /// </summary>
        /// <seealso cref="Sceelix.Surfaces.Procedures.SurfaceModifyProcedure.SurfaceModifyParameter" />
        public class SurfaceInvertModify : SurfaceModifyParameter
        {
            /// <summary>
            /// Index of the layer to convert.
            /// </summary>
            private readonly IntParameter _parameterLayerIndex = new IntParameter("Layer Index", 0);



            protected SurfaceInvertModify()
                : base("Invert")
            {
            }



            private void Invert(SurfaceLayer layerToInvert)
            {
                int numColumns = layerToInvert.NumColumns;
                int numRows = layerToInvert.NumRows;

                layerToInvert.Update();

                ParallelHelper.For(0, numColumns, x =>
                {
                    for (int y = 0; y < numRows; y++)
                    {   
                        layerToInvert.SetValue(new Coordinate(x, y), layerToInvert.Invert(layerToInvert.GetValue(new Coordinate(x, y))));
                    }
                });
            }



            public override SurfaceEntity Transform(SurfaceEntity surfaceEntity)
            {
                var surfaceLayers = surfaceEntity.Layers.ToList();
                if (_parameterLayerIndex.Value >= surfaceLayers.Count)
                    throw new ArgumentException("The indicated layer index exceeds the amount of existing layers.");

                Invert(surfaceLayers[_parameterLayerIndex.Value]);

                return surfaceEntity;
            }
        }

        #endregion

        #region Stepify

        /// <summary>
        /// Performs value quantization on height layers, creating a "step-like" effect.
        /// </summary>
        /// <seealso cref="Sceelix.Surfaces.Procedures.SurfaceModifyProcedure.SurfaceModifyParameter" />
        public class SurfaceStepifyModify : SurfaceModifyParameter
        {
            /// <summary>
            /// Index of the layer to convert.
            /// </summary>
            private readonly IntParameter _parameterLayerIndex = new IntParameter("Layer Index", 0);

            /// <summary>
            /// The size of each step.
            /// </summary>
            private readonly FloatParameter _parameterStepSize = new FloatParameter("Size", 10);



            protected SurfaceStepifyModify()
                : base("Stepify")
            {
            }



            public override SurfaceEntity Transform(SurfaceEntity surfaceEntity)
            {
                var surfaceLayers = surfaceEntity.Layers.ToList();
                if (_parameterLayerIndex.Value >= surfaceLayers.Count)
                    throw new ArgumentException("The indicated layer index exceeds the amount of existing layers.");

                var heightLayer = surfaceLayers[_parameterLayerIndex.Value] as HeightLayer;
                if (heightLayer == null)
                    throw new ArgumentException("The indicated layer index does not correspond to a valid height layer.");


                int numColumns = heightLayer.NumColumns;
                int numRows = heightLayer.NumRows;

                var stepSize = _parameterStepSize.Value;

                ParallelHelper.For(0, numColumns, x =>
                {
                    for (int y = 0; y < numRows; y++)
                    {
                        var layerValue = heightLayer.GetGenericValue(new Coordinate(x, y));

                        var newValue = Math.Round(layerValue / stepSize) * stepSize;

                        heightLayer.SetValue(new Coordinate(x, y), (float) newValue);
                    }
                });

                return surfaceEntity;
            }
        }

        #endregion

        #region Calculate Normals

        /// <summary>
        /// Recalculates the normals of the surface based on the height layer, so as to create a steady, smooth effect.
        /// </summary>
        /// <remarks>Creates a normal layer, if one does not yet exist.</remarks>
        public class SurfaceCalculateNormalsModify : SurfaceModifyParameter
        {
            protected SurfaceCalculateNormalsModify()
                : base("Recalculate Normals")
            {
            }



            public override SurfaceEntity Transform(SurfaceEntity surfaceEntity)
            {
                var normalLayer = surfaceEntity.GetLayer<NormalLayer>();
                if (normalLayer == null)
                {
                    var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
                    if (heightLayer == null)
                    {
                        surfaceEntity.AddLayer(new NormalLayer(new Vector3D[surfaceEntity.NumColumns, surfaceEntity.NumRows], Vector3D.ZVector));
                    }
                    else
                    {
                        var numColumns = heightLayer.NumColumns;
                        var numRows = heightLayer.NumRows;

                        Vector3D[,] values = new Vector3D[numColumns, numRows];
                        ParallelHelper.For(0, numColumns, x =>
                        {
                            for (int y = 0; y < numRows; y++) values[x, y] = heightLayer.CalculateNormal(new Coordinate(x, y));
                        });

                        surfaceEntity.AddLayer(new NormalLayer(values));
                    }
                }
                else
                {
                    //if there is no heightmap, use a 1x1 normalmap with Z-up
                    //otherwise
                    var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
                    if (heightLayer == null)
                    {
                        normalLayer.Fill(Vector3D.ZVector);
                    }
                    else
                    {
                        var numColumns = surfaceEntity.NumColumns;
                        var numRows = surfaceEntity.NumRows;

                        //make sure that the resolution of layer is the same as the surface
                        //normalLayer.ForceSurfaceResolution(false);

                        ParallelHelper.For(0, numColumns, x =>
                        {
                            for (int y = 0; y < numRows; y++) normalLayer.SetValue(new Coordinate(x, y), heightLayer.CalculateNormal(new Coordinate(x, y)));
                        });
                    }
                }

                return surfaceEntity;
            }
        }

        #endregion

        #region Change Cell Size

        /// <summary>
        /// Changes the cell size of the surface, but maintaining the overall size.
        /// </summary>
        public class CellSizeModify : SurfaceModifyParameter
        {
            /// <summary>
            /// New square size of each terrain cell. Using a higher value, multiple of the current cell size can be delivered quickly.
            /// Should be a multiple of the surface total Width and Length.
            /// </summary>
            private readonly FloatParameter _parameterCellSize = new FloatParameter("Cell Size", 1);



            protected CellSizeModify()
                : base("Change Cell Size")
            {
            }



            internal static SurfaceEntity ChangeCellSize(SurfaceEntity surfaceEntity, float newCellSize)
            {
                //if the cell sizes are the same, we don't need to do anything
                if (Math.Abs(newCellSize - surfaceEntity.CellSize) < float.Epsilon)
                    return surfaceEntity;

                int columns = (int) (surfaceEntity.Width / newCellSize) + 1;
                int rows = (int) (surfaceEntity.Length / newCellSize) + 1;

                var newSurface = new SurfaceEntity(columns, rows, newCellSize)
                {
                    Origin = surfaceEntity.Origin,
                    Material = (Material) surfaceEntity.Material.DeepClone()
                };

                surfaceEntity.Attributes.SetAttributesTo(newSurface.Attributes);

                foreach (var surfaceLayer in surfaceEntity.Layers)
                {
                    var newLayer = surfaceLayer.CreateEmpty(columns, rows);
                    ParallelHelper.For(0, columns, i =>
                    {
                        for (int j = 0; j < rows; j++)
                        {
                            var coordinate = new Coordinate(i, j);
                            var coordinateWorldPosition = newSurface.ToWorldPosition(coordinate);
                            var value = surfaceLayer.GetValue(coordinateWorldPosition);

                            newLayer.SetValue(coordinate, value);
                        }
                    });

                    newSurface.AddLayer(newLayer);
                    //surfaceLayer.ForceSurfaceResolution(true);
                }

                return newSurface;
            }



            public override SurfaceEntity Transform(SurfaceEntity surfaceEntity)
            {
                //surfaceEntity.CellSize = _parameterCellSize.Value;

                var newCellSize = _parameterCellSize.Value;

                var newSurface = ChangeCellSize(surfaceEntity, newCellSize);

                return newSurface;
            }
        }

        #endregion

        #region Change Resolution

        /// <summary>
        /// Changes the number of vertex columns or rows (by changing the cell size, internally), but maintaining the overall size.
        /// Especially useful to enforce certain resolutions before saving 
        /// </summary>
        public class ResolutionModify : SurfaceModifyParameter
        {
            /// <summary>
            /// The vertex count to enforce for the chosen target (i.e. columns or rows)
            /// </summary>
            private readonly IntParameter _parameterResolution = new IntParameter("Resolution", 128);

            /// <summary>
            /// The dimension to be changed. The other dimension will be changed accordingly to maintain the size.
            /// </summary>
            private readonly ChoiceParameter _parameterChoiceTarget = new ChoiceParameter("Target", "Columns", "Columns", "Rows");



            protected ResolutionModify()
                : base("Change Resolution")
            {
            }



            public override SurfaceEntity Transform(SurfaceEntity surfaceEntity)
            {
                var chosenSize = _parameterChoiceTarget.Value == "Columns" ? surfaceEntity.Width : surfaceEntity.Length;
                var newCellSize = chosenSize / (_parameterResolution.Value - 1);

                return CellSizeModify.ChangeCellSize(surfaceEntity, newCellSize);
            }
        }

        #endregion

        #region Layer Convert

        public abstract class LayerConversionTypeParameter : CompoundParameter
        {
            protected LayerConversionTypeParameter(string label) : base(label)
            {
            }



            public abstract SurfaceLayer Transform(SurfaceLayer surfaceLayer);
        }

        /// <summary>
        /// Converts a height layer to a blend layer.
        /// </summary>
        public class HeightToBlendLayerConversion : LayerConversionTypeParameter
        {
            /// <summary>
            /// The texture index of the newly created blend layer.
            /// </summary>
            private readonly IntParameter _parameterTextureIndex = new IntParameter("Texture Index", 0);



            public HeightToBlendLayerConversion()
                : base("Height to Blend")
            {
            }



            public override SurfaceLayer Transform(SurfaceLayer surfaceLayer)
            {
                if (surfaceLayer is HeightLayer)
                {
                    var heightLayer = (HeightLayer) surfaceLayer;

                    var numColumns = heightLayer.NumColumns;
                    var numRows = heightLayer.NumRows;
                    var newValues = new float[numColumns, numRows];
                    var height = heightLayer.Height;

                    ParallelHelper.For(0, numColumns, x =>
                    {
                        for (int y = 0; y < numRows; y++)
                            //convert to the 0-1 range
                            newValues[x, y] = heightLayer.RawValues[x, y] / height;
                    });

                    return new BlendLayer(newValues, _parameterTextureIndex.Value);
                }

                throw new ArgumentException("The given layer is not a height layer.");
            }
        }

        /// <summary>
        /// Converts a blend layer to a height layer.
        /// </summary>
        public class BlendToHeightLayerConversion : LayerConversionTypeParameter
        {
            /// <summary>
            /// The scale to multiply the blend layer.
            /// </summary>
            private readonly FloatParameter _parameterScale = new FloatParameter("Scale", 10);



            public BlendToHeightLayerConversion()
                : base("Blend to Height")
            {
            }



            public override SurfaceLayer Transform(SurfaceLayer surfaceLayer)
            {
                if (surfaceLayer is BlendLayer)
                {
                    var blendLayer = (BlendLayer) surfaceLayer;

                    var numColumns = blendLayer.NumColumns;
                    var numRows = blendLayer.NumRows;
                    var newValues = new float[numColumns, numRows];
                    var scale = _parameterScale.Value;

                    ParallelHelper.For(0, numColumns, x =>
                    {
                        for (int y = 0; y < numRows; y++)
                            //convert to the 0-1 range
                            newValues[x, y] = blendLayer.RawValues[x, y] * scale;
                    });

                    return new HeightLayer(newValues);
                }

                throw new ArgumentException("The given layer is not a blend layer.");
            }
        }

        /// <summary>
        /// Converts layers between different types.
        /// </summary>
        /// <seealso cref="Sceelix.Surfaces.Procedures.SurfaceModifyProcedure.SurfaceModifyParameter" />
        public class LayerConvertModify : SurfaceModifyParameter
        {
            /// <summary>
            /// Index of the layer to convert.
            /// </summary>
            private readonly IntParameter _parameterLayerIndex = new IntParameter("Layer Index", 0);

            /// <summary>
            /// Type of conversion (origin layer and destination layer) to perform.
            /// </summary> 
            private readonly SelectListParameter<LayerConversionTypeParameter> _parameterSelectConversionType = new SelectListParameter<LayerConversionTypeParameter>("Type", "Blend to Height");



            public LayerConvertModify()
                : base("Layer Convert")
            {
            }



            public override SurfaceEntity Transform(SurfaceEntity surfaceEntity)
            {
                var surfaceLayers = surfaceEntity.Layers.ToList();
                if (_parameterLayerIndex.Value >= surfaceLayers.Count)
                    throw new ArgumentException("The indicated layer index exceeds the amount of existing layers.");


                var layerToConvert = surfaceLayers[_parameterLayerIndex.Value];
                var convertedLayer = _parameterSelectConversionType.SelectedItem.Transform(layerToConvert);

                //if indeed a conversion took place, remove the old layer and add this one
                if (convertedLayer != layerToConvert)
                {
                    surfaceEntity.AddLayer(convertedLayer);
                    surfaceEntity.RemoveLayer(layerToConvert);
                }

                return surfaceEntity;
            }
        }

        #endregion
    }
}