using System.Linq;
using NUnit.Framework;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;
using Sceelix.Surfaces.Procedures;

namespace Sceelix.Surfaces.Tests.Procedures
{
    public class SurfaceCreateTests
    {
        [Test]
        public void CreateEmptySurface()
        {
            var surfaceCreateProcedure = new SurfaceCreateProcedure();
            surfaceCreateProcedure.Parameters["Type"].Set("Empty");
            {
                var emptyParameter = surfaceCreateProcedure.Parameters["Type"].Parameters["Empty"];
                emptyParameter.Parameters["Width"].Set(100);
                emptyParameter.Parameters["Length"].Set(150);
                emptyParameter.Parameters["Cell Size"].Set(2f);
                emptyParameter.Parameters["Interpolation"].Set("TopLeft");
            }
            surfaceCreateProcedure.Execute();

            var createdSurface = surfaceCreateProcedure.Outputs["Output"].Dequeue<SurfaceEntity>();

            Assert.AreEqual(100, createdSurface.Width);
            Assert.AreEqual(150, createdSurface.Length);
            Assert.AreEqual(1, createdSurface.Layers.Count());
        }


        [Test]
        public void CreatePerlinSurface()
        {
            var surfaceCreateProcedure = new SurfaceCreateProcedure();
            surfaceCreateProcedure.Parameters["Type"].Set("Perlin");
            {
                var perlinParameter = surfaceCreateProcedure.Parameters["Type"].Parameters["Perlin"];
                perlinParameter.Parameters["Width"].Set(150);
                perlinParameter.Parameters["Length"].Set(170);
                perlinParameter.Parameters["Height Scale"].Set(50);
                perlinParameter.Parameters["Cell Size"].Set(2f);
                perlinParameter.Parameters["Interpolation"].Set("TopLeft");
                perlinParameter.Parameters["Frequency"].Set(5);
                perlinParameter.Parameters["Roughness"].Set(5);
                perlinParameter.Parameters["Seed"].Set(45);
                perlinParameter.Parameters["Offset"].Set(new Vector2D(0, 0));
            }
            surfaceCreateProcedure.Execute();

            var createdSurface = surfaceCreateProcedure.Outputs["Output"].Dequeue<SurfaceEntity>();

            Assert.AreEqual(150, createdSurface.Width);
            Assert.AreEqual(170, createdSurface.Length);
            Assert.AreEqual(76, createdSurface.NumColumns);
            Assert.AreEqual(86, createdSurface.NumRows);
            Assert.AreEqual(1, createdSurface.Layers.Count());

            var createdSurfaceLayer = createdSurface.GetLayer<HeightLayer>();
            Assert.LessOrEqual(createdSurfaceLayer.MaxHeight, 50);
            Assert.GreaterOrEqual(createdSurfaceLayer.MinHeight, 0);
        }


        [Test]
        public void CreateRamSurface()
        {
            var surfaceCreateProcedure = new SurfaceCreateProcedure();
            surfaceCreateProcedure.Parameters["Type"].Set("Ramp");
            {
                var rampParameter = surfaceCreateProcedure.Parameters["Type"].Parameters["Ramp"];
                rampParameter.Parameters["Width"].Set(110);
                rampParameter.Parameters["Length"].Set(50);
                rampParameter.Parameters["Height Scale"].Set(50);
                rampParameter.Parameters["Cell Size"].Set(0.5f);
                rampParameter.Parameters["Interpolation"].Set("TopLeft");
                rampParameter.Parameters["Shape"].Set("Gradient X");
                rampParameter.Parameters["Method"].Set("Linear");
                rampParameter.Parameters["Invert"].Set(false);
                rampParameter.Parameters["Offset"].Set(new Vector2D(0, 0));
                rampParameter.Parameters["Size"].Set(new Vector2D(1, 1));
                rampParameter.Parameters["Continuity"].Set("Mirror");
            }
            surfaceCreateProcedure.Execute();

            var createdSurface = surfaceCreateProcedure.Outputs["Output"].Dequeue<SurfaceEntity>();
            Assert.AreEqual(110, createdSurface.Width);
            Assert.AreEqual(50, createdSurface.Length);
            Assert.AreEqual(221, createdSurface.NumColumns);
            Assert.AreEqual(101, createdSurface.NumRows);
        }


        [Test]
        public void CreateTileableSurface()
        {
            var surfaceCreateProcedure = new SurfaceCreateProcedure();
            surfaceCreateProcedure.Parameters["Type"].Set("Tileable");
            {
                var tileableParameter = surfaceCreateProcedure.Parameters["Type"].Parameters["Tileable"];
                tileableParameter.Parameters["Width"].Set(75);
                tileableParameter.Parameters["Length"].Set(82);
                tileableParameter.Parameters["Height Scale"].Set(23);
                tileableParameter.Parameters["Cell Size"].Set(1f);
                tileableParameter.Parameters["Interpolation"].Set("TopLeft");
                tileableParameter.Parameters["Frequency"].Set(new Vector2D(3, 3));
                tileableParameter.Parameters["Seed"].Set(0);
            }
            surfaceCreateProcedure.Execute();
            var createdSurface = surfaceCreateProcedure.Outputs["Output"].Dequeue<SurfaceEntity>();
            Assert.AreEqual(75, createdSurface.Width);
            Assert.AreEqual(82 ,createdSurface.Length);

            var createdSurfaceLayer = createdSurface.GetLayer<HeightLayer>();
            Assert.LessOrEqual(createdSurfaceLayer.MaxHeight, 23);
            Assert.GreaterOrEqual(createdSurfaceLayer.MinHeight, 0);
        }
    }
}