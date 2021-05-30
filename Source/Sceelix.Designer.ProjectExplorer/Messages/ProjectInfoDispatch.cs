using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class ProjectInfoDispatch
    {
        private readonly Project _project;



        public ProjectInfoDispatch(Project project)
        {
            _project = project;
        }



        public Project Project
        {
            get { return _project; }
        }
    }
}