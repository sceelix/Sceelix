using Sceelix.Core;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs;
using Sceelix.Core.Procedures;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.Messages
{
    public class GraphExecutionFinished
    {
        private readonly IEntity[] _data;
        private readonly FileItem _fileItem;
        private readonly GraphProcedure _procedure;



        public GraphExecutionFinished(GraphProcedure procedure, IEntity[] data, FileItem fileItem = null)
        {
            _procedure = procedure;
            _data = data;
            _fileItem = fileItem;
        }



        public IEntity[] Data
        {
            get { return _data; }
        }



        public GraphProcedure Procedure
        {
            get { return _procedure; }
        }



        public FileItem FileItem
        {
            get { return _fileItem; }
        }
    }
}