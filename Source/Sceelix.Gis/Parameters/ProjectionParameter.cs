using System.Linq;
using DotSpatial.Projections;
using Sceelix.Core.Parameters;

namespace Sceelix.Gis.Parameters
{
    public class ProjectionParameter : CompoundParameter
    {
        private readonly CoordinateSystemCategory _coordinateSystemCategory;
        private readonly ChoiceParameter _parameterChoiceProjection;



        public ProjectionParameter(string label, CoordinateSystemCategory coordinateSystemCategory) : base(label)
        {
            _coordinateSystemCategory = coordinateSystemCategory;
            _parameterChoiceProjection = new ChoiceParameter("Projection", coordinateSystemCategory.Names.First(), coordinateSystemCategory.Names){Description = "Choice of Projection."};

            ReadSubclassFields();
        }



        public ProjectionInfo ProjectionInfo => _coordinateSystemCategory.GetProjection(_parameterChoiceProjection.Value);
    }
}