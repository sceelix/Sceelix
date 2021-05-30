using System;

namespace Sceelix.Designer.ProjectExplorer.Messages
{
    public class ProjectInfoRequest
    {
        private readonly object _requestingObject;



        public ProjectInfoRequest(Object requestingObject)
        {
            _requestingObject = requestingObject;
        }



        public object RequestingObject
        {
            get { return _requestingObject; }
        }
    }
}