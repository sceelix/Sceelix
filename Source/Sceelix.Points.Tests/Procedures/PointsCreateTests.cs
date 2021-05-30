using System.Linq;
using NUnit.Framework;
using Sceelix.Mathematics.Data;
using Sceelix.Points.Procedures;

namespace Sceelix.Points.Tests.Procedures
{
    public class PointsCreateTests
    {
        [Test]
        public void CreateGrid()
        {
            var pointsCreateProcedure = new PointsCreateProcedure();
            pointsCreateProcedure.Parameters["Type"].Set("Grid");
            {
                var gridParameter = pointsCreateProcedure.Parameters["Type"].Parameters["Grid"];
                gridParameter.Parameters["Columns"].Set(10);
                gridParameter.Parameters["Rows"].Set(10);
                gridParameter.Parameters["Layers"].Set(10);
                gridParameter.Parameters["Spacing"].Set(new Vector3D(10, 10, 10));
            }
            pointsCreateProcedure.Execute();
            var outputData = pointsCreateProcedure.Outputs["Output"].DequeueAll();

            Assert.AreEqual(outputData.Count(), 1000);
        }
    }
}