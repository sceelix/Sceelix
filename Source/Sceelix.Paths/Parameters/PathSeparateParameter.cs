using Sceelix.Core.Parameters;
using Sceelix.Paths.Data;

namespace Sceelix.Paths.Parameters
{
    /// <summary>
    /// Separation options.
    /// </summary>
    /// <seealso cref="Sceelix.Core.Parameters.CompoundParameter" />
    public class PathSeparateParameter : CompoundParameter
    {
        /// <summary>
        /// Defines what the attributes of the individual path entities should be:
        /// if they should take the attributes of the parent, use the ones of the edge
        /// or mix both.
        /// </summary>
        private readonly ChoiceParameter _parameterAttributes = new ChoiceParameter("Attributes", "Parent and Edge", "Parent", "Edge", "Parent and Edge");

        /// <summary>
        /// Defines how the 3D Scope of the individual path entities should be:
        /// if they should inherit from the parent path or assume new ones, adjusted
        /// to the orientation of the edge.
        /// </summary>
        private readonly ChoiceParameter _parameterScope = new ChoiceParameter("Scope", "Parent", "Parent", "Edge");



        public PathSeparateParameter()
            : base("Separate")
        {
        }



        public PathEntity Process(PathEntity parent, PathEdge edge)
        {
            //the deepclone is important
            //otherwise we cannot perform clean properly
            //because despite we are separating the edges, the vertices are still shared
            //so they need to be cloned
            PathEntity newPathEntity = (PathEntity) new PathEntity(edge).DeepClone();
            newPathEntity.CleanConnections();

            if (_parameterScope.Value == "Parent")
                newPathEntity.AdjustScope(parent.BoxScope);

            //Now, the attributes
            if (_parameterAttributes.Value == "Parent")
            {
                parent.Attributes.SetAttributesTo(newPathEntity.Attributes);
            }
            else if (_parameterAttributes.Value == "Edge")
            {
                //promote the attributes of the edge
                edge.Attributes.SetAttributesTo(newPathEntity.Attributes);
            }
            else if (_parameterAttributes.Value == "Parent and Edge")
            {
                //promote the attributes of the edge
                edge.Attributes.SetAttributesTo(newPathEntity.Attributes);

                //copy the remaining attributes of the parent
                parent.Attributes.MergeAttributesTo(newPathEntity.Attributes);
            }

            return newPathEntity;
        }
    }
}