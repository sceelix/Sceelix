using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class ProjectItemMoved
    {
        private readonly string _destination;
        private readonly ProjectItem _item;
        private readonly string _origin;



        public ProjectItemMoved(ProjectItem item, string origin, string destination)
        {
            _item = item;
            _origin = origin;
            _destination = destination;
        }



        public ProjectItem Item
        {
            get { return _item; }
        }



        public string Origin
        {
            get { return _origin; }
        }



        public string Destination
        {
            get { return _destination; }
        }
    }
}