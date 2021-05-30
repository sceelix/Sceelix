using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;

namespace Sceelix.Core.Attributes
{
    public enum MergeMethod
    {
        First,
        Mid,
        Last,
        Sum,
        Average,
        List
    }

    public class MergeMeta
    {
        private readonly MergeMethod _mergeMethod = MergeMethod.First;



        public object Merge(List<object> objects)
        {
            if (_mergeMethod == MergeMethod.First)
                return objects.First();
            if (_mergeMethod == MergeMethod.Mid)
                return objects[objects.Count / 2];
            if (_mergeMethod == MergeMethod.Last)
                return objects.Last();
            if (_mergeMethod == MergeMethod.Sum)
                return objects.Aggregate((val, result) => (dynamic) result + (dynamic) val);
            if (_mergeMethod == MergeMethod.Average)
                return objects.Aggregate((val, result) => (dynamic) result + (dynamic) val) / (dynamic) objects.Count;
            if (_mergeMethod == MergeMethod.List)
                return new SceeList(objects);

            return null;
        }
    }
}