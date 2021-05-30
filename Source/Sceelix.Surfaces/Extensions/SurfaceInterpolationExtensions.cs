using System;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Extensions
{
    public static class SurfaceInterpolationExtensions
    {
        private static object BilinearInterpolation(SurfaceLayer surfaceLayer, float fractionX, float fractionY, object[] values)
        {
            return surfaceLayer.Add(surfaceLayer.MultiplyScalar(surfaceLayer.Add(surfaceLayer.MultiplyScalar(values[0], 1 - fractionY), surfaceLayer.MultiplyScalar(values[2], fractionY)), 1 - fractionX), surfaceLayer.MultiplyScalar(surfaceLayer.Add(surfaceLayer.MultiplyScalar(values[1], 1 - fractionY), surfaceLayer.MultiplyScalar(values[3], fractionY)), fractionX));
        }



        public static Func<float, float, object[], object> ToObjectFunction(this SurfaceInterpolation interpolation, SurfaceLayer surfaceLayer)
        {
            Func<float, float, object[], object> interpolationFunction = null;
            switch (interpolation)
            {
                case SurfaceInterpolation.TopLeft:
                    interpolationFunction = (fractionX, fractionY, values) => TopLeftInterpolation(surfaceLayer, fractionX, fractionY, values);
                    break;
                case SurfaceInterpolation.TopRight:
                    interpolationFunction = (fractionX, fractionY, values) => TopRightInterpolation(surfaceLayer, fractionX, fractionY, values);
                    break;
                case SurfaceInterpolation.Bilinear:
                    interpolationFunction = (fractionX, fractionY, values) => BilinearInterpolation(surfaceLayer, fractionX, fractionY, values);
                    break;
            }

            return interpolationFunction;
        }



        private static object TopLeftInterpolation(SurfaceLayer surfaceLayer, float fractionX, float fractionY, object[] values)
        {
            if (fractionX < fractionY) //the bottom triangle
                return surfaceLayer.Add(surfaceLayer.Add(surfaceLayer.MultiplyScalar(values[0], 1 - fractionY), surfaceLayer.MultiplyScalar(values[3], fractionX)), surfaceLayer.MultiplyScalar(values[2], fractionY - fractionX));

            //the upper triangle
            return surfaceLayer.Add(surfaceLayer.Add(surfaceLayer.MultiplyScalar(values[0], 1 - fractionX), surfaceLayer.MultiplyScalar(values[3], fractionY)), surfaceLayer.MultiplyScalar(values[1], fractionX - fractionY));
        }



        private static object TopRightInterpolation(SurfaceLayer surfaceLayer, float fractionX, float fractionY, object[] values)
        {
            if (fractionX < 1 - fractionY) //  fractionX < 0.5 || fractionY < 0.5   the upper left triangle
                return surfaceLayer.Add(surfaceLayer.Add(surfaceLayer.MultiplyScalar(values[1], fractionX), surfaceLayer.MultiplyScalar(values[2], fractionY)), surfaceLayer.MultiplyScalar(values[0], 1 - fractionX - fractionY));

            //the lower triangle
            return surfaceLayer.Add(surfaceLayer.Add(surfaceLayer.MultiplyScalar(values[1], 1 - fractionY), surfaceLayer.MultiplyScalar(values[2], 1 - fractionX)), surfaceLayer.MultiplyScalar(values[3], fractionY - (1 - fractionX)));
        }
    }
}