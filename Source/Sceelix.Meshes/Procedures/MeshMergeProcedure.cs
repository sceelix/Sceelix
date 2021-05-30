using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Merges multiple meshes into a single one.
    /// </summary>
    [Procedure("266f147e-064f-44dc-8a38-5c9b7dacca74", Label = "Mesh Merge", Category = "Mesh")]
    public class MeshMergeProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (square) input means that the node will be executed once for all meshes. Useful to merge any collection of meshes at once. <br/>
        /// Setting a <b>Dual</b> (circles) input means that the node will be executed once for each pair of meshes. Useful to merge exactly two meshes at once.
        /// </summary>
        private readonly DualOrCollectiveInputChoiceParameter<MeshEntity> _input = new DualOrCollectiveInputChoiceParameter<MeshEntity>("Input", "Collective");

        /// <summary>
        /// The merged meshes, according to the defined criteria.
        /// </summary>
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// Criteria on which to group the meshes. If none is defined, all the meshes will be merged into one. 
        /// </summary>
        private readonly ListParameter _parameterCriteria = new ListParameter("Criteria", () => new ObjectParameter("Criterium") {EntityEvaluation = true, Description = "Criterium on which to group the surfaces. Can access the attributes of each mesh using the @@attributeName notation."});

        /// <summary>
        /// How the scope for the merged meshes should be defined.<br/>
        /// <b>First</b>: The scope from the first mesh will be selected and adapted to the new mesh size.<br/>
        /// <b>Reset</b>: The scope will be reset, meaning that it will be aligned to the world axes.
        /// </summary>
        private readonly ChoiceParameter _parameterScopeMerge = new ChoiceParameter("Scope Selection", "First", "First", "Reset");



        private string GetString(MeshEntity meshEntity, IEnumerable<ObjectParameter> criteria)
        {
            return string.Join(" ", criteria.Select(val => val.Get(meshEntity).ToString()).ToArray());
        }



        protected override void Run()
        {
            IEnumerable<MeshEntity> meshes = _input.Read(); //[data];

            IEnumerable<IGrouping<string, MeshEntity>> groups = meshes.GroupBy(val => GetString(val, _parameterCriteria.Items.OfType<ObjectParameter>()));

            foreach (IGrouping<string, MeshEntity> grouping in groups)
            {
                MeshEntity superMeshEntity = null;

                foreach (MeshEntity mesh in grouping)
                    if (superMeshEntity == null)
                    {
                        superMeshEntity = mesh;
                    }
                    else
                    {
                        //try to merge attributes, where possible
                        mesh.Attributes.MergeAttributesTo(superMeshEntity.Attributes);

                        //add the faces to the 
                        superMeshEntity.AddRange(mesh);
                    }

                //having gone through all the items in the list, finalize and output the merged shape
                if (superMeshEntity != null)
                {
                    if (_parameterScopeMerge.Value == "Reset")
                        superMeshEntity.BoxScope = BoxScope.Identity;

                    superMeshEntity.AdjustScope();
                    _output.Write(superMeshEntity);
                }
            }
        }
    }
}