using System;
using System.Collections.Generic;
using Sceelix.Core.Graphs;

namespace Sceelix.Core.Exceptions
{
    public class GraphException : Exception
    {
        public GraphException(Node throwingNode, Exception innerException) :
            base(innerException.Message, innerException)
        {
            ThrowingNode = throwingNode;
        }



        /// <summary>
        /// Gets the node that threw this exception.
        /// </summary>
        public Node ThrowingNode
        {
            get;
        }



        /// <summary>
        /// Gets the nodes that have thrown the exceptions, going deep
        /// in possible inner exceptions.
        /// </summary>
        public IEnumerable<Node> ThrowingNodes
        {
            get
            {
                GraphException currentException = this;

                do
                {
                    yield return currentException.ThrowingNode;
                } while ((currentException = currentException.InnerException as GraphException) != null);
            }
        }
    }
}