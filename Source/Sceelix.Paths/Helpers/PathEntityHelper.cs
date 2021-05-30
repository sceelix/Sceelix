using Sceelix.Paths.Data;
using Sceelix.Paths.Procedures;

namespace Sceelix.Paths.Helpers
{
    public static class PathEntityHelper
    {
        //private static readonly PathCreateProcedure PathCreateProcedure = new PathCreateProcedure();
        private static readonly PathModifyProcedure PathModifyProcedure = new PathModifyProcedure();



        /// <summary>
        /// Blends edges and vertices within paths. Intersecting edges result in shared vertices. Overlapping vertices are joined.
        /// </summary>
        /// <returns></returns>
        public static PathEntity Blend(this PathEntity pathEntity)
        {
            lock (PathModifyProcedure)
            {
                PathModifyProcedure.Inputs["Input"].Enqueue(pathEntity);
                PathModifyProcedure.Parameters["Operation"].Set("Blend");
                PathModifyProcedure.Execute();

                return PathModifyProcedure.Outputs["Output"].Dequeue<PathEntity>();
            }
        }



        /// <summary>
        /// Rounds the coordinate values of the path vertices, reducing possible mathematical errors in floating point values.
        /// </summary>
        /// <returns></returns>
        public static PathEntity Round(this PathEntity pathEntity, int precision = 2)
        {
            lock (PathModifyProcedure)
            {
                PathModifyProcedure.Inputs["Input"].Enqueue(pathEntity);
                PathModifyProcedure.Parameters["Operation"].Set("Round");
                PathModifyProcedure.Parameters["Operation"].Parameters["Round"].Parameters["Precision"].Set(precision);
                PathModifyProcedure.Execute();

                return PathModifyProcedure.Outputs["Output"].Dequeue<PathEntity>();
            }
        }
    }
}