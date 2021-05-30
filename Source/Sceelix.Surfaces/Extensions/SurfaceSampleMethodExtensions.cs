using System;
using Sceelix.Mathematics.Helpers;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Extensions
{
    public static class SurfaceSampleMethodExtensions
    {
        public static Func<int, int, int, int> ToIntFunction(this SampleMethod sampleMethod)
        {
            Func<int, int, int, int> coordinateHandleFunction = (value, min, max) => value;
            switch (sampleMethod)
            {
                case SampleMethod.Repeat:
                    coordinateHandleFunction = MathHelper.Repeat;
                    break;
                case SampleMethod.Clamp:
                    coordinateHandleFunction = MathHelper.Clamp;
                    break;
                case SampleMethod.Mirror:
                    coordinateHandleFunction = MathHelper.Mirror;
                    break;
            }

            return coordinateHandleFunction;
        }
    }
}