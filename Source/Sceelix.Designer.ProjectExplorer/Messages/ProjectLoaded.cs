using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class ProjectLoaded
    {
        private readonly Project _project;



        public ProjectLoaded(Project project)
        {
            _project = project;
        }



        public Project Project
        {
            get { return _project; }
        }
    }
}