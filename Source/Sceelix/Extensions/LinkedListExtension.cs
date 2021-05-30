using System.Collections.Generic;

namespace Sceelix.Extensions
{
    public static class LinkedListExtension
    {
        public static T Dequeue<T>(this LinkedList<T> linkedList)
        {
            T linkedListNode = linkedList.First.Value;
            linkedList.RemoveFirst();

            return linkedListNode;
        }



        public static bool IsEmpty<T>(this LinkedList<T> linkedList)
        {
            return linkedList.Count == 0;
        }
    }
}