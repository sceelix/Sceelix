using Sceelix.Mathematics.Geometry;

namespace Sceelix.Mathematics.Spatial
{
    public class SegmentQuadTree : GenericQuadTree<LineSegment2D>
    {
        public SegmentQuadTree(int initialPartitionSize, int sectionItemMaxCount)
            : base(initialPartitionSize, sectionItemMaxCount)
        {
        }



        protected override BoundingRectangle GetItemBoundary(LineSegment2D item)
        {
            return new BoundingRectangle(new[] {item.Start, item.End});
        }



        /*
        protected override Vector2D GetItemPoint(LineSegment2D item)
		{
			return item;
		}

	    
        protected override BoundingRectangle GetItemBoundary(LineSegment2D item, float radius)
		{
			return new BoundingRectangle(new [] { item.Start, item.End });
		}

		protected override bool SectionContains(GenericPartitionTreeSection<BoundingRectangle, LineSegment2D> section, LineSegment2D item)
		{
		    

            return section.Boundary.Intersects(new BoundingRectangle()).Contains(item.Start) || section.Boundary.Contains(item.End);
		}*/
    }
}