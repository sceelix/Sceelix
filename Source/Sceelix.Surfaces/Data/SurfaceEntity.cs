using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;
using Sceelix.Mathematics.Spatial;
using Sceelix.Surfaces.Materials;

namespace Sceelix.Surfaces.Data
{
    /// <summary>
    /// The surface is an entity that defines a uniform, grid-based set of vertices, all of which have a certain height and can have any kind of properties.
    /// A surface has its Y coordinate 
    /// </summary>
    [Entity("Surface")]
    public class SurfaceEntity : Entity, IActor
    {
        /// <summary>
        /// Distance between the points of the grid.
        /// </summary>
        private float _cellSize;

        private List<SurfaceLayer> _layers = new List<SurfaceLayer>();

        /// <summary>
        /// Material that defines show to render this 
        /// </summary>
        private Material _material = new DefaultSurfaceMaterial();

        private Vector2D _origin;



        public SurfaceEntity(int numColumns, int numRows, float cellSize = 1)
        {
            NumColumns = numColumns;
            NumRows = numRows;
            _cellSize = cellSize;
        }



        /// <summary>
        /// Gets the axis-oriented boundingbox that fully encloses the surface.
        /// </summary>
        /// <value></value>
        public BoundingBox BoundingBox
        {
            get
            {
                var minimum = new Vector3D(_origin.X, _origin.Y, MinimumZ);

                return new BoundingBox(minimum, minimum + new Vector3D(Width, Length, Height));
            }
        }



        /// <summary>
        /// 2D Rectangle that encompasses the surface.
        /// </summary>
        public BoundingRectangle BoundingRectangle => new BoundingRectangle(_origin.X, _origin.Y, Width, Length);



        [EntityProperty("Scope", HandleType = typeof(SceeList))]
        public BoxScope BoxScope
        {
            get { return new BoxScope(sizes: BoundingBox.Size, translation: _origin.ToVector3D(MinimumZ)); }
            set
            {
                // Does nothing here
            }
        }



        /// <summary>
        /// Distance between vertices of the surface grid.
        /// </summary>
        [EntityProperty]
        public float CellSize => _cellSize;


        /// <summary>
        /// Total height of the surface (= the maximum relative position of the surface).
        /// </summary>
        [EntityProperty]
        public float Height => MaximumZ - MinimumZ;


        [SubEntity("Layers")]
        public IEnumerable<SurfaceLayer> Layers => _layers;


        /// <summary>
        /// Size of the surface in the Y direction
        /// </summary>
        [EntityProperty]
        public float Length => (NumRows - 1) * _cellSize;



        /// <summary>
        /// Material applied to the surface.
        /// </summary>
        public Material Material
        {
            get { return _material; }
            set { _material = value; }
        }



        [EntityProperty]
        public float MaximumZ
        {
            get { return _layers.OfType<I3DLayer>().Select(x => x.MaxHeight).Where(x => !float.IsNaN(x)).DefaultIfEmpty(0).Max(); }
        }



        [EntityProperty]
        public float MinimumZ
        {
            get { return _layers.OfType<I3DLayer>().Select(x => x.MinHeight).Where(x => !float.IsNaN(x)).DefaultIfEmpty(0).Min(); }
        }



        /// <summary>
        /// Vertex count in X.
        /// </summary>
        [EntityProperty]
        public int NumColumns
        {
            get;
        }


        /// <summary>
        /// Vertex count in Y.
        /// </summary>
        [EntityProperty]
        public int NumRows
        {
            get;
        }



        /// <summary>
        /// Bottom-left position of the surface, on the XY-Plane.
        /// </summary>
        [EntityProperty(HandleType = typeof(SceeList))]
        public Vector2D Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }



        public override IEnumerable<IEntity> SubEntityTree
        {
            get
            {
                foreach (SurfaceLayer surfaceLayer in Layers) yield return surfaceLayer;
            }
        }



        /// <summary>
        /// Size of the surface in the X direction
        /// </summary>
        [EntityProperty]
        public float Width => (NumColumns - 1) * _cellSize;



        /// <summary>
        /// Adds the layer to the surface and initializes it.
        /// </summary>
        /// <param name="surfaceLayer">The surface layer.</param>
        /// <returns>The same layer that was added, after initialization.</returns>
        public T AddLayer<T>(T surfaceLayer) where T : SurfaceLayer
        {
            _layers.Add(surfaceLayer);

            surfaceLayer.Surface = this;

            return surfaceLayer;
        }



        /// <summary>
        /// Determines whether this surface contains the given coordinates.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <returns>
        ///   <c>true</c> if this surface contains the specified (column,row) coordinates; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Coordinate coordinate)
        {
            return coordinate.X >= 0 && coordinate.Y >= 0 && coordinate.X < NumColumns && coordinate.Y < NumRows;
        }



        /// <summary>
        /// Determines whether this surface contains the given world position.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <returns>
        ///   <c>true</c> if this surface contains the specified world position; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Vector2D worldPosition)
        {
            var relativePosition = worldPosition - _origin;

            return relativePosition.X >= 0 && relativePosition.X <= Width
                                           && relativePosition.Y >= 0 && relativePosition.Y <= Length;
        }



        /// <summary>
        /// Creates a deep clone of the surface.
        /// </summary>
        /// <returns></returns>
        public override IEntity DeepClone()
        {
            SurfaceEntity clone = (SurfaceEntity) base.DeepClone();

            clone._layers = new List<SurfaceLayer>();
            foreach (var surfaceLayer in _layers)
            {
                var layerClone = (SurfaceLayer) surfaceLayer.DeepClone();
                layerClone.Surface = clone;

                clone.AddLayer(layerClone);
            }

            clone._cellSize = _cellSize;
            clone._origin = _origin;
            clone._material = (Material) _material.DeepClone();

            return clone;
        }



        /// <summary>
        /// Gets the first layer of the give type T that it can find.
        /// </summary>
        /// <typeparam name="T">Type of the  layer to look for.</typeparam>
        /// <returns> The first layer of the given type T that is found, or null if no layer of the given type exists.</returns>
        public T GetLayer<T>() where T : ISurfaceLayer
        {
            return _layers.OfType<T>().FirstOrDefault();
        }



        /// <summary>
        /// Gets all the layers of the indicated type T.
        /// </summary>
        /// <typeparam name="T">Type of the  layer to look for.</typeparam>
        /// <returns>The layers of type T in this surface.</returns>
        public IEnumerable<T> GetLayers<T>() where T : ISurfaceLayer
        {
            return _layers.OfType<T>();
        }



        public void InsertInto(BoxScope target)
        {
            //the translation is the first to be changed
            _origin = target.Translation.ToVector2D(); // - new Vector3D(0,0, MinimumZ);

            //scale the cell size so that it fits the new scope
            var newCellSizeX = target.Sizes.X / (NumColumns - 1);
            var newCellSizeY = target.Sizes.Y / (NumRows - 1);

            var newCellSize = Math.Min(newCellSizeX, newCellSizeY);
            _cellSize = newCellSize;

            //foreach (SurfaceLayer surfaceLayer in _layers)
            //    surfaceLayer.UpdateCellSize();

            foreach (I3DLayer surfaceLayer in _layers.OfType<I3DLayer>())
                surfaceLayer.TranslateVertically(target.Translation.Z - BoxScope.Translation.Z);

            //scale the heights
            var zSize = Height;
            if (zSize > 0)
            {
                var zScale = target.Sizes.Z / zSize;
                if (Math.Abs(zScale - 1) > float.Epsilon)
                    foreach (I3DLayer surfaceLayer in _layers.OfType<I3DLayer>())
                        surfaceLayer.ScaleVertically(zScale);
            }
        }



        public void Merge(SurfaceEntity otherSurface)
        {
            if (NumRows == otherSurface.NumRows && NumColumns == otherSurface.NumColumns)
                foreach (SurfaceLayer surfaceLayer in otherSurface.Layers)
                    AddLayer((SurfaceLayer) surfaceLayer.DeepClone());
            else
                foreach (SurfaceLayer surfaceLayer in otherSurface.Layers)
                    AddLayer(surfaceLayer.ChangeResolution(NumColumns, NumRows));
        }



        /// <summary>
        /// Removes the layer from the surface.
        /// </summary>
        /// <param name="layer">The layer.</param>
        public void RemoveLayer(SurfaceLayer layer)
        {
            layer.Surface = null;

            _layers.Remove(layer);
        }



        public void ReplaceLayer(SurfaceLayer originalLayer, SurfaceLayer newLayer)
        {
            var index = _layers.IndexOf(originalLayer);
            if (index < 0)
                throw new InvalidOperationException("The indicated 'original layer' does not exist on the surface.");

            ReplaceLayer(index, newLayer);
        }



        public void ReplaceLayer(int index, SurfaceLayer newLayer)
        {
            _layers[index].Surface = null;
            _layers[index] = newLayer;
            _layers[index].Surface = this;
        }



        /// <summary>
        /// Converts the given 2D world coordinates to set of surface coordinates.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <param name="clamp">indicates if the coordinates should be clamped to the surface bounds.</param>
        /// <returns></returns>
        public Coordinate ToCoordinates(Vector2D worldPosition, bool clamp = true, RoundingMethod roundingMethod = RoundingMethod.Floor)
        {
            var relativePosition = worldPosition - _origin;
            int coordinateX, coordinateY;

            switch (roundingMethod)
            {
                case RoundingMethod.Nearest:
                    coordinateX = (int) Math.Round(relativePosition.X / (double) _cellSize, MidpointRounding.AwayFromZero);
                    coordinateY = NumRows - 1 - (int) Math.Round(relativePosition.Y / (double) _cellSize, MidpointRounding.AwayFromZero);
                    break;

                case RoundingMethod.Ceiling:
                    coordinateX = (int) Math.Ceiling(relativePosition.X / (double) _cellSize);
                    coordinateY = NumRows - 1 - (int) Math.Ceiling(relativePosition.Y / (double) _cellSize);
                    break;
                default:
                    coordinateX = (int) Math.Floor(relativePosition.X / (double) _cellSize);
                    coordinateY = NumRows - 1 - (int) Math.Floor(relativePosition.Y / (double) _cellSize);
                    break;
            }

            if (clamp)
                return new Coordinate(MathHelper.Clamp(coordinateX, 0, NumColumns - 1), MathHelper.Clamp(coordinateY, 0, NumRows - 1));

            return new Coordinate(coordinateX, coordinateY);
        }



        /// <summary>
        /// Converts the given surface coordinates to a 2D world position.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public Vector2D ToWorldPosition(Coordinate coordinate)
        {
            return new Vector2D(_origin.X + coordinate.X * _cellSize, _origin.Y + Length - coordinate.Y * _cellSize);
        }
    }
}