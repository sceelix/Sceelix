using Sceelix.Designer.Annotations;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Settings.Types;

namespace Sceelix.Designer.ProjectExplorer.Management
{
    [ApplicationSettings("Project Explorer")]
    public class ProjectExplorerSettings : ApplicationSettings
    {
        /// <summary>
        /// Indicates if the project explorer should focus on the file currently active in the document area.
        /// </summary>
        public readonly BoolApplicationField TrackDocumentArea = new BoolApplicationField(true);

        public ProjectExplorerSettings() 
            : base("Project Explorer")
        {
        }
    }
}
