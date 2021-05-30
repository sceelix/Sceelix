using System.Linq;
using NUnit.Framework;
using Sceelix.Paths.Data;
using Sceelix.Paths.Procedures;

namespace Sceelix.Paths.Tests.Procedures
{
    public class PathCreateTests
    {
        [Test]
        public void CreateGrid()
        {
            var pathCreateProcedure = new PathCreateProcedure();
            pathCreateProcedure.Parameters["Type"].Set("Grid");
            {
                var gridParameter = pathCreateProcedure.Parameters["Type"].Parameters["Grid"];
                gridParameter.Parameters["Number of Columns"].Set(20);
                gridParameter.Parameters["Number of Rows"].Set(10);
                gridParameter.Parameters["Column Width"].Set(10);
                gridParameter.Parameters["Row Height"].Set(5);
            }
            pathCreateProcedure.Execute();

            var pathEntity = pathCreateProcedure.Outputs["Output"].Dequeue<PathEntity>();

            Assert.AreEqual((20 + 1) * (10 + 1), pathEntity.Vertices.Count());
            Assert.AreEqual(200f, pathEntity.BoundingBox.Width);
            Assert.AreEqual(50f, pathEntity.BoundingBox.Length);
        }

        

        [Test]
        public void CreateVoronoi()
        {
            var pathCreateProcedure = new Sceelix.Paths.Procedures.PathCreateProcedure();
            pathCreateProcedure.Parameters["Type"].Set("Voronoi");
            {
                var voronoiParameter = pathCreateProcedure.Parameters["Type"].Parameters["Voronoi"];
                voronoiParameter.Parameters["Width"].Set(50);
                voronoiParameter.Parameters["Height"].Set(75);
                voronoiParameter.Parameters["Spacing"].Set(10f);
                voronoiParameter.Parameters["Max Offset"].Set(5f);
                voronoiParameter.Parameters["Seed"].Set(0);
            }
            pathCreateProcedure.Execute();

            var pathEntity = pathCreateProcedure.Outputs["Output"].Dequeue<PathEntity>();
            Assert.AreEqual(50, pathEntity.BoxScope.Sizes.X);
            Assert.AreEqual(75, pathEntity.BoxScope.Sizes.Y);
        }
    }
}