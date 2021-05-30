using Sceelix.Core.Parameters;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Parameters
{
    /// <summary>
    /// Separation options.
    /// </summary>
    /// <seealso cref="Sceelix.Core.Parameters.CompoundParameter" />
    public class MeshSeparateParameter : CompoundParameter
    {
        /// <summary>
        /// Defines what the attributes of the individual meshes should be:
        /// if they should take the attributes of the parent, use the ones of the face
        /// or mix both.
        /// </summary>
        private readonly ChoiceParameter _parameterAttributes = new ChoiceParameter("Attributes", "Parent and Face", "Parent", "Face", "Parent and Face");

        /// <summary>
        /// Defines how the 3D Scope of the individual meshes should be:
        /// if they should inherit from the parent mesh or assume new ones, adjusted
        /// to the orientation of the face.
        /// </summary>
        private readonly ChoiceParameter _parameterScope = new ChoiceParameter("Scope", "Parent", "Parent", "Derived", "Face");



        public MeshSeparateParameter()
            : base("Separate")
        {
        }



        public MeshEntity Process(MeshEntity parent, Face face)
        {
            //the deepclone is important
            //otherwise we cannot perform clean properly
            //because despite we are separating the faces, the vertices are still shared
            //so they need to be cloned
            MeshEntity newMeshEntity = (MeshEntity) new MeshEntity(face).DeepClone();
            newMeshEntity.CleanFaceConnections();

            if (_parameterScope.Value == "Parent")
                newMeshEntity.AdjustScope(parent.BoxScope);
            else if (_parameterScope.Value == "Derived")
                newMeshEntity.BoxScope = face.GetDerivedScope(parent.BoxScope);
            //newMeshEntity.BoxScope.AdjustToFace(parent.BoxScope, face);

            //Now, the attributes
            if (_parameterAttributes.Value == "Parent")
            {
                parent.Attributes.SetAttributesTo(newMeshEntity.Attributes);
            }
            else if (_parameterAttributes.Value == "Face")
            {
                //promote the attributes of the face
                face.Attributes.SetAttributesTo(newMeshEntity.Attributes);
            }
            else if (_parameterAttributes.Value == "Parent and Face")
            {
                //promote the attributes of the face
                face.Attributes.SetAttributesTo(newMeshEntity.Attributes);

                //copy the remaining attributes of the parent
                parent.Attributes.MergeAttributesTo(newMeshEntity.Attributes);
            }

            return newMeshEntity;
        }
    }
}