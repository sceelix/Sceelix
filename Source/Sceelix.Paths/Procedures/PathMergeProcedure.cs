using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Paths.Data;

namespace Sceelix.Paths.Procedures
{
    /// <summary>
    /// Merges several paths into a single one.
    /// </summary>
    [Procedure("807f4655-bd7f-48e1-b809-1a9dee94c57a", Label = "Path Merge", Category = "Path")]
    public class PathMergeProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (square) input means that the node will be executed once for all paths. Useful to merge any collection of paths at once. <br/>
        /// Setting a <b>Dual</b> (circles) input means that the node will be executed once for each pair of paths. Useful to merge exactly two paths at once.
        /// </summary>
        private readonly DualOrCollectiveInputChoiceParameter<PathEntity> _input = new DualOrCollectiveInputChoiceParameter<PathEntity>("Input", "Collective");
        //private readonly CollectiveInput<PathEntity> _input = new CollectiveInput<PathEntity>("Input");

        /// <summary>
        /// The merged paths, according to the defined criteria.
        /// </summary>
        private readonly Output<PathEntity> _output = new Output<PathEntity>("Output");

        /// <summary>
        /// Criteria on which to group the paths. If none is defined, all the paths will be merged into one.
        /// </summary>
        private readonly ListParameter _parameterCriteria = new ListParameter("Criteria", () => new ObjectParameter("Criterium") {EntityEvaluation = true, Description = "Criterium on which to group the paths. Can access the attributes of each path using the @@attributeName notation."});

        /// <summary>
        /// How the scope for the merged paths should be defined.<br/>
        /// <b>First</b>: The scope from the first path will be selected and adapted to the new mesh size.<br/>
        /// <b>Reset</b>: The scope will be reset, meaning that it will be aligned to the world axes.
        /// </summary>
        private readonly ChoiceParameter _parameterScopeMerge = new ChoiceParameter("Scope Selection", "First", "First", "Reset");



        private string GetString(PathEntity pathEntity, IEnumerable<ObjectParameter> criteria)
        {
            return string.Join(" ", criteria.Select(val => val.Get(pathEntity).ToString()).ToArray());
        }



        protected override void Run()
        {
            List<PathEntity> paths = _input.Read().ToList();

            IEnumerable<IGrouping<string, PathEntity>> groups = paths.GroupBy(val => GetString(val, _parameterCriteria.Items.OfType<ObjectParameter>()));
            foreach (IGrouping<string, PathEntity> grouping in groups)
            {
                PathEntity superPathEntity = null;

                foreach (PathEntity path in grouping)
                    if (superPathEntity == null)
                    {
                        superPathEntity = path;
                    }
                    else
                    {
                        //try to merge attributes, where possible
                        path.Attributes.MergeAttributesTo(superPathEntity.Attributes);

                        //add the faces to the 
                        superPathEntity.AddEdges(path.Edges);
                    }

                //having gone through all the items in the list, finalize and output the merged shape
                if (superPathEntity != null)
                {
                    if (_parameterScopeMerge.Value == "Reset")
                        superPathEntity.BoxScope = BoxScope.Identity;

                    superPathEntity.AdjustScope();
                    _output.Write(superPathEntity);
                }
            }

            /*if (paths.Count > 0)
            {
                PathEntity mainNetwork = paths[0];
                for (int index = 1; index < paths.Count; index++)
                {
                    var network = paths[index];
                    mainNetwork.Attributes.MergeAttributesTo(network.Attributes);

                    mainNetwork.AddEdges(network.Edges);
                }

                _output.Write(mainNetwork);
            }*/
        }
    }
}