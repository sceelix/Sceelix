using System.Collections.Generic;
using System.Linq;
using Sceelix.Mathematics.Geometry;
using Sceelix.Mathematics.Helpers;
using Sceelix.Mathematics.Spatial;

namespace Sceelix.Mathematics.Data
{
    /// <summary>
    /// The BoxScope is used for keeping track of the location, adation and size of entities.
    /// 
    /// Note: Due to being a struct, all the axes, sizes and translation are initialized
    /// with Zero vectors when the default BoxScope() constructor is used. For most practical
    /// matters, the BoxScope.Identity should be used.
    /// </summary>
    public struct BoxScope //: IDeepCloneable<BoxScope>
    {
        /// <summary>
        /// Creates an world axis oriented scope based on the given bounding box.
        /// </summary>
        public BoxScope(BoundingBox boundingBox)
        {
            XAxis = Vector3D.XVector;
            YAxis = Vector3D.YVector;
            ZAxis = Vector3D.ZVector;
            Sizes = new Vector3D(boundingBox.Width, boundingBox.Length, boundingBox.Height);
            Translation = boundingBox.Min;
        }



        /// <summary>
        /// Creates a custom boxscope based on the given axis directions and translation.
        /// Sizes are initialized to 0.
        /// </summary>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        /// <param name="translation"></param>
        public BoxScope(Vector3D xAxis, Vector3D yAxis, Vector3D zAxis, Vector3D translation)
        {
            XAxis = xAxis;
            YAxis = yAxis;
            ZAxis = zAxis;
            Translation = translation;
            Sizes = Vector3D.Zero;
        }



        public BoxScope(Vector3D xAxis, Vector3D yAxis, Vector3D zAxis, Vector3D translation, Vector3D sizes)
        {
            XAxis = xAxis;
            YAxis = yAxis;
            ZAxis = zAxis;
            Translation = translation;
            Sizes = sizes;
        }



        /// <summary>
        /// Creates a boxscope instance from a set of 
        /// </summary>
        /// <param name="xAxis">The x axis direction. If null, will be set to (1,0,0), unless the other two axes are not null, in which case it will be calculated from their cross product</param>
        /// <param name="yAxis">The y axis direction. If null, will be set to (0,1,0), unless the other two axes are not null, in which case it will be calculated from their cross product</param>
        /// <param name="zAxis">The z axis direction. If null, will be set to (0,0,1), unless the other two axes are not null, in which case it will be calculated from their cross product.</param>
        /// <param name="translation">The boxscope offset/translation. If null, will be set to (0,0,0).</param>
        /// <param name="sizes">The sizes of the 3 axes. If null, will be set to (0,0,0).</param>
        public BoxScope(Vector3D? xAxis = null, Vector3D? yAxis = null, Vector3D? zAxis = null, Vector3D? translation = null, Vector3D? sizes = null)
        {
            XAxis = xAxis ?? (yAxis.HasValue && zAxis.HasValue ? yAxis.Value.Cross(zAxis.Value) : Vector3D.XVector);
            YAxis = yAxis ?? (xAxis.HasValue && zAxis.HasValue ? zAxis.Value.Cross(xAxis.Value) : Vector3D.YVector);
            ZAxis = zAxis ?? (xAxis.HasValue && yAxis.HasValue ? xAxis.Value.Cross(yAxis.Value) : Vector3D.ZVector);

            /*_xAxis = xAxis ?? Vector3D.XVector;
            _yAxis = yAxis ?? Vector3D.YVector;
            _zAxis = zAxis ?? Vector3D.ZVector;*/
            Translation = translation ?? Vector3D.Zero;
            Sizes = sizes ?? Vector3D.Zero;
        }



        /// <summary>
        /// Creates an axis-oriented scope based on the given positions.
        /// </summary>
        /// <param name="positions"></param>
        public BoxScope(IEnumerable<Vector3D> positions)
            : this(BoundingBox.FromPoints(positions))
        {
        }



        public BoxScope(BoxScope boxScope, Vector3D? xAxis = null, Vector3D? yAxis = null, Vector3D? zAxis = null, Vector3D? translation = null, Vector3D? sizes = null)
        {
            XAxis = xAxis ?? boxScope.XAxis;
            YAxis = yAxis ?? boxScope.YAxis;
            ZAxis = zAxis ?? boxScope.ZAxis;
            Translation = translation ?? boxScope.Translation;
            Sizes = sizes ?? boxScope.Sizes;
        }



        #region Methods

        /// <summary>
        /// Grows the scope by, while keeping orientation, upgrades
        /// the translation and sizes so as to incorporate the new
        /// given points as well.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public BoxScope Grow(IEnumerable<Vector3D> positions)
        {
            Plane3D planeX = new Plane3D(XAxis, Translation);
            Plane3D planeY = new Plane3D(YAxis, Translation);
            Plane3D planeZ = new Plane3D(ZAxis, Translation);

            var sizes = Sizes; //new Vector3D();

            //Different from above: this needs to check every point of the face
            foreach (Vector3D vector3D in positions)
            {
                float x = Plane3DHelper.MovePlane(ref planeX, vector3D, sizes.X);
                float y = Plane3DHelper.MovePlane(ref planeY, vector3D, sizes.Y);
                float z = Plane3DHelper.MovePlane(ref planeZ, vector3D, sizes.Z);

                sizes = new Vector3D(x, y, z);
            }

            var xAxis = planeX.Normal;
            var yAxis = planeY.Normal;
            var zAxis = planeZ.Normal;

            var translation = Plane3D.CalculateIntersection(planeX, planeY, planeZ);

            return new BoxScope(xAxis, yAxis, zAxis, translation, sizes);
        }



        /// <summary>
        /// Maintains the orientation, but resets the translation and sizes
        /// and recalculates them based on the given set of points.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public BoxScope Adjust(IEnumerable<Vector3D> positions)
        {
            var positionsList = positions.ToList();
            if (!positionsList.Any())
                return this;

            var resettedScope = new BoxScope(XAxis, YAxis, ZAxis, positionsList.First(), new Vector3D());

            return resettedScope.Grow(positionsList.Skip(1));
        }



        /// <summary>
        /// Converts a coordinate in ABSOLUTE world coordinates into this scope's ABSOLUTE coordinates.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Vector3D ToScopePosition(Vector3D position)
        {
            Vector3D relativePosition = position - Translation;

            float dX = relativePosition.Dot(XAxis);
            float dY = relativePosition.Dot(YAxis);
            float dZ = relativePosition.Dot(ZAxis);

            return new Vector3D(dX, dY, dZ);
        }



        /// <summary>
        /// Converts a coordinate in ABSOLUTE world coordinates into this scope's RELATIVE coordinates (0 - 1 range, relative to the scope sizes).
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Vector3D ToRelativeScopePosition(Vector3D position)
        {
            return (ToScopePosition(position) / Sizes).MakeValid();
        }



        /// <summary>
        /// Converts a coordinate in ABSOLUTE scope coordinates into world ABSOLUTE coordinates.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Vector3D ToWorldPosition(Vector3D position)
        {
            return XAxis * position.X + YAxis * position.Y + ZAxis * position.Z + Translation;
        }



        /// <summary>
        /// Converts a coordinate in RELATIVE scope coordinates into world ABSOLUTE coordinates.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Vector3D ToRelativeWorldPosition(Vector3D position)
        {
            return ToWorldPosition(position * Sizes);
        }



        /// <summary>
        /// Converts a direction from world's orientation into this scope's orientation.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public Vector3D ToScopeDirection(Vector3D direction)
        {
            float dX = direction.Dot(XAxis);
            float dY = direction.Dot(YAxis);
            float dZ = direction.Dot(ZAxis);

            return new Vector3D(dX, dY, dZ);
        }



        /// <summary>
        /// Converts a direction from this scope's orientation into world's orientation.
        /// </summary>
        /// <param name="scopeDirection"></param>
        /// <returns></returns>
        public Vector3D ToWorldDirection(Vector3D scopeDirection)
        {
            return XAxis * scopeDirection.X + YAxis * scopeDirection.Y + ZAxis * scopeDirection.Z;
        }



        /// <summary>
        /// Rotates the scope so that the first direction (in world space) will face the second direction (also in world space).
        /// </summary>
        /// <param name="firstDirection"></param>
        /// <param name="secondDirection"></param>
        /// <returns></returns>
        public BoxScope OrientTo(Vector3D firstDirection, Vector3D secondDirection, Vector3D pivot)
        {
            float angleTo = firstDirection.AngleTo(secondDirection);

            Vector3D rotationAxis = firstDirection.Cross(secondDirection).Normalize();
            if (!rotationAxis.IsNumericallyZero && !rotationAxis.IsInfinityOrNaN)
            {
                var rotation = Matrix.CreateTranslation(pivot) * Matrix.CreateAxisAngle(rotationAxis, angleTo) * Matrix.CreateTranslation(-pivot);
                var newScope = Transform(rotation);
                return newScope;
            }

            return this;
        }



        /// <summary>
        /// Gets the coordinates, in world space, of the 8 corners of the scope.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector3D> CornerPositions
        {
            get
            {
                Vector3D sizedXAxis = SizedXAxis;
                Vector3D sizedYAxis = SizedYAxis;
                Vector3D sizedZAxis = SizedZAxis;

                yield return Translation;
                yield return Translation + sizedXAxis;
                yield return Translation + sizedYAxis;
                yield return Translation + sizedZAxis;
                yield return Translation + sizedXAxis + sizedYAxis;
                yield return Translation + sizedXAxis + sizedZAxis;
                yield return Translation + sizedYAxis + sizedZAxis;
                yield return Translation + sizedXAxis + sizedYAxis + sizedZAxis;
            }
        }



        /// <summary>
        /// Gets a world-aligned bounding box that encloses this scope.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                BoundingBox boundingBox = new BoundingBox();

                foreach (Vector3D point in CornerPositions)
                    boundingBox.AddPoint(point);

                return boundingBox;
            }
        }



        public BoxScope Transform(Matrix transformation)
        {
            //transform the origin point
            var newTranslation = transformation.Transform(Translation);

            //determine the corner points of a cube with 1 unit sizes and transform them
            var pointX = transformation.Transform(Translation + XAxis);
            var pointY = transformation.Transform(Translation + YAxis);
            var pointZ = transformation.Transform(Translation + ZAxis);

            //determine the new direction vectors from the vector between the transformed points
            var newSizedXAxis = pointX - newTranslation;
            var newSizedYAxis = pointY - newTranslation;
            var newSizedZAxis = pointZ - newTranslation;

            //these sizes are in a relative, percentage scale
            var percentageSizeX = newSizedXAxis.Length;
            var percentageSizeY = newSizedYAxis.Length;
            var percentageSizeZ = newSizedZAxis.Length;

            var transposeInverse = transformation.Inverse.Transpose;

            newSizedXAxis = transposeInverse.Transform(XAxis);
            newSizedYAxis = transposeInverse.Transform(YAxis);
            newSizedZAxis = transposeInverse.Transform(ZAxis);


            return new BoxScope(newSizedXAxis.Normalize(), newSizedYAxis.Normalize(), newSizedZAxis.Normalize(), newTranslation, new Vector3D(Sizes.X * percentageSizeX, Sizes.Y * percentageSizeY, Sizes.Z * percentageSizeZ).MakeValid());
        }



        public Matrix ToWorldPositionMatrix()
        {
            return new Matrix(XAxis.X, YAxis.X, ZAxis.X, Translation.X,
                XAxis.Y, YAxis.Y, ZAxis.Y, Translation.Y,
                XAxis.Z, YAxis.Z, ZAxis.Z, Translation.Z,
                0, 0, 0, 1);
        }



        public Matrix ToScopePositionMatrix()
        {
            return ToWorldPositionMatrix().Transpose;
        }



        public Matrix ToWorldDirectionMatrix()
        {
            return new Matrix(XAxis.X, YAxis.X, ZAxis.X, 0,
                XAxis.Y, YAxis.Y, ZAxis.Y, 0,
                XAxis.Z, YAxis.Z, ZAxis.Z, 0,
                0, 0, 0, 1);
        }



        public Matrix ToScopeDirectionMatrix()
        {
            return ToWorldDirectionMatrix().Transpose;
        }



        public BoxScope Translate(Vector3D translation)
        {
            return new BoxScope(XAxis, YAxis, ZAxis, Translation + translation, Sizes);
        }



        public Vector3D[] ToRelativeMainPoints(BoxScope subScope)
        {
            Vector3D sizedXAxis = subScope.SizedXAxis;
            Vector3D sizedYAxis = subScope.SizedYAxis;
            Vector3D sizedZAxis = subScope.SizedZAxis;

            Vector3D[] mainPoints = new Vector3D[7];
            mainPoints[0] = ToRelativeScopePosition(subScope.Translation);
            mainPoints[1] = ToRelativeScopePosition(subScope.Translation + sizedXAxis);
            mainPoints[2] = ToRelativeScopePosition(subScope.Translation + sizedYAxis);
            mainPoints[3] = ToRelativeScopePosition(subScope.Translation + sizedZAxis);
            mainPoints[4] = ToScopeDirection(subScope.XAxis);
            mainPoints[5] = ToScopeDirection(subScope.YAxis);
            mainPoints[6] = ToScopeDirection(subScope.ZAxis);

            return mainPoints;
        }



        public BoxScope FromRelativeMainPoints(Vector3D[] mainFeatures)
        {
            var translation = ToRelativeWorldPosition(mainFeatures[0]);
            var cornerX = ToRelativeWorldPosition(mainFeatures[1]);
            var cornerY = ToRelativeWorldPosition(mainFeatures[2]);
            var cornerZ = ToRelativeWorldPosition(mainFeatures[3]);

            var directionX = cornerX - translation;
            var directionY = cornerY - translation;
            var directionZ = cornerZ - translation;

            var sizeX = directionX.Length;
            var sizeY = directionY.Length;
            var sizeZ = directionZ.Length;

            //fix the directions
            directionX = ToWorldDirection(mainFeatures[4]);
            directionY = ToWorldDirection(mainFeatures[5]);
            directionZ = ToWorldDirection(mainFeatures[6]);

            return new BoxScope(directionX, directionY, directionZ,
                translation, new Vector3D(sizeX, sizeY, sizeZ));
        }



        public BoxScope ToRelativeScope(BoxScope subScope)
        {
            var newXAxis = ToScopeDirection(subScope.XAxis);
            var newYAxis = ToScopeDirection(subScope.YAxis);
            var newZAxis = ToScopeDirection(subScope.ZAxis);

            var newTranslation = ToRelativeScopePosition(subScope.Translation);


            var newSizeX = ToWorldDirection(subScope.XAxis) * Sizes.X;
            var newSizeY = ToWorldDirection(subScope.YAxis) * Sizes.Y;

            //newSizeX.

            var newSizes = (subScope.Sizes / Sizes).MakeValid();

            return new BoxScope(newXAxis, newYAxis, newZAxis, newTranslation, newSizes);
        }



        public BoxScope FromRelativeScope(BoxScope subScope)
        {
            var newXAxis = ToWorldDirection(subScope.XAxis);
            var newYAxis = ToWorldDirection(subScope.YAxis);
            var newZAxis = ToWorldDirection(subScope.ZAxis);

            var newTranslation = ToRelativeWorldPosition(subScope.Translation);
            var newSizes = subScope.Sizes * Sizes;

            return new BoxScope(newXAxis, newYAxis, newZAxis, newTranslation, newSizes);
        }



        /*public BoxScope DeepClone()
        {
            return new BoxScope(XAxis, YAxis, ZAxis, Translation, Sizes);
        }*/



        public override string ToString()
        {
            //return String.Format("Sizes: {0}, Translation: {1}, XAxis: {2}, YAxis: {3}, ZAxis: {4}", _sizes, _translation, _xAxis, _yAxis, _zAxis);
            return string.Format("scope({0},{1},{2},{3},{4})", XAxis, YAxis, ZAxis, Sizes, Translation);
        }

        #endregion

        #region Properties

        public Vector3D XAxis
        {
            get;
        }


        public Vector3D SizedXAxis => XAxis * Sizes.X;


        public Vector3D YAxis
        {
            get;
        }


        public Vector3D SizedYAxis => YAxis * Sizes.Y;


        public Vector3D ZAxis
        {
            get;
        }


        public Vector3D SizedZAxis => ZAxis * Sizes.Z;


        public Vector3D Translation
        {
            get;
        }


        public Vector3D Sizes
        {
            get;
        }


        /// <summary>
        /// A scope located at the origin, coincident with the world axes, and with unit sizes.
        /// </summary>
        public static BoxScope Identity => new BoxScope(Vector3D.XVector, Vector3D.YVector, Vector3D.ZVector, Vector3D.Zero, Vector3D.One);


        public Plane3D FrontPlane => new Plane3D(YAxis, Translation + SizedYAxis);


        public Plane3D BackPlane => new Plane3D(-YAxis, Translation);


        public Plane3D LeftPlane => new Plane3D(-XAxis, Translation);


        public Plane3D RightPlane => new Plane3D(XAxis, Translation + SizedXAxis);


        public Plane3D TopPlane => new Plane3D(ZAxis, Translation + SizedZAxis);


        public Plane3D BottomPlane => new Plane3D(-ZAxis, Translation);


        public Vector3D Centroid => Translation + SizedXAxis / 2f + SizedYAxis / 2f + SizedZAxis / 2f;



        public BoundingRectangle BoundingRectangle
        {
            get
            {
                BoundingRectangle boundingRectangle = new BoundingRectangle();

                foreach (Vector3D point in CornerPositions)
                    boundingRectangle.AddPoint(point.ToVector2D());

                return boundingRectangle;
            }
        }



        public bool IsSkewed
        {
            get
            {
                var dot1 = XAxis.Dot(YAxis);
                var dot2 = XAxis.Dot(ZAxis);
                var dot3 = YAxis.Dot(ZAxis);

                return dot1 > 0.01f || dot2 > 0.01f || dot3 > 0.01f;
            }
        }

        #endregion
    }
}