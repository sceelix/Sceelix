using NUnit.Framework;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Procedures;

namespace Sceelix.Meshes.Tests.Procedures
{
    public class MeshCreateTests
    {
        [Test]
        public void CreateBox()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Box");
            {
                var boxParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Box"];
                boxParameter.Parameters["Width"].Set(5f);
                boxParameter.Parameters["Length"].Set(3f);
                boxParameter.Parameters["Height"].Set(7f);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(8, mesh.VertexCount);
            Assert.AreEqual(5 * 3 * 2 + 7 * 3 * 2 + 5 * 7 * 2, mesh.Area);
            Assert.AreEqual(6, mesh.Faces.Count);
        }



        [Test]
        public void CreateCircle()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Circle");
            {
                var circleParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Circle"];
                circleParameter.Parameters["Radius"].Set(1f);
                circleParameter.Parameters["Segments"].Set(25);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(25, mesh.VertexCount);
        }



        [Test]
        public void CreateCross()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Cross");
            {
                var crossParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Cross"];
                crossParameter.Parameters["Width"].Set(5f);
                crossParameter.Parameters["Height"].Set(5f);
                crossParameter.Parameters["Body Width"].Set(3f);
                crossParameter.Parameters["Body Height"].Set(3f);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(12, mesh.VertexCount);
            Assert.AreEqual(21, mesh.Area);
        }



        [Test]
        public void CreateCube()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Cube");
            meshCreateProcedure.Parameters["Primitive"].Parameters["Cube"].Parameters["Size"].Set(2f);
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(8, mesh.VertexCount);
            Assert.AreEqual(2 * 2 * 6, mesh.Area);
            Assert.AreEqual(6, mesh.Faces.Count);
        }



        [Test]
        public void CreateCylinder()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Cylinder");
            {
                var cylinderParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Cylinder"];
                cylinderParameter.Parameters["Height"].Set(10f);
                cylinderParameter.Parameters["Radius"].Set(2f);
                cylinderParameter.Parameters["Segments"].Set(10);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(10f, mesh.BoundingBox.Height);
            Assert.AreEqual(20, mesh.VertexCount);
            Assert.AreEqual(12, mesh.Faces.Count);
        }



        [Test]
        public void CreateDodecahedron()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Dodecahedron");
            meshCreateProcedure.Parameters["Primitive"].Parameters["Dodecahedron"].Parameters["Size"].Set(3f);
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(12, mesh.Faces.Count);
            Assert.AreEqual(20, mesh.VertexCount);
        }



        [Test]
        public void CreateIcosahedron()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Icosahedron");
            meshCreateProcedure.Parameters["Primitive"].Parameters["Icosahedron"].Parameters["Size"].Set(1f);
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(20, mesh.Faces.Count);
            Assert.AreEqual(12, mesh.VertexCount);
        }



        [Test]
        public void CreateLShape()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("L-Shape");
            {
                var lShapeParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["L-Shape"];
                lShapeParameter.Parameters["Width"].Set(5f);
                lShapeParameter.Parameters["Height"].Set(5f);
                lShapeParameter.Parameters["Body Width"].Set(2f);
                lShapeParameter.Parameters["Body Height"].Set(2f);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(6, mesh.VertexCount);
            Assert.AreEqual(16, mesh.Area);
        }



        [Test]
        public void CreateOctahedron()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Octahedron");
            meshCreateProcedure.Parameters["Primitive"].Parameters["Octahedron"].Parameters["Radius"].Set(1f);
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();

            Assert.AreEqual(8, mesh.Faces.Count);
            Assert.AreEqual(6, mesh.VertexCount);
        }



        [Test]
        public void CreateRandomFace()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Random Face");
            {
                var randomFaceParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Random Face"];
                randomFaceParameter.Parameters["Seed"].Set(0);
                randomFaceParameter.Parameters["Size"].Set(10f);
                randomFaceParameter.Parameters["Number of vertices"].Set(10);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(10, mesh.VertexCount);
        }



        [Test]
        public void CreateRectangle()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Rectangle");
            {
                var rectangleParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Rectangle"];
                rectangleParameter.Parameters["Width"].Set(2f);
                rectangleParameter.Parameters["Height"].Set(5f);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(10, mesh.Area);
            Assert.AreEqual(4, mesh.VertexCount);
        }



        [Test]
        public void CreateSphere()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Sphere");
            {
                var sphereParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Sphere"];
                sphereParameter.Parameters["Horizontal Segments"].Set(10);
                sphereParameter.Parameters["Radius"].Set(1f);
                sphereParameter.Parameters["Vertical Segments"].Set(10);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(92, mesh.VertexCount);
        }



        [Test]
        public void CreateStar()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Star");
            {
                var starParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Star"];
                starParameter.Parameters["Points"].Set(5);
                starParameter.Parameters["Radius 1"].Set(2f);
                starParameter.Parameters["Radius 2"].Set(1f);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(10, mesh.VertexCount);
        }



        [Test]
        public void CreateText()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Text");
            {
                var textParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Text"];
                textParameter.Parameters["Text"].Set("Sceelix");
                textParameter.Parameters["Font"].Set("Arial");
                textParameter.Parameters["Size"].Set(10);
                textParameter.Parameters["Style"].Set("Regular");
                textParameter.Parameters["Index"].Set("");
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(8, mesh.Faces.Count);
        }



        [Test]
        public void CreateTShape()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("T-Shape");
            {
                var tShapeParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["T-Shape"];
                tShapeParameter.Parameters["Width"].Set(5f);
                tShapeParameter.Parameters["Height"].Set(5f);
                tShapeParameter.Parameters["Body Width"].Set(2f);
                tShapeParameter.Parameters["Body Height"].Set(2f);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(8, mesh.VertexCount);
        }



        [Test]
        public void CreateUShape()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("U-Shape");
            {
                var uShapeParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["U-Shape"];
                uShapeParameter.Parameters["Width"].Set(5f);
                uShapeParameter.Parameters["Height"].Set(5f);
                uShapeParameter.Parameters["Body Width"].Set(2f);
                uShapeParameter.Parameters["Body Height"].Set(2f);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(8, mesh.VertexCount);
        }



        [Test]
        public void CreateZShape()
        {
            var meshCreateProcedure = new MeshCreateProcedure();
            meshCreateProcedure.Parameters["Primitive"].Set("Z-Shape");
            {
                var zShapeParameter = meshCreateProcedure.Parameters["Primitive"].Parameters["Z-Shape"];
                zShapeParameter.Parameters["Width"].Set(10f);
                zShapeParameter.Parameters["Height"].Set(10f);
                zShapeParameter.Parameters["Body Width"].Set(2f);
                zShapeParameter.Parameters["Body Height"].Set(3f);
            }
            meshCreateProcedure.Execute();

            var mesh = meshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            Assert.AreEqual(8, mesh.VertexCount);
        }
    }
}