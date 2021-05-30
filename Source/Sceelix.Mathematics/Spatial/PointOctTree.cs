using System;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Spatial
{
    public class PointOctTree : GenericOctTree<Vector3D>
    {
        public PointOctTree(int initialPartitionSize, int sectionItemMaxCount) : base(initialPartitionSize, sectionItemMaxCount)
        {
        }



        public bool Contains(BoundingBox box, Vector3D point)
        {
            throw new NotImplementedException();
        }



        public bool Contains(BoundingSphere box, Vector3D point)
        {
            throw new NotImplementedException();
        }



        /*protected override Vector3D GetItemPoint(Vector3D item)
		{
			return item;
		}

		protected override BoundingBox GetItemBoundary(Vector3D item, float radius)
		{
			var min = item - new Vector3D(radius, radius, radius);
			var max = item + new Vector3D(radius, radius, radius);
			return new BoundingBox(min, max);
		}

        protected override bool SectionContains(GenericPartitionTreeSection<BoundingBox, Vector3D> section, Vector3D item)
		{
			return section.Boundary.Contains(item);
		}*/



        protected override BoundingBox GetItemBoundary(Vector3D item)
        {
            return new BoundingBox(item, item);
        }
    }
}