using System;
using System.Collections.Generic;
using System.Linq;

namespace Sceelix.Mathematics.Spatial
{
    public abstract class GenericPartitionTree<TItem, TPoint, TBoundary>
    {
        public const int MaxIterationsPerExpansion = 16;



        protected GenericPartitionTree(int initialPartitionSize, int sectionItemMaxCount)
        {
            SectionItemMaxCount = sectionItemMaxCount;
            InitialPartitionSize = initialPartitionSize;
        }



        /*protected GenericPartitionTree(TBoundary boundary, int sectionItemMaxCount)
        {
            _sectionItemMaxCount = sectionItemMaxCount;
            _initialPartitionSize = initialPartitionSize;
        }*/


        public int InitialPartitionSize
        {
            get;
        }


        public int ItemCount => Root != null ? Root.TotalItemCount : 0;


        public IEnumerable<TItem> Items => Root != null ? Root.AllItems ?? new List<TItem>() : new List<TItem>();


        public GenericPartitionTreeSection<TBoundary, TItem> Root
        {
            get;
            private set;
        }



        public IEnumerable<TBoundary> SectionBoundaries
        {
            get
            {
                if (Root != null)
                    foreach (var genericPartitionTreeSection in Root.AllSubsections)
                        yield return genericPartitionTreeSection.Boundary;
            }
        }



        public int SectionCount => Root != null ? Root.TotalSubsectionCount : 0;


        public int SectionItemMaxCount
        {
            get;
        }


        protected abstract int SubsectionsPerSection
        {
            get;
        }



        public void AddItem(TItem item)
        {
            var boundary = GetItemBoundary(item);

            InitializeIfNecessary(boundary);

            ExpandWhilstNecessary(boundary);

            //find a section where we can put this item
            var section = FindSection(boundary);

            if (SectionNeedsSubdividing(section))
            {
                Subdivide(section);
                section = FindSection(boundary);
            }

            //if (section == null)
            //    FindSection(boundary);

            section.AddItem(item, boundary);
        }



        public void AddItems(IEnumerable<TItem> items)
        {
            foreach (var item in items)
                AddItem(item);
        }



        protected void AddSubsection(GenericPartitionTreeSection<TBoundary, TItem> root, GenericPartitionTreeSection<TBoundary, TItem> section)
        {
            if (root.SubsectionCount >= SubsectionsPerSection)
                throw new IndexOutOfRangeException();

            section.Parent = root;
            root.AddSubsection(section);
        }



        protected abstract bool BoundaryContains(TBoundary sectionBoundary, TBoundary boundary);

        protected abstract bool BoundaryIntersects(TBoundary boundary, TBoundary target);



        private bool CheckBoundaryShape(TBoundary sectionBoundary, TBoundary shape)
        {
            return BoundaryContains(sectionBoundary, shape) || BoundaryIntersects(sectionBoundary, shape);
        }



        protected abstract GenericPartitionTreeSection<TBoundary, TItem> Expand(TPoint director);



        private void ExpandWhilstNecessary(TBoundary boundary)
        {
            if (!BoundaryContains(Root.Boundary, boundary))
            {
                var director = GetExpandDirector(boundary);
                var iterations = 0;
                while (!BoundaryContains(Root.Boundary, boundary))
                {
                    Root = Expand(director);
                    iterations++;

                    if (iterations > MaxIterationsPerExpansion)
                        throw new IndexOutOfRangeException();
                }
            }
        }



        private GenericPartitionTreeSection<TBoundary, TItem> FindSection(TBoundary boundary)
        {
            return FindSubsection(Root, boundary);
        }



        private GenericPartitionTreeSection<TBoundary, TItem> FindSubsection(GenericPartitionTreeSection<TBoundary, TItem> section, TBoundary boundary)
        {
            //this should be contains
            //but when looking with a query, the shape must contain or intersect
            if (BoundaryContains(section.Boundary, boundary)) //previously was CheckBoundaryShape
            {
                if (section.SubsectionCount == 0)
                    return section;

                foreach (var subsection in section.Subsections)
                {
                    var subSectionFound = FindSubsection(subsection, boundary);
                    if (subSectionFound != null)
                        return subSectionFound;
                }

                //return itself, because it is the smallest section that can hold the item
                return section;
            }

            return null;
        }



        //protected abstract bool SectionContains(GenericPartitionTreeSection<TBoundary, TItem> section, TBoundary boundary);

        protected abstract TPoint GetExpandDirector(TBoundary boundary);


        //protected abstract TPoint GetItemPoint(TItem item);

        protected abstract TBoundary GetItemBoundary(TItem item);



        private GenericPartitionTreeSection<TBoundary, TItem> GetItemSection(TItem item, GenericPartitionTreeSection<TBoundary, TItem> root)
        {
            return root.Items.Contains(item) ? root : root.Subsections.Select(subsection => GetItemSection(item, subsection)).FirstOrDefault(x => x != null);
        }



        private GenericPartitionTreeSection<TBoundary, TItem> GetItemSection(TItem item)
        {
            return GetItemSection(item, Root);
        }



        public IEnumerable<TItem> GetItemsWithinBoundary(TBoundary boundary)
        {
            return GetItemsWithinShape(boundary, CheckBoundaryShape);
            /*if (boundary != null && Root != null)
                return GetItemsWithinBoundary(boundary, Root);

            return new TItem[0];*/
        }



        public abstract IEnumerable<TItem> GetItemsWithinRadius(TPoint point, float radius);


        /*private IEnumerable<TItem> GetItemsWithinBoundary(TBoundary boundary, GenericPartitionTreeSection<TBoundary, TItem> section)
        {
            var list = new List<TItem>();
            if (!BoundaryContains(section.Boundary, boundary) && !BoundaryIntersects(section.Boundary, boundary))
                return list;

            if (section.SubsectionCount == 0)
                return section.Items;

            foreach (var subsection in section.Subsections)
                list.AddRange(GetItemsWithinBoundary(boundary, subsection));

            return list;
        }*/



        protected IEnumerable<TItem> GetItemsWithinShape<TShape>(TShape shape, Func<TBoundary, TShape, bool> boundaryCheck)
        {
            if (Root != null)
                return GetItemsWithinShape(shape, boundaryCheck, Root);

            return Enumerable.Empty<TItem>();
        }



        private IEnumerable<TItem> GetItemsWithinShape<TShape>(TShape shape, Func<TBoundary, TShape, bool> boundaryCheck, GenericPartitionTreeSection<TBoundary, TItem> section)
        {
            var list = new List<TItem>();
            if (boundaryCheck(section.Boundary, shape))
            {
                //add the direct items are contained in the given shape
                list.AddRange(section.ItemsBoundaries.Where(x => boundaryCheck(x.Value, shape)).Select(x => x.Key));

                foreach (var subsection in section.Subsections)
                    list.AddRange(GetItemsWithinShape(shape, boundaryCheck, subsection));
            }

            return list;
        }



        //protected abstract TBoundary GetItemBoundary(TItem item, float radius);

        protected abstract TBoundary GetSectionBoundary(TBoundary boundary, int size);



        public bool HasItem(TItem item)
        {
            return HasItem(item, Root);
        }



        private bool HasItem(TItem item, GenericPartitionTreeSection<TBoundary, TItem> root)
        {
            if (root == null)
                return false;

            //calculate its boundary and search for the section where it would fit
            var boundary = GetItemBoundary(item);
            var section = FindSection(boundary);

            if (section == null)
                return false;

            //root.Items.Contains(item) || root.Subsections.Any(subsection => HasItem(item, subsection));
            return section.Items.Contains(item);
        }



        /// <summary>
        /// Initializes if necessary.
        /// </summary>
        /// <param name="boundary">The boundary.</param>
        private void InitializeIfNecessary(TBoundary boundary)
        {
            if (Root != null)
                return;

            Root = new GenericPartitionTreeSection<TBoundary, TItem>(GetSectionBoundary(boundary, InitialPartitionSize));
        }



        protected void RedistributeItemsWithinSubsections(GenericPartitionTreeSection<TBoundary, TItem> section)
        {
            List<TItem> itemsToRemove = new List<TItem>();

            foreach (var itemBoundary in section.ItemsBoundaries)
            {
                var subsection = FindSubsection(section, itemBoundary.Value);

                //because the section returned can be itself, perform the check
                if (subsection != null && subsection != section)
                {
                    subsection.AddItem(itemBoundary.Key, itemBoundary.Value);
                    itemsToRemove.Add(itemBoundary.Key);
                }
            }

            //keep the items that only fit in the whole section
            section.RemoveItems(itemsToRemove);
        }



        private void RemoveAllItems(GenericPartitionTreeSection<TBoundary, TItem> root)
        {
            root.RemoveItems();

            foreach (var subsection in root.Subsections)
                RemoveAllItems(subsection);
        }



        public void RemoveAllItems()
        {
            RemoveAllItems(Root);
            Root = null;
        }



        public void RemoveItem(TItem item)
        {
            var itemSection = GetItemSection(item);
            if (itemSection != null)
                itemSection.RemoveItem(item);
        }



        public void RemoveItems(IEnumerable<TItem> items)
        {
            foreach (var item in items)
                RemoveItem(item);
        }



        private bool SectionNeedsSubdividing(GenericPartitionTreeSection<TBoundary, TItem> section)
        {
            return !section.IsSubdivided && section.ItemCount >= SectionItemMaxCount;
        }



        protected abstract void Subdivide(GenericPartitionTreeSection<TBoundary, TItem> section);
    }
}