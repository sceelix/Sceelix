using System;
using System.Linq;

namespace Sceelix.Mathematics.Spatial
{
    public class ObjectOctTree<T> : GenericOctTree<T>
    {
        private readonly Func<T, BoundingBox> _boundaryFunction;



        public ObjectOctTree(int initialPartitionSize, int sectionItemMaxCount, Func<T, BoundingBox> boundaryFunction)
            : base(initialPartitionSize, sectionItemMaxCount)
        {
            _boundaryFunction = boundaryFunction;
        }



        public ObjectOctTree(BoundingBox boundingBox, int sectionItemMaxCount, Func<T, BoundingBox> boundaryFunction)
            : base((int) Math.Ceiling(boundingBox.Size.ToArray().Max()), sectionItemMaxCount)
        {
            _boundaryFunction = boundaryFunction;
        }



        protected override BoundingBox GetItemBoundary(T item)
        {
            return _boundaryFunction(item);
        }
    }
}