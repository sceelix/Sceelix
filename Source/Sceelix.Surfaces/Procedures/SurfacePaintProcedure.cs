using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Mathematics.Parameters;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Extensions;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Allows the definition of the splat map, which will handle the application of textures on the surface.
    /// </summary>
    [Procedure("690a154f-5fa0-4f60-bf12-25f25d764db0", Label = "Surface Paint", Category = "Surface")]
    public class SurfacePaintProcedure : SystemProcedure
    {
        /// <summary>
        /// The surface to be painted.
        /// </summary>
        /*private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");*/

        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (circle) input means that the node will be executed once per surface. <br/>
        /// Setting a <b>Collective</b> (square) input means that the node will be executed once for all surfaces. Especially useful when actors are to be painted on the surfaces, but it would be complex to match with the right surface.
        /// </summary>
        private readonly SingleOrCollectiveInputChoiceParameter<SurfaceEntity> _parameterInput = new SingleOrCollectiveInputChoiceParameter<SurfaceEntity>("Inputs", "Single");


        /// <summary>
        /// The surface that was painted.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");

        /// <summary>
        /// Painting designs to be applied.
        /// </summary>
        private readonly ListParameter<SurfacePaintingParameter> _parameterDesigns = new ListParameter<SurfacePaintingParameter>("Designs");



        protected override void Run()
        {
            var surfaceEntities = _parameterInput.Read();


            List<Action<SurfaceEntity, float[][,]>> functions = new List<Action<SurfaceEntity, float[][,]>>();
            foreach (var parameterDesign in _parameterDesigns.Items)
                //get the application function
                functions.Add(parameterDesign.GetApplyFunction());


            foreach (var surfaceEntity in surfaceEntities)
            {
                var surfaceLayers = surfaceEntity.Layers.OfType<BlendLayer>().ToList();

                if (_parameterDesigns.Items.Any())
                {
                    var maxTextureIndex = _parameterDesigns.Items.Max(x => x.ParameterTextureIndex.Value);

                    var maxLayerArrays = Math.Max(maxTextureIndex + 1, surfaceLayers.Count);

                    float[][,] blendValues = new float[maxLayerArrays][,];

                    for (int l = 0; l < maxLayerArrays; l++)
                        if (l >= surfaceLayers.Count)
                        {
                            var blendLayer = new BlendLayer(new float[surfaceEntity.NumColumns, surfaceEntity.NumRows], l);
                            surfaceEntity.AddLayer(blendLayer);
                            blendValues[l] = blendLayer.RawValues;
                        }
                        else
                        {
                            var blendLayer = surfaceLayers[l];
                            float[,] layerValues = blendValues[l] = new float[surfaceEntity.NumColumns, surfaceEntity.NumRows];
                            ParallelHelper.For(0, surfaceEntity.NumColumns, x =>
                            {
                                for (int y = 0; y < surfaceEntity.NumRows; y++) layerValues[x, y] = blendLayer.GetGenericValue(new Coordinate(x, y));
                            });

                            //add the new, readjusted blendlayer
                            //remove the previous one
                            //this does not affect the rest of the iteration, since we are working on a list copy
                            surfaceEntity.AddLayer(new BlendLayer(blendValues[l], l));
                            surfaceEntity.RemoveLayer(blendLayer);
                        }

                    foreach (var function in functions)
                        function(surfaceEntity, blendValues);
                }

                _output.Write(surfaceEntity);
            }
        }



        #region Abstract Parameter

        public abstract class SurfacePaintingParameter : CompoundParameter
        {
            /// <summary>
            /// The zero-based index of the texture to paint on the surface, as listed in a defined Multi Texture material.
            /// This will create a new blend texture for the given index, if one does not yet exist.
            /// </summary>
            protected internal readonly IntParameter ParameterTextureIndex = new IntParameter("Texture Index", 0) {MinValue = 0};



            protected SurfacePaintingParameter(string label)
                : base(label)
            {
            }



            public abstract Action<SurfaceEntity, float[][,]> GetApplyFunction();



            protected BlendLayer GetBlendLayer(SurfaceEntity entity)
            {
                var textureIndex = ParameterTextureIndex.Value;

                var blendLayerList = entity.Layers.OfType<BlendLayer>().ToList();
                for (int i = blendLayerList.Count; i <= textureIndex; i++) entity.AddLayer(new BlendLayer(new float[entity.NumColumns, entity.NumRows], i));

                return blendLayerList[textureIndex];
            }



            protected void SetDataIndex(int x, int y, float[][,] blendValues, int index, float newValue)
            {
                //following the idea that alpha blending consists of:
                //newColor = oldColor * (1 - value) + newColor * value
                for (int i = 0; i < blendValues.Length; i++)
                {
                    blendValues[i][x, y] *= 1 - newValue;
                    if (i == index)
                        blendValues[i][x, y] += newValue;
                }
            }



            protected void SetDataIndex(int x, int y, float[,,] values, int index, float newValue)
            {
                //if we are painting on top, we are always keeping the highest value for that texture
                newValue = Math.Max(newValue, values[x, y, index]);

                //the other values have to be proportionally lowered
                var inverseValue = 1 - newValue;
                for (int i = 0; i < 4; i++)
                    values[x, y, i] *= inverseValue;

                //now, assign the new value
                values[x, y, index] = newValue;

                //and keep the normalization, i.e. the sum of all must be 1
                var sum = values[x, y, 0] + values[x, y, 1] + values[x, y, 2] + values[x, y, 3];
                for (int i = 0; i < 4; i++)
                    values[x, y, i] /= sum;
            }
        }

        #endregion

        #region Uniform Splatting

        /// <summary>
        /// Paints the surface sections based on the direction of their normals.
        /// </summary>
        /// <seealso cref="SurfacePaintProcedure.SurfacePaintingParameter" />
        public class UniformPaintingParameter : SurfacePaintingParameter
        {
            private readonly FloatParameter _parameterValue = new FloatParameter("Value", 1);



            protected UniformPaintingParameter()
                : base("Uniform")
            {
            }



            public override Action<SurfaceEntity, float[][,]> GetApplyFunction()
            {
                return (surfaceEntity, values) =>
                {
                    //var index = Convert.ToInt32("" + _parameterTextureChoice.Value.Last()) - 1;
                    var textureIndex = ParameterTextureIndex.Value;
                    var value = _parameterValue.Value;

                    ParallelHelper.For(0, surfaceEntity.NumColumns, x =>
                    {
                        for (int y = 0; y < surfaceEntity.NumRows; y++)
                            //and finally, set the data
                            SetDataIndex(x, y, values, textureIndex, value);
                    });
                };
            }
        }

        #endregion

        #region Direction Splatting

        /// <summary>
        /// Paints the surface sections based on the direction of their normals.
        /// </summary>
        /// <seealso cref="SurfacePaintProcedure.SurfacePaintingParameter" />
        public class DirectionPaintingParameter : SurfacePaintingParameter
        {
            /// <summary>
            /// The normal direction to compare to. The intensity of the texture at a surface point will depend of the how its normal differs from this one.
            /// </summary>
            private readonly Vector3DParameter _parameterDirection = new Vector3DParameter("Surface Normal", Vector3D.ZVector);

            /// <summary>
            /// The maximum angle to perform the comparison between the indicated surface normal and the normal at each point.
            /// </summary>
            private readonly FloatParameter _parameterAngle = new FloatParameter("Max. Angle", 45f);

            /// <summary>
            /// Intensity function to apply. Useful to define smoothing effects.
            /// </summary>
            private readonly SelectListParameter<HeightFunctionParameter> _parameterHeightFunction = new SelectListParameter<HeightFunctionParameter>("Function", "Constant") {IsExpandedAsDefault = false};



            protected DirectionPaintingParameter()
                : base("Direction")
            {
            }



            public override Action<SurfaceEntity, float[][,]> GetApplyFunction()
            {
                return (surfaceEntity, values) =>
                {
                    //var index = Convert.ToInt32("" + _parameterTextureChoice.Value.Last()) - 1;
                    var textureIndex = ParameterTextureIndex.Value;

                    var direction = _parameterDirection.Value.Normalize();
                    var invert = _parameterAngle.Value < 0;
                    var absAngle = Math.Abs(_parameterAngle.Value);
                    var angleMultiplier = 90f / absAngle;
                    var clampValue = MathHelper.ToRadians(absAngle);

                    var function = _parameterHeightFunction.SelectedItem.GetFunction();

                    var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
                    var normalLayer = surfaceEntity.GetLayer<NormalLayer>();


                    ParallelHelper.For(0, surfaceEntity.NumColumns, x =>
                    {
                        for (int y = 0; y < surfaceEntity.NumRows; y++)
                        {
                            var coordinate = new Coordinate(x, y);

                            //obtain the normal of the surface at that point
                            var normal = normalLayer != null ? normalLayer.GetGenericValue(coordinate) : heightLayer.CalculateNormal(coordinate);

                            //calculate the angle between the requested direction and the normal
                            var angle = normal.AngleTo(direction);

                            //to avoid repetitions, clamp the value to the maximum angle defined
                            angle = MathHelper.Clamp(angle, 0, clampValue);

                            //determine the 
                            var value = (float) Math.Cos(angle * angleMultiplier);

                            //the value can go below 0, so keep it above
                            value = MathHelper.Clamp(value, 0, 1f);

                            //if requested by parameter, invert the
                            if (invert)
                                value = 1 - value;

                            //apply the function
                            value *= function(value);
                            //value = MathHelper.Clamp(value, 0, 1);

                            //and finally, set the data
                            SetDataIndex(x, y, values, textureIndex, value);
                        }
                    });
                };
            }
        }

        #endregion

        /// <summary>
        /// Paints layers from other surfaces on the surface.
        /// </summary>
        public class SurfaceOnSurfacePainting : SurfacePaintingParameter
        {
            /// <summary>
            /// The surface that is to be painted on the main surface.
            /// </summary>
            private readonly SingleInput<SurfaceEntity> _inputSurface = new SingleInput<SurfaceEntity>("Surface");

            /// <summary>
            /// The surface that was painted on the main surface.
            /// </summary>
            private readonly Output<SurfaceEntity> _outputSurface = new Output<SurfaceEntity>("Surface");


            /// <summary>
            /// Index of the layer which contains the intensity information. Should be a floating-point based layer, such as a height or blend layer.<br/>
            /// By default, the index 0 means the height layer.
            /// </summary>
            private readonly IntParameter _parameterLayerIndex = new IntParameter("Index", 0) {MinValue = 0};

            /// <summary>
            /// Intensity function to apply. Useful to define smoothing effects.
            /// </summary>
            private readonly SelectListParameter<HeightFunctionParameter> _parameterHeightFunction = new SelectListParameter<HeightFunctionParameter>("Function", "Constant") {IsExpandedAsDefault = false};



            public SurfaceOnSurfacePainting()
                : base("Surface")
            {
            }



            public override Action<SurfaceEntity, float[][,]> GetApplyFunction()
            {
                //check the inputs
                var surface = _inputSurface.Read();

                var surfaceLayers = surface.Layers.ToList();
                if (_parameterLayerIndex.Value >= surfaceLayers.Count)
                    throw new ArgumentException("The indicated layer index exceeds the amount of existing layers.");

                var floatLayer = surfaceLayers[_parameterLayerIndex.Value] as FloatLayer;
                if (floatLayer == null)
                    throw new ArgumentException("The indicated layer index does not correspond to a valid floating-point based layer.");

                var textureIndex = ParameterTextureIndex.Value;
                float minValue = floatLayer.MinValue, maxValue = floatLayer.MaxValue;

                var heightFunctionParameter = _parameterHeightFunction.SelectedItem.GetFunction();

                _outputSurface.Write(surface);

                return (surfaceEntity, values) =>
                {
                    ParallelHelper.For(0, surfaceEntity.NumColumns, x =>
                    {
                        for (int y = 0; y < surfaceEntity.NumRows; y++)
                        {
                            var worldPosition = surfaceEntity.ToWorldPosition(new Coordinate(x, y));
                            var value = (floatLayer.GetGenericValue(worldPosition) - minValue) / (maxValue - minValue);

                            value = heightFunctionParameter(value) * value;

                            SetDataIndex(x, y, values, textureIndex, value);
                        }
                    });
                };
            }
        }

        #region Functions

        public abstract class HeightFunctionParameter : CompoundParameter
        {
            protected HeightFunctionParameter(string label)
                : base(label)
            {
            }



            public abstract Func<float, float> GetFunction();
        }

        /// <summary>
        /// The same value will be applied, regardless of input.
        /// </summary>
        /// <seealso cref="SurfacePaintProcedure.HeightFunctionParameter" />
        public class ConstFunctionParameter : HeightFunctionParameter
        {
            /// <summary>
            /// Value to be returned.
            /// </summary>
            private readonly FloatParameter _parameterValue = new FloatParameter("Value", 1f)
            {
                MinValue = 0,
                MaxValue = 1,
                Increment = 0.001f
            };



            public ConstFunctionParameter()
                : base("Constant")
            {
            }



            public override Func<float, float> GetFunction()
            {
                var value = _parameterValue.Value;
                return x => value;
            }
        }

        /// <summary>
        /// The value will vary linearly between the defined values.
        /// </summary>
        /// <seealso cref="SurfacePaintProcedure.HeightFunctionParameter" />
        public class LinearFunctionParameter : HeightFunctionParameter
        {
            /// <summary>
            /// The start value.
            /// </summary>
            private readonly FloatParameter _parameterStartValue = new FloatParameter("Start", 0f)
            {
                MinValue = 0,
                MaxValue = 1,
                Increment = 0.01f
            };

            /// <summary>
            /// The end value.
            /// </summary>
            private readonly FloatParameter _parameterEndValue = new FloatParameter("End", 1f)
            {
                MinValue = 0,
                MaxValue = 1,
                Increment = 0.01f
            };



            public LinearFunctionParameter()
                : base("Linear")
            {
            }



            public override Func<float, float> GetFunction()
            {
                var startValue = _parameterStartValue.Value;
                var multiplier = _parameterEndValue.Value - _parameterStartValue.Value;

                return x => startValue + multiplier * x;
            }
        }


        /*public class ExponentialFunctionParameter : HeightFunctionParameter
        {
            public ExponentialFunctionParameter() 
                : base("Exponential")
            {
            }

            public override float GetFunctionValue(float x)
            {
                return (float) Math.Exp(x);
            }

            public override void SetupData()
            {
            }
        }*/

        #endregion

        #region Height Splatting

        #region Limits Parameter

        public abstract class HeightLimitsParameter : CompoundParameter
        {
            //private readonly BoolParameter ParameterRelative = new BoolParameter("Relative", true);



            protected HeightLimitsParameter(string label)
                : base(label)
            {
            }



            public abstract float ActualEnd(SurfaceEntity entity);


            public abstract float ActualStart(SurfaceEntity entity);
        }

        /// <summary>
        /// Defines limits in absolute values.
        /// </summary>
        /// <seealso cref="SurfacePaintProcedure.HeightLimitsParameter" />
        public class AbsoluteLimitsParameter : HeightLimitsParameter
        {
            /// <summary>
            /// The starting height where to paint the texture.
            /// </summary>
            private readonly FloatParameter _parameterStart = new FloatParameter("Start", 0f) {MinValue = 0};

            /// <summary>
            /// The ending height where to paint the texture.
            /// </summary>
            private readonly FloatParameter _parameterEnd = new FloatParameter("End", 100f) {MinValue = 0};



            public AbsoluteLimitsParameter()
                : base("Absolute")
            {
            }



            public override float ActualEnd(SurfaceEntity entity)
            {
                return _parameterEnd.Value;
            }



            public override float ActualStart(SurfaceEntity entity)
            {
                return _parameterStart.Value;
            }
        }

        /// <summary>
        /// Defines limits in surface-relative values (between 0 and 1).
        /// </summary>
        public class RelativeLimitsParameter : HeightLimitsParameter
        {
            /// <summary>
            /// The starting height where to paint the texture.
            /// </summary>
            private readonly FloatParameter _parameterStart = new FloatParameter("Start", 0f)
            {
                MinValue = 0,
                MaxValue = 1
            };

            /// <summary>
            /// The ending height where to paint the texture.
            /// </summary>
            private readonly FloatParameter _parameterEnd = new FloatParameter("End", 1f) {MinValue = 0, MaxValue = 1};



            public RelativeLimitsParameter()
                : base("Relative")
            {
            }



            public override float ActualEnd(SurfaceEntity entity)
            {
                return _parameterEnd.Value * entity.Height + entity.MinimumZ;
            }



            public override float ActualStart(SurfaceEntity entity)
            {
                return _parameterStart.Value * entity.Height + entity.MinimumZ;
            }
        }

        #endregion

        /// <summary>
        /// Paints the surface sections based on their height. 
        /// </summary>
        /// <seealso cref="SurfacePaintProcedure.SurfacePaintingParameter" />
        public class HeightPaintingParameter : SurfacePaintingParameter
        {
            /// <summary>
            /// The height limits between which the painting should occur.
            /// </summary>
            private readonly SelectListParameter<HeightLimitsParameter> _parameterHeightLimits = new SelectListParameter<HeightLimitsParameter>("Limits", "Relative") {IsExpandedAsDefault = false};

            /// <summary>
            /// Intensity function to apply. Useful to define smoothing effects.
            /// </summary>
            private readonly SelectListParameter<HeightFunctionParameter> _parameterHeightFunction = new SelectListParameter<HeightFunctionParameter>("Function", "Constant") {IsExpandedAsDefault = false};



            protected HeightPaintingParameter()
                : base("Height")
            {
            }



            public override Action<SurfaceEntity, float[][,]> GetApplyFunction()
            {
                return (surfaceEntity, values) =>
                {
                    var textureIndex = ParameterTextureIndex.Value;

                    var limits = _parameterHeightLimits.Items.First();
                    var start = limits.ActualStart(surfaceEntity);
                    var end = limits.ActualEnd(surfaceEntity);
                    var size = end - start;

                    if (start >= end)
                        throw new Exception("Starting limit must be lower than ending limit.");

                    var function = _parameterHeightFunction.SelectedItem.GetFunction();

                    var heightLayer = surfaceEntity.GetLayer<HeightLayer>();

                    ParallelHelper.For(0, surfaceEntity.NumColumns, x =>
                    {
                        for (int y = 0; y < surfaceEntity.NumRows; y++)
                        {
                            var height = heightLayer != null ? heightLayer.GetGenericValue(new Coordinate(x, y)) : 0;

                            if (height >= start && height <= end)
                            {
                                var relativeHeight = (height - start) / size;

                                SetDataIndex(x, y, values, textureIndex, function(relativeHeight));
                                //values[x, y, index] = function.GetFunctionValue(relativeHeight);
                            }
                        }
                    });
                };
            }
        }

        #endregion
    }
}