using System;
using System.Collections.Generic;
using System.Linq;
using DotSpatial.Projections;
using Sceelix.Core.Parameters;

namespace Sceelix.Gis.Parameters
{
    /// <summary>
    /// Selection of data projection.
    /// </summary>
    public class ProjectionSelectParameter : SelectListParameter
    {
        public ProjectionSelectParameter() : this("Projection")
        {
        }



        public ProjectionSelectParameter(string label) : base(label,
            () => new SelectListParameter("Geographic", GetGeographic().ToArray()){Description = "Chooses a geographic coordinate system."},
            () => new SelectListParameter("Projected", GetProjected().ToArray()){Description = "Chooses a projected coordinate system."})
        {
        }



        public ProjectionInfo Projection => ((ProjectionParameter) ((ListParameter) Items.First()).Items.First()).ProjectionInfo;



        private static IEnumerable<Func<Parameter>> GetGeographic()
        {
            foreach (var geographicSystem in KnownCoordinateSystems.Geographic.Names)
            {
                var system = geographicSystem;
                yield return () => new ProjectionParameter(system, KnownCoordinateSystems.Geographic.GetCategory(system)){Description = $"Geographic Coordinate System for {system}."};
            }
        }



        private static IEnumerable<Func<Parameter>> GetProjected()
        {
            foreach (var geographicSystem in KnownCoordinateSystems.Projected.Names)
            {
                var system = geographicSystem;
                yield return () => new ProjectionParameter(system, KnownCoordinateSystems.Projected.GetCategory(system)){Description = $"Projected Coordinate System for {system}."};
            }
        }
    }
}