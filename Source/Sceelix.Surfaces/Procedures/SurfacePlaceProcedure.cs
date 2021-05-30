using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Geometry;
using Sceelix.Mathematics.Parameters;
using Sceelix.Mathematics.Spatial;
using Sceelix.Meshes.Data;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Allows the placement of entities on top of surfaces.
    /// </summary>
    [Procedure("2dbcc442-f71a-43af-a11a-ece62ff48f81", Label = "Surface Place", Category = "Surface")]
    public class SurfacePlaceProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (circle) input means that the node will be executed once per surface. <br/>
        /// Setting a <b>Collective</b> (square) input means that the node will be executed once for all surfaces. Especially useful when actors are to be placed on the surfaces, but it would be complex to match with the right surface.
        /// </summary>
        private readonly SingleOrCollectiveInputChoiceParameter<SurfaceEntity> _parameterInput = new SingleOrCollectiveInputChoiceParameter<SurfaceEntity>("Inputs", "Single");


        /// <summary>
        /// Surface where to place the entities.
        /// </summary>
        /*private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Surface");*/

        /// <summary>
        /// Surface on which the entities where placed.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Surface");

        /// <summary>
        /// Entities to place on the surface.
        /// </summary>
        private readonly SelectListParameter<SurfacePlacementParameter> _parameterEntity = new SelectListParameter<SurfacePlacementParameter>("Entity", "Actor");



        protected override void Run()
        {
            var surfaceEntities = _parameterInput.Read().ToList();
            SurfacePlacementParameter entityOperation = _parameterEntity.Items.FirstOrDefault();
            if (entityOperation != null)
                entityOperation.Run(surfaceEntities);

            _output.Write(surfaceEntities);
        }



        #region Abstract Parameter

        public abstract class SurfacePlacementParameter : CompoundParameter
        {
            protected SurfacePlacementParameter(string label)
                : base(label)
            {
            }



            protected internal abstract void Run(IEnumerable<SurfaceEntity> surfaceEntity);
        }

        #endregion

        #region Actor Placement

        /// <summary>
        /// Places actors on the surface.
        /// </summary>
        /// <seealso cref="SurfacePlaceProcedure.SurfacePlacementParameter" />
        public class ActorPlacementParameter : SurfacePlacementParameter
        {
            /// <summary>
            /// Actors to be placed on the surface.
            /// </summary>
            private readonly CollectiveInput<IActor> _input = new CollectiveInput<IActor>("Actor");

            /// <summary>
            /// Actors that were placed on the surface.
            /// </summary>
            private readonly Output<IActor> _output = new Output<IActor>("Actor");

            /// <summary>
            /// Location to use as placement reference.
            /// </summary>
            private readonly CompoundParameter _parameterSampleLocation = new CompoundParameter("Sample Location",
                new Vector3DParameter("Position", new Vector3D(0f, 0f, 0f)) {Description = "Position relative to the actor scope origin."},
                new ChoiceParameter("Offset", "Relative", "Absolute", "Relative") {Description = "Indicates if the position is measured as absolute units or scope-size relative value (between 0 and 1)"});



            protected ActorPlacementParameter()
                : base("Actor")
            {
            }



            protected internal override void Run(IEnumerable<SurfaceEntity> surfaceEntities)
            {
                List<IActor> entities = _input.Read().ToList();

                foreach (var surfaceEntity in surfaceEntities)
                {
                    //BoundingBox shapeBB = entities.BoxScope.GetBoundingBox();
                    BoundingRectangle surfaceBoundingRectangle = surfaceEntity.BoundingRectangle;

                    var position = (Vector3D) _parameterSampleLocation["Position"].Get();
                    var offsetChoice = (string) _parameterSampleLocation["Offset"].Get();

                    foreach (IActor actor in entities)
                    {
                        Vector3D offset = offsetChoice == "Relative" ? actor.BoxScope.ToRelativeWorldPosition(position) : actor.BoxScope.ToWorldPosition(position);
                        Vector2D flatlocation = offset.ToVector2D();

                        //BoundingRectangle shapeBoundingRectangle = new BoundingRectangle(new Vector2D(shapeBoundingBox.Min.X, shapeBoundingBox.Min.Y), new Vector2D(shapeBoundingBox.Max.X, shapeBoundingBox.Max.Y));

                        if (surfaceBoundingRectangle.Contains(flatlocation))
                        {
                            //var absoluteHeightAt = surfaceEntity.GetAbsoluteHeightAt(new Vector2D(flatlocation.X, flatlocation.Y));
                            var absoluteHeightAt = surfaceEntity.GetLayer<HeightLayer>().GetGenericValue(new Vector2D(flatlocation.X, flatlocation.Y));

                            actor.InsertInto(new BoxScope(actor.BoxScope, translation: actor.BoxScope.Translation + new Vector3D(0, 0, absoluteHeightAt - actor.BoxScope.Translation.Z)));
                        }
                    }
                }


                _output.Write(entities);
            }
        }

        #endregion

        #region Mesh Placement

        /// <summary>
        /// Places meshes on the surface.
        /// </summary>
        /// <seealso cref="SurfacePlaceProcedure.SurfacePlacementParameter" />
        public class MeshPlacementParameter : SurfacePlacementParameter
        {
            /// <summary>
            /// Meshes to be placed on the surface.
            /// </summary>
            private readonly CollectiveInput<MeshEntity> _input = new CollectiveInput<MeshEntity>("Mesh");

            /// <summary>
            /// Meshes that were placed on the surface.
            /// </summary>
            private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Mesh");

            /// <summary>
            /// Indicates if the meshes adapt all their vertices on the terrain, of if they should be forced to remain planar 
            /// Enforcing planarity could mean that not all vertices would be lying on the terrain.
            /// </summary>
            private readonly BoolParameter _parameterKeepPlanar = new BoolParameter("Keep Planar", false);



            protected MeshPlacementParameter()
                : base("Mesh")
            {
            }



            protected internal override void Run(IEnumerable<SurfaceEntity> surfaceEntities)
            {
                List<MeshEntity> meshes = _input.Read().ToList();

                foreach (var surfaceEntity in surfaceEntities)
                {
                    BoundingRectangle surfaceBoundingRectangle = surfaceEntity.BoundingRectangle;
                    var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
                    bool keepPlanar = _parameterKeepPlanar.Value;

                    //iterate over all 
                    foreach (MeshEntity mesh in meshes)
                        if (surfaceBoundingRectangle.Intersects(mesh.BoundingRectangle))
                        {
                            foreach (Face face in mesh.Faces)
                            {
                                foreach (Vertex vertex in face.AllVertices)
                                {
                                    var height = heightLayer != null ? heightLayer.GetGenericValue(vertex.Position.ToVector2D()) : 0;

                                    if (!float.IsNaN(height) && !float.IsInfinity(height))
                                        vertex.Position = new Vector3D(vertex.Position.X, vertex.Position.Y, height);
                                }

                                if (keepPlanar && !face.RecalculateIsPlanar())
                                {
                                    //calculate the centroid
                                    //create a plane at that location 
                                    var centroid = face.Centroid;
                                    var normal = face.RecalculateNormal();

                                    Plane3D plane3D = new Plane3D(normal, centroid);

                                    foreach (Vertex vertex in face.AllVertices)
                                    {
                                        var height = plane3D.GetHeightAt(vertex.Position.ToVector2D());

                                        //if a valid height has been found
                                        if (!float.IsNaN(height) && !float.IsInfinity(height))
                                            vertex.Position = new Vector3D(vertex.Position.X, vertex.Position.Y, height);
                                    }
                                }
                            }

                            mesh.AdjustScope();
                        }
                }

                _output.Write(meshes);
            }
        }

        #endregion
    }
}