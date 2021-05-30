using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Spatial
{
    public class PointQuadTree : GenericQuadTree<Vector2D>
    {
        public PointQuadTree(int initialPartitionSize, int sectionItemMaxCount)
            : base(initialPartitionSize, sectionItemMaxCount)
        {
        }



        /*protected override Vector2D GetItemPoint(Vector2D item)
		{
			return item;
		}

        protected override BoundingRectangle GetItemBoundary(Vector2D item, float radius)
		{
			var min = item - new Vector2D(radius, radius);
			var max = item + new Vector2D(radius, radius);
			return new BoundingRectangle(min, max);
		}

        protected override bool SectionContains(GenericPartitionTreeSection<BoundingRectangle, Vector2D> section, Vector2D item)
		{
			return section.Boundary.Contains(item);
		}*/



        protected override BoundingRectangle GetItemBoundary(Vector2D item)
        {
            return new BoundingRectangle(item, item);
        }
    }
}