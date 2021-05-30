using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class ProjectItemsDropped
    {
        private readonly ProjectItem[] _projectTreeItems;



        public ProjectItemsDropped(ProjectItem[] projectTreeItems)
        {
            _projectTreeItems = projectTreeItems;
        }



        public ProjectItem[] ProjectTreeItems
        {
            get { return _projectTreeItems; }
        }
    }
}
