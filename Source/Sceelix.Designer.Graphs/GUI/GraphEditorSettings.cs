using Sceelix.Designer.Annotations;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Settings.Types;

namespace Sceelix.Designer.Graphs.GUI
{
    [ApplicationSettings("Editors/Graph")]
    public class GraphEditorSettings : ApplicationSettings
    {
        /// <summary>
        /// Determines if graphs should be immediately executed if a result-critical is performed 
        /// (parameter change, new nodes or edge connections, etc.). This is very useful to obtain
        /// instant feedback on performed changes, but could become inconvenient for larger and 
        /// more time-consuming graphs. 
        /// </summary>
        public readonly BoolApplicationField LiveExecution = new BoolApplicationField(true);

        /// <summary>
        /// Indicates if the graph changes should be saved to file when the graph is executed.
        /// </summary>
        public readonly BoolApplicationField SaveOnExecution = new BoolApplicationField(true);


        /// <summary>
        /// Indicates if the log window entries should be cleared when the graph is executed.
        /// </summary>
        public readonly BoolApplicationField ClearLogsOnExecution = new BoolApplicationField(true);

        /// <summary>
        /// Tracks information about the nodes, edges and ports that an entity has gone through.
        /// This is very useful to understand the nodes that contributed to the creation of an
        /// entity, when it is selected, but can carry a significant performance overhead for more 
        /// complex graphs.
        /// </summary>
        public readonly BoolApplicationField TrackEntityPaths = new BoolApplicationField(true);


        /// <summary>
        /// Shows, at each port, how many entities have come in (for input ports) or come out (for
        /// output ports) in the last graph execution.
        /// </summary>
        public readonly BoolApplicationField ViewEntityCount = new BoolApplicationField(true){ AllowsPreview = true };


        public GraphEditorSettings()
            : base("GraphEditor")
        {
        }
    }
}