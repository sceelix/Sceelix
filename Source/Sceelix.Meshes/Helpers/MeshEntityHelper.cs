using Sceelix.Meshes.Data;
using Sceelix.Meshes.Procedures;

namespace Sceelix.Meshes.Helpers
{
    public static class MeshEntityHelper
    {
        private static readonly MeshCreateProcedure MeshCreateProcedure = new MeshCreateProcedure();
        private static readonly MeshModifyProcedure MeshModifyProcedure = new MeshModifyProcedure();



        /// <summary>
        /// Creates a Box Mesh.
        /// </summary>
        /// <param name="width">Size of the box in X.</param>
        /// <param name="length">Size of the box in Y.</param>
        /// <param name="height">Size of the box in Z.</param>
        /// <returns></returns>
        public static MeshEntity CreateBox(float width = 1, float length = 1, float height = 1)
        {
            lock (MeshCreateProcedure)
            {
                MeshCreateProcedure.Parameters["Primitive"].Set("Box");
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Box"].Parameters["Width"].Set(width);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Box"].Parameters["Length"].Set(length);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Box"].Parameters["Height"].Set(height);
                MeshCreateProcedure.Execute();

                return MeshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }



        /// <summary>
        /// Creates a Cylinder Mesh.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="segments"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static MeshEntity CreateCone(float radius = 1, int segments = 5, float height = 1)
        {
            lock (MeshCreateProcedure)
            {
                MeshCreateProcedure.Parameters["Primitive"].Set("Cone");
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Cone"].Parameters["Radius"].Set(radius);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Cone"].Parameters["Segments"].Set(segments);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Cone"].Parameters["Height"].Set(height);
                MeshCreateProcedure.Execute();
                return MeshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }



        /// <summary>
        /// Creates a Cube Mesh.
        /// </summary>
        /// <param name="size">Size of the cube's edges in every direction.</param>
        /// <returns></returns>
        public static MeshEntity CreateCube(float size = 1)
        {
            lock (MeshCreateProcedure)
            {
                MeshCreateProcedure.Parameters["Primitive"].Set("Cube");
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Cube"].Parameters["Size"].Set(size);
                MeshCreateProcedure.Execute();

                return MeshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }



        /// <summary>
        /// Creates a Cylinder Mesh.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="segments"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static MeshEntity CreateCylinder(float radius = 1, int segments = 5, float height = 1)
        {
            lock (MeshCreateProcedure)
            {
                MeshCreateProcedure.Parameters["Primitive"].Set("Cylinder");
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Cylinder"].Parameters["Radius"].Set(radius);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Cylinder"].Parameters["Segments"].Set(segments);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Cylinder"].Parameters["Height"].Set(height);
                MeshCreateProcedure.Execute();
                return MeshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }



        /// <summary>
        /// Creates a Rectangle Mesh.
        /// </summary>
        /// <param name="width">Size of the rectangle in X.</param>
        /// <param name="length">Size of the rectangle in Y.</param>
        /// <returns></returns>
        public static MeshEntity CreateRectangle(float width = 1, int length = 5)
        {
            lock (MeshCreateProcedure)
            {
                MeshCreateProcedure.Parameters["Primitive"].Set("Rectangle");
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Rectangle"].Parameters["Width"].Set(width);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Rectangle"].Parameters["Height"].Set(length);
                MeshCreateProcedure.Execute();
                return MeshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }



        /// <summary>
        /// Creates a Sphere Mesh.
        /// </summary>
        /// <param name="radius">Radius of the sphere.</param>
        /// <param name="verticalSegments">Number of segments/partitions in the vertical direction.</param>
        /// <param name="horizontalSegments">Number of segments/partitions in the horizontal direction.</param>
        /// <returns></returns>
        public static MeshEntity CreateSphere(float radius = 1, int verticalSegments = 5, int horizontalSegments = 5)
        {
            lock (MeshCreateProcedure)
            {
                MeshCreateProcedure.Parameters["Primitive"].Set("Sphere");
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Sphere"].Parameters["Radius"].Set(radius);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Sphere"].Parameters["Vertical Segments"].Set(verticalSegments);
                MeshCreateProcedure.Parameters["Primitive"].Parameters["Sphere"].Parameters["Horizontal Segments"].Set(horizontalSegments);
                MeshCreateProcedure.Execute();
                return MeshCreateProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }



        /// <summary>
        /// Extrudes all the faces of the mesh in the direction of their normal faces.
        /// </summary>
        /// <param name="entity">Mesh to be extruded.</param>
        /// <param name="amount">Size of the extrusion blocks.</param>
        /// <param name="cap">If true, a face with a flipped orientation will be added where the original mesh faces were.</param>
        /// <returns></returns>
        public static MeshEntity Extrude(this MeshEntity entity, float amount = 1, bool cap = false)
        {
            lock (MeshCreateProcedure)
            {
                MeshModifyProcedure.Inputs["Input"].Enqueue(entity);
                MeshModifyProcedure.Parameters["Operation"].Set("Extrude");
                MeshModifyProcedure.Parameters["Operation"].Parameters["Extrude"].Parameters["Amount"].Set(amount);
                MeshModifyProcedure.Parameters["Operation"].Parameters["Extrude"].Parameters["Cap"].Set(cap);
                MeshModifyProcedure.Execute();
                return MeshModifyProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }



        /// <summary>
        /// Replaces all faces with offset versions that either go inward or outward.
        /// </summary>
        /// <param name="entity">Mesh to be extruded.</param>
        /// <param name="amount">Size of the extrusion blocks.</param>
        /// <param name="cap">If true, a face with a flipped orientation will be added where the original mesh faces were.</param>
        /// <returns></returns>
        public static MeshEntity Offset(this MeshEntity entity, float amount = 1, string attributeSection = null)
        {
            lock (MeshCreateProcedure)
            {
                MeshModifyProcedure.Inputs["Input"].Enqueue(entity);
                MeshModifyProcedure.Parameters["Operation"].Set("Offset");
                MeshModifyProcedure.Parameters["Operation"].Parameters["Offset"].Parameters["Amount"].Set(amount);

                if (attributeSection != null)
                    MeshModifyProcedure.Parameters["Operation"].Parameters["Offset"].Parameters["Section"].Set(attributeSection);

                MeshModifyProcedure.Execute();

                return MeshModifyProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }



        /// <summary>
        /// Creates a pyramid on all the faces of the mesh in the direction of their normal faces.
        /// </summary>
        /// <param name="entity">Mesh to be pyramidized.</param>
        /// <param name="height">Height of the pyramid.</param>
        /// <param name="cap">If true, a face with a flipped orientation will be added where the original mesh faces were.</param>
        /// <returns></returns>
        public static MeshEntity Pyramidize(this MeshEntity entity, float height = 1, bool cap = false)
        {
            lock (MeshCreateProcedure)
            {
                MeshModifyProcedure.Inputs["Input"].Enqueue(entity);
                MeshModifyProcedure.Parameters["Operation"].Set("Pyramidize");
                MeshModifyProcedure.Parameters["Operation"].Parameters["Pyramidize"].Parameters["Height"].Set(height);
                MeshModifyProcedure.Parameters["Operation"].Parameters["Pyramidize"].Parameters["Cap"].Set(cap);
                MeshModifyProcedure.Execute();
                return MeshModifyProcedure.Outputs["Output"].Dequeue<MeshEntity>();
            }
        }
    }
}