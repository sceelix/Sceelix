using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.Graphs.GUI.Execution
{
    public class ExecutionOptions
    {
        private bool _debug = false;
        private bool _depthFirst;
        public Object _limit = null;
        private bool _strictLimit;



        public ExecutionOptions()
        {
        }



        public ExecutionOptions(bool debug)
        {
            _debug = debug;
        }



        public ExecutionOptions(bool debug, object limit, bool strictLimit, bool depthFirst)
        {
            _debug = debug;
            this._limit = limit;
            _strictLimit = strictLimit;
            _depthFirst = depthFirst;
        }



        public bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }



        /// <summary>
        /// The element (node, edge or port) that defines the limit of execution of the graph.
        /// Nodes coming after this element shall not be processed and executed.
        /// </summary>
        public object Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }



        /// <summary>
        /// If enabled, only the output of the selected limit should be shown.
        /// Otherwise, all the outputs until the selected limit should be shown.
        /// </summary>
        public bool StrictLimit
        {
            get { return _strictLimit; }
            set { _strictLimit = value; }
        }



        public bool DepthFirst
        {
            get { return _depthFirst; }
            set { _depthFirst = value; }
        }
    }
}