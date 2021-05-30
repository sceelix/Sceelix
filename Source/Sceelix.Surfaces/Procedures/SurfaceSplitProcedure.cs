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
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Splits the input surface into several slices according
    /// to specific constraints.
    /// </summary>
    [Procedure("a9e64925-57dc-4211-92cb-6c2eb4f0e98c", Label = "Surface Split", Category = "Surface")]
    public class SurfaceSplitProcedure : SystemProcedure
    {
        /// <summary>
        /// Surface that is meant to be split.
        /// </summary>
        private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");

        /// <summary>
        /// The planes/axes that define the direction of the splitting.
        /// </summary>
        private readonly ChoiceParameter _parameterAxis = new ChoiceParameter("Axis", "X", "X", "Y");


        /// <summary>
        /// Indicates if the cut should start from right to left (in case the chosen axis is "X") or from top to bottom (in the case of "Y")
        /// </summary>
        private readonly BoolParameter _parameterInvert = new BoolParameter("Invert", false);


        /// <summary>
        /// List of slices to split.
        /// </summary>
        private readonly ListParameter _parameterSplits = new ListParameter("Slices",
            () => new RepetitiveSliceParameter {Description = "Splits the meshes repeatedly while there is available spacing."},
            () => new SliceParameter());


        /// <summary>
        /// Attribute where the index of the surface slice will be stored.
        /// </summary>
        private readonly AttributeParameter<int> _attributeIndex = new AttributeParameter<int>("Index", AttributeAccess.Write);

        /// <summary>
        /// Attribute where the total number of surface slices will be stored. 
        /// </summary>
        private readonly AttributeParameter<int> _attributeCount = new AttributeParameter<int>("Count", AttributeAccess.Write);



        private IEnumerable<SliceParameter> GetSplitList(ListParameter parameterSplits)
        {
            foreach (var parameter in _parameterSplits.Items)
            {
                var repetitiveSliceParameter = parameter as RepetitiveSliceParameter;
                if (repetitiveSliceParameter != null)
                {
                    for (int i = 0; i < repetitiveSliceParameter.Repetitions; i++)
                        foreach (var sliceParameter in repetitiveSliceParameter.Items)
                            yield return sliceParameter;
                }
                else
                {
                    var sliceParameter = (SliceParameter) parameter;
                    yield return sliceParameter;
                }
            }
        }



        protected override void Run()
        {
            var surfaceEntity = _input.Read();

            var normalOrInvertSign = _parameterInvert.Value ? -1 : 1;
            var normalOrInvertInclusion = _parameterInvert.Value ? 1 : 0;


            var availableSize = _parameterAxis.Value == "X" ? surfaceEntity.Width : surfaceEntity.Length;
            bool completelyCut = SetupSlices(_parameterSplits.Items.OfType<ISliceParameter>().ToList(), availableSize, surfaceEntity.CellSize);


            var sliceParameters = GetSplitList(_parameterSplits).ToList();

            var increment = _parameterAxis.Value == "X" ? new Vector2D(normalOrInvertSign, 0) : new Vector2D(0, normalOrInvertSign);
            var otherAxisSize = _parameterAxis.Value == "X" ? new Vector2D(0, surfaceEntity.Length) : new Vector2D(surfaceEntity.Width, 0);
            var initialOffset = _parameterAxis.Value == "X" ? new Vector2D(surfaceEntity.Width * normalOrInvertInclusion, 0) : new Vector2D(0, surfaceEntity.Length * normalOrInvertInclusion);

            List<SurfaceLayer>[] dividedLayers = new List<SurfaceLayer>[sliceParameters.Count];
            for (int i = 0; i < dividedLayers.Length; i++)
                dividedLayers[i] = new List<SurfaceLayer>();


            Vector2D startingPoint = surfaceEntity.Origin + initialOffset;
            for (int i = 0; i < sliceParameters.Count; i++)
            {
                var sliceParameter = sliceParameters[i];

                var sizedIncrement = increment * sliceParameter.ActualSize;

                var sliceStartingPoint = startingPoint;
                var sliceEndingPoint = startingPoint + sizedIncrement + otherAxisSize;

                var temp = Vector2D.Minimize(sliceStartingPoint, sliceEndingPoint);
                sliceEndingPoint = Vector2D.Maximize(sliceStartingPoint, sliceEndingPoint);
                sliceStartingPoint = temp;

                //if (sliceStartingPoint.X > sliceEndingPoint.X || sliceStartingPoint.Y > sliceEndingPoint.Y)
                //    ObjectHelper.Swap(ref sliceStartingPoint, ref sliceEndingPoint);

                var sliceStartingPointCoordinates = surfaceEntity.ToCoordinates(sliceStartingPoint, roundingMethod: RoundingMethod.Nearest);
                var sliceEndingPointCoordinates = surfaceEntity.ToCoordinates(sliceEndingPoint, roundingMethod: RoundingMethod.Nearest);

                var numColumns = Math.Abs(sliceEndingPointCoordinates.X - sliceStartingPointCoordinates.X) + 1;
                var numRows = Math.Abs(sliceStartingPointCoordinates.Y - sliceEndingPointCoordinates.Y) + 1;

                var cellSize = surfaceEntity.CellSize;
                if (sliceParameter.ActualSize % surfaceEntity.CellSize > 0)
                    cellSize = _parameterAxis.Value == "X" ? sliceParameter.ActualSize / (numColumns - 1) : sliceParameter.ActualSize / (numRows - 1);


                var newSurface = new SurfaceEntity(numColumns, numRows, cellSize);
                newSurface.Origin = sliceStartingPoint;
                surfaceEntity.Attributes.SetAttributesTo(newSurface.Attributes);
                newSurface.Material = surfaceEntity.Material;

                foreach (var surfaceLayer in surfaceEntity.Layers)
                {
                    var newLayer = surfaceLayer.CreateEmpty(numColumns, numRows);
                    newSurface.AddLayer(newLayer);

                    //for (int k = 0; k < newLayer.NumColumns; k++)
                    var layer = surfaceLayer;
                    ParallelHelper.For(0, newLayer.NumColumns, k =>
                    {
                        for (int l = 0; l < newLayer.NumRows; l++)
                        {
                            var coordinate = new Coordinate(k, l);
                            var worldPosition = newSurface.ToWorldPosition(coordinate);
                            var objectValue = layer.GetValue(worldPosition);
                            newLayer.SetValue(coordinate, objectValue);
                        }
                    });
                }

                _attributeIndex[newSurface] = i;
                _attributeCount[newSurface] = sliceParameters.Count;

                sliceParameters[i].Output.Write(newSurface);

                startingPoint += sizedIncrement;
            }
        }



        private static bool SetupSlices(List<ISliceParameter> splitParameters, float totalSize, float cellSize)
        {
            //step 1: calculate relative sizes
            splitParameters.ForEach(x => x.CalculateDesiredSizes(totalSize, cellSize));

            float totalDesiredSize = splitParameters.OfType<SliceParameter>().Sum(x => x.DesiredSize);

            //if this is the case, we don't even consider subitems/repetitive splices
            if (totalDesiredSize > totalSize)
            {
                int numFlexibleItems = splitParameters.Sum(x => x.NumFlexibleItems);
                float spaceToRemove = 0;
                if (numFlexibleItems > 0)
                    spaceToRemove = (totalDesiredSize - totalSize) / numFlexibleItems;


                foreach (var splitParameter in splitParameters)
                    if (splitParameter is RepetitiveSliceParameter)
                    {
                        ((RepetitiveSliceParameter) splitParameter).Repetitions = 0;
                    }
                    else if (splitParameter is SliceParameter)
                    {
                        SliceParameter slice = (SliceParameter) splitParameter;

                        if (slice.IsFlexible)
                            slice.ActualSize = Math.Max(0, slice.DesiredSize - spaceToRemove);
                        else
                            slice.ActualSize = slice.DesiredSize;
                    }

                float consumableSpace = totalSize;

                //if the slices go above the maximum size, make their size 0
                foreach (var slice in splitParameters.OfType<SliceParameter>())
                    if (consumableSpace < slice.ActualSize)
                    {
                        slice.ActualSize = consumableSpace;
                        consumableSpace = 0;
                    }
                    else
                    {
                        consumableSpace -= slice.ActualSize;
                    }

                return true;
            }

            var allSliceParameters = splitParameters.SelectMany(x => x.AllSubSlices).ToList();

            //determine how much room we have left to share among repeat splits
            var availableSpaceToSplit = totalSize - totalDesiredSize;
            var repetitiveSliceParameters = splitParameters.OfType<RepetitiveSliceParameter>().ToList();
            float amountToShare = availableSpaceToSplit / repetitiveSliceParameters.Count;

            foreach (var repetitiveSliceParameter in repetitiveSliceParameters)
            {
                float takenSpace = repetitiveSliceParameter.UpdateRepetitions(amountToShare);
                totalDesiredSize += takenSpace;
            }

            //calculate the available space to share with the flexibles
            availableSpaceToSplit = totalSize - totalDesiredSize;
            var spaceForEachFlexible = availableSpaceToSplit / splitParameters.Sum(x => x.NumFlexibleItems);
            spaceForEachFlexible = MathHelper.FloorToFactor(spaceForEachFlexible, cellSize);

            foreach (var sliceParameter in allSliceParameters)
                if (sliceParameter.IsFlexible)
                    sliceParameter.ActualSize = sliceParameter.DesiredSize + spaceForEachFlexible;
                else
                    sliceParameter.ActualSize = sliceParameter.DesiredSize;

            return allSliceParameters.Any(x => x.IsFlexible) || Math.Abs(totalDesiredSize - totalSize) < float.Epsilon;
        }



        #region Slice Parameters

        public interface ISliceParameter
        {
            IEnumerable<SliceParameter> AllSubSlices
            {
                get;
            }


            int NumFlexibleItems
            {
                get;
            }



            //float Measure(float availableSpace);
            //bool IsFlexible { get; }
            void CalculateDesiredSizes(float totalSize, float cellSize);

            void Reset();
        }

        /// <summary>
        /// Splits the meshes repeatedly while there is available spacing.
        /// </summary>
        public class RepetitiveSliceParameter : ListParameter<SliceParameter>, ISliceParameter
        {
            public RepetitiveSliceParameter()
                : base("Repetitive Slice", () => new SliceParameter()) //() => new RepetitiveSliceParameter(),
            {
            }



            public IEnumerable<SliceParameter> AllSubSlices
            {
                get { return Items.SelectMany(x => ((ISliceParameter) x).AllSubSlices); }
            }



            public int NumFlexibleItems
            {
                get { return Items.Sum(x => x.NumFlexibleItems) * Repetitions; }
            }



            public int Repetitions
            {
                get;
                set;
            }



            public void CalculateDesiredSizes(float totalSize, float cellSize)
            {
                foreach (ISliceParameter splitParameter in Items)
                    splitParameter.CalculateDesiredSizes(totalSize, cellSize);
            }



            public void Reset()
            {
                Repetitions = 0;

                foreach (var sliceParameter in Items)
                    sliceParameter.Reset();
            }



            public float UpdateRepetitions(float availableSpace)
            {
                var sum = Items.Sum(x => x.DesiredSize);

                Repetitions = (int) (availableSpace / sum);
                return sum * Repetitions;
            }
        }

        /// <summary>
        /// Splits a mesh piece with a given size.
        /// </summary>
        public class SliceParameter : CompoundParameter, ISliceParameter
        {
            /// <summary>
            /// The mesh section that was split.
            /// </summary>
            internal readonly Output<SurfaceEntity> Output = new Output<SurfaceEntity>("Output");

            /// <summary>
            /// Size of the slice.
            /// </summary>
            internal readonly FloatParameter SizeParameter = new FloatParameter("Size", 1);

            /// <summary>
            /// If "Absolute", the value of "Amount" will be considered as an absolute value, if "Relative", the value will be a percentage. (0 - 1)
            /// </summary>
            internal readonly ChoiceParameter SizingParameter = new ChoiceParameter("Sizing", "Absolute", "Absolute", "Relative");

            /// <summary>
            /// If checked, the size of this slice will be adjusted according to the available space.
            /// </summary>
            internal readonly BoolParameter FlexibleParameter = new BoolParameter("Flexible", true);



            public SliceParameter()
                : base("Slice")
            {
            }



            internal float ActualSize
            {
                get;
                set;
            }



            public IEnumerable<SliceParameter> AllSubSlices
            {
                get { yield return this; }
            }



            internal float DesiredSize
            {
                get;
                set;
            }


            public bool IsFlexible => FlexibleParameter.Value;


            public int NumFlexibleItems => IsFlexible ? 1 : 0;



            public void CalculateDesiredSizes(float totalSize, float cellSize)
            {
                if (SizingParameter.Value == "Relative")
                    DesiredSize = SizeParameter.Value * totalSize;
                else
                    DesiredSize = SizeParameter.Value;

                DesiredSize = MathHelper.RoundToFactor(DesiredSize, cellSize);
            }



            public void Reset()
            {
                DesiredSize = ActualSize = 0;
            }
        }

        #endregion
    }
}