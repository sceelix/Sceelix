using System.Collections.Generic;
using System.Linq;

namespace Sceelix.Mathematics.Spatial
{
    public class GenericPartitionTreeSection<TBoundary, TItem>
    {
        private readonly Dictionary<TItem, TBoundary> _items;
        private readonly List<GenericPartitionTreeSection<TBoundary, TItem>> _subsections;



        internal GenericPartitionTreeSection(TBoundary boundary)
        {
            Boundary = boundary;
            _items = new Dictionary<TItem, TBoundary>();
            _subsections = new List<GenericPartitionTreeSection<TBoundary, TItem>>();
        }



        internal IEnumerable<TItem> AllItems
        {
            get
            {
                if (ItemCount > 0)
                    return Items;

                var items = new List<TItem>();
                foreach (var subsection in _subsections)
                    items.AddRange(subsection.AllItems);
                return items;
            }
        }



        internal IEnumerable<GenericPartitionTreeSection<TBoundary, TItem>> AllSubsections
        {
            get { return _subsections.Union(_subsections.SelectMany(x => x.AllSubsections)); }
        }



        internal TBoundary Boundary
        {
            get;
        }


        public bool IsSubdivided => SubsectionCount > 0;


        internal int ItemCount => _items.Count;


        internal IEnumerable<TItem> Items => _items.Keys;


        internal IEnumerable<KeyValuePair<TItem, TBoundary>> ItemsBoundaries => _items;


        internal GenericPartitionTreeSection<TBoundary, TItem> Parent
        {
            get;
            set;
        }


        public int SubsectionCount => _subsections.Count;


        internal IEnumerable<GenericPartitionTreeSection<TBoundary, TItem>> Subsections => _subsections;



        internal int TotalItemCount
        {
            get { return _subsections.Count == 0 ? _items.Count : _subsections.Sum(subsection => subsection.TotalItemCount); }
        }



        internal int TotalSubsectionCount
        {
            get { return 1 + _subsections.Sum(subsection => subsection.TotalSubsectionCount); }
        }



        internal void AddItem(TItem item, TBoundary boundary)
        {
            _items.Add(item, boundary);
        }



        internal void AddSubsection(GenericPartitionTreeSection<TBoundary, TItem> section)
        {
            _subsections.Add(section);
        }



        internal void RemoveItem(TItem item)
        {
            _items.Remove(item);

            if (Parent != null && Parent.TotalItemCount == 0)
                Parent.RemoveSubsections();
        }



        internal void RemoveItems()
        {
            _items.Clear();

            if (Parent != null && Parent.TotalItemCount == 0)
                Parent.RemoveSubsections();
        }



        internal void RemoveItems(List<TItem> keysToRemove)
        {
            foreach (var key in keysToRemove)
                _items.Remove(key);
        }



        internal void RemoveSubsections()
        {
            _subsections.Clear();
        }
    }
}