using System.Collections.Generic;

namespace Sceelix.Extensions
{
    public static class QueueExtension
    {
        public static T DequeueOrDefault<T>(this Queue<T> queue)
        {
            return queue.Count > 0 ? queue.Dequeue() : default(T);
        }



        public static bool IsEmpty<T>(this Queue<T> queue)
        {
            return queue.Count == 0;
        }
    }
}