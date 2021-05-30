using System.Collections.Generic;
using System.Linq;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Algorithms;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Extensions
{
    public static class ClipperExtension
    {
        private const float Precision = 10000;



        public static IEnumerable<Face> PolyTreeToFaceList(this PolyNode polyNode, BoxScope alignedScope, Face parentFace = null, bool reverse = true)
        {
            Face futureParent = null;

            if (polyNode.IsHole && parentFace != null)
            {
                var positions = polyNode.Contour.Select(point => point.ToVector3D(alignedScope));

                if (reverse)
                    positions = positions.Reverse();

                parentFace.AddHole(positions);
            }
            else if (polyNode.Contour.Any())
            {
                var positions = polyNode.Contour.Select(point => point.ToVector3D(alignedScope));

                if (reverse)
                    positions = positions.Reverse();

                futureParent = new Face(positions);

                yield return futureParent;
            }

            foreach (Face subface in polyNode.Childs.SelectMany(x => PolyTreeToFaceList(x, alignedScope, futureParent, reverse)))
                yield return subface;
        }



        public static IntPoint ToIntPoint(this Vector3D vector3D, BoxScope planarScope)
        {
            var rotatedVector = planarScope.ToScopePosition(vector3D).Round(3);

            return new IntPoint(rotatedVector.X * Precision, rotatedVector.Y * Precision);
        }



        public static Vector3D ToVector3D(this IntPoint intpoint, BoxScope planarScope)
        {
            var vector3D = new Vector3D(intpoint.X / Precision, intpoint.Y / Precision);

            return planarScope.ToWorldPosition(vector3D);
        }
    }
}