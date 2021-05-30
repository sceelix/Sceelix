using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Procedures
{
    /// <summary>
    /// Merges multiple surfaces and their layers into a single one.
    /// </summary>
    [Procedure("e7a1fccf-9331-4d76-8911-ce398b26217f", Label = "Surface Merge", Category = "Surface")]
    public class SurfaceMergeProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (square) input means that the node will be executed once for all surfaces. Useful to merge any collection of surfaces at once. <br/>
        /// Setting a <b>Dual</b> (circles) input means that the node will be executed once for each pair of surfaces. Useful to merge exactly two surfaces at once.
        /// </summary>
        private readonly DualOrCollectiveInputChoiceParameter<SurfaceEntity> _input = new DualOrCollectiveInputChoiceParameter<SurfaceEntity>("Input", "Collective");

        /// <summary>
        /// The merged surfaces, according to the defined criteria.
        /// </summary>
        private readonly Output<SurfaceEntity> _output = new Output<SurfaceEntity>("Output");

        /// <summary>
        /// Criteria on which to group the surfaces. If none is defined, all the surfaces will be merged into one.
        /// </summary>
        private readonly ListParameter _parameterCriteria = new ListParameter("Criteria", () => new ObjectParameter("Criterium") {EntityEvaluation = true, Description = "Criterium on which to group the surfaces. Can access the attributes of each surface using the @@attributeName notation."});



        private string GetString(SurfaceEntity meshEntity, IEnumerable<ObjectParameter> augmentations)
        {
            return string.Join(" ", augmentations.Select(val => val.Get(meshEntity).ToString()).ToArray());
        }



        protected override void Run()
        {
            IEnumerable<SurfaceEntity> surfaces = _input.Read();

            //divide the surfaces into groups, according to criteria
            IEnumerable<IGrouping<string, SurfaceEntity>> groups = surfaces.GroupBy(val => GetString(val, _parameterCriteria.Items.OfType<ObjectParameter>()));

            foreach (IGrouping<string, SurfaceEntity> grouping in groups)
            {
                var groupingSurfaces = grouping.ToList();

                //if there is only one surface, just return it and proceed to the next group
                if (groupingSurfaces.Count == 1)
                {
                    _output.Write(groupingSurfaces.First());
                    continue;
                }

                //otherwise, calculate the total bounding box of the surfaces we are merging
                var selectedSurface = groupingSurfaces.First();

                //create the new surface that will enclose the information of all the previous ones
                SurfaceEntity superSurfaceEntity = new SurfaceEntity(selectedSurface.NumColumns, selectedSurface.NumRows, selectedSurface.CellSize)
                {
                    Origin = selectedSurface.Origin,
                    Material = (Material) selectedSurface.Material.DeepClone()
                };

                //reverse the list of surfaces, since value prevalence is from the last to the first
                groupingSurfaces.Reverse();

                foreach (var surface in groupingSurfaces)
                    superSurfaceEntity.Merge(surface);

                _output.Write(superSurfaceEntity);
            }
        }
    }
}