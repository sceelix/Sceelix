using System;
using System.Linq;

namespace Sceelix.Mathematics.Spatial
{
    public class ObjectQuadTree<T> : GenericQuadTree<T>
    {
        private readonly Func<T, BoundingRectangle> _boundaryFunction;



        public ObjectQuadTree(int initialPartitionSize, int sectionItemMaxCount, Func<T, BoundingRectangle> boundaryFunction)
            : base(initialPartitionSize, sectionItemMaxCount)
        {
            _boundaryFunction = boundaryFunction;
        }



        public ObjectQuadTree(BoundingRectangle boundingRectangle, int sectionItemMaxCount, Func<T, BoundingRectangle> boundaryFunction)
            : base((int) boundingRectangle.Size.ToArray().Max(), sectionItemMaxCount)
        {
            _boundaryFunction = boundaryFunction;
        }



        protected override BoundingRectangle GetItemBoundary(T item)
        {
            return _boundaryFunction(item);
        }
    }
}