using System;
using System.Collections.Generic;
using Sceelix.Core.Bindings;
using Sceelix.Core.Messages;
using Sceelix.Core.Resources;
using Sceelix.Logging;

namespace Sceelix.Core.Environments
{
    /// <summary>
    /// The Procedure Environment is an aggregation of several services that affect procedures
    /// at several points of their execution, providing diverse means for resource loading, messaging,
    /// debugging and others.
    /// </summary>
    public interface IProcedureEnvironment
    {
        /// <summary>
        /// Constitutes a way to listen and interact with the execution process at certain graph execution moments.
        /// </summary>
        [Obsolete("Replaced with GetService<IExecutionBinding>()")]
        IExecutionBinding ExecutionBinding
        {
            get;
        }

        /// <summary>
        /// Allows for logging of messages to the registered output.
        /// </summary>
        [Obsolete("Replaced with GetService<ILogger>()")]
        ILogger Logger
        {
            get;
        }


        /// <summary>
        /// Allows for any kind of communication to be performed from and with the nodes.
        /// </summary>
        [Obsolete("Replaced with GetService<IMessenger>()")]
        IMessenger Messenger
        {
            get;
        }

        /// <summary>
        /// Controls the access to resources, such as files, streams, variables, etc. This can go beyond a simple
        /// access to files, being applicable to assembly resources, zip packages and more.
        /// </summary>
        [Obsolete("Replaced with GetService<IResourceManager>()")]
        IResourceManager Resources
        {
            get;
        }

        T GetService<T>(bool defaultIfNull = true, bool throwIfNone = true) where T : class;

        IEnumerable<T> GetServices<T>() where T : class;
    }
}