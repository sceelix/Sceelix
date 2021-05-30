using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Flips mesh faces or coordinates.
    /// </summary>
    [Procedure("2ac8a430-827d-4886-9bf4-0a99a621f2b2", Label = "Mesh Flip", Category = "Mesh")]
    public class MeshFlipProcedure : TransferProcedure<MeshEntity>
    {
        /// <summary>
        /// Type of mesh flipping operation to perform.
        /// </summary>
        private readonly SelectListParameter<MeshFlipParameter> _parameterFlip = new SelectListParameter<MeshFlipParameter>("Type", "Faces");


        public override IEnumerable<string> Tags => base.Tags.Union(_parameterFlip.SubParameterLabels);



        protected override MeshEntity Process(MeshEntity meshEntity)
        {
            foreach (MeshFlipParameter transformMeshParameter in _parameterFlip.Items)
                meshEntity = transformMeshParameter.Transform(meshEntity);

            return meshEntity;
        }



        #region Abstract Parameter

        public abstract class MeshFlipParameter : CompoundParameter
        {
            protected MeshFlipParameter(string label)
                : base(label)
            {
            }



            public abstract MeshEntity Transform(MeshEntity meshEntity);
        }

        #endregion

        #region Flip Coordinates

        /// <summary>
        /// Flips coordinates of all vertex positions.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshFlipProcedure.MeshFlipParameter" />
        public class FlipCoordinatesParameter : MeshFlipParameter
        {
            /// <summary>
            /// Coordinates to flip. For example, "X&lt;-&gt;Y" flips the X with the Y coordinate.
            /// </summary>
            private readonly ChoiceParameter _parameterChoiceCoords = new ChoiceParameter("Coordinates", "X<->Y", "X<->Y", "X<->Z", "Y<->Z");



            public FlipCoordinatesParameter()
                : base("Coordinates")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                switch (_parameterChoiceCoords.Value)
                {
                    case "X<->Y":
                        foreach (Vertex vertex in meshEntity.FaceVertices)
                        {
                            vertex.Position = vertex.Position.FlipXY();

                            if (vertex.Normal.HasValue)
                                vertex.Normal = vertex.Normal.Value.FlipXY();
                        }

                        break;
                    case "X<->Z":
                        foreach (Vertex vertex in meshEntity.FaceVertices)
                        {
                            vertex.Position = vertex.Position.FlipXZ();

                            if (vertex.Normal.HasValue)
                                vertex.Normal = vertex.Normal.Value.FlipXZ();
                        }

                        break;
                    case "Y<->Z":
                        foreach (Vertex vertex in meshEntity.FaceVertices)
                        {
                            vertex.Position = vertex.Position.FlipYZ();

                            if (vertex.Normal.HasValue)
                                vertex.Normal = vertex.Normal.Value.FlipYZ();
                        }

                        break;
                }

                return meshEntity;
            }
        }

        #endregion

        #region Flip Faces

        /// <summary>
        /// Flips the orientation of the mesh faces.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshFlipProcedure.MeshFlipParameter" />
        public class FlipFacesParameter : MeshFlipParameter
        {
            public FlipFacesParameter()
                : base("Faces")
            {
            }



            public override MeshEntity Transform(MeshEntity meshEntity)
            {
                foreach (Face face in meshEntity)
                    face.Flip();

                return meshEntity;
            }
        }

        #endregion
    }
}