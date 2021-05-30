using System;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.Messages
{
    public class ProcedureExecutionFinished
    {
        private readonly String _finishMessage;
        private readonly FileItem _item;



        public ProcedureExecutionFinished(FileItem item, string finishMessage)
        {
            _item = item;
            _finishMessage = finishMessage;
        }



        public string FinishMessage
        {
            get { return _finishMessage; }
        }



        public FileItem Item
        {
            get { return _item; }
        }
    }
}