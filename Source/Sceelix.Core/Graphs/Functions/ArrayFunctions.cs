namespace Sceelix.Core.Graphs.Functions
{
    public class ArrayFunctions
    {
        public static object Avg(dynamic list)
        {
            return list.Average();
        }



        public static object Last(dynamic list)
        {
            return list.Last();
        }



        public static object Max(dynamic list)
        {
            return list.Max();
        }



        public static object Min(dynamic list)
        {
            return list.Min();
        }



        public static object Size(dynamic list)
        {
            return list.Count();
        }



        public static object Sum(dynamic list)
        {
            return list.Sum();
        }
    }
}