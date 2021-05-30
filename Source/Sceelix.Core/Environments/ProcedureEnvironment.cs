using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sceelix.Core.Bindings;
using Sceelix.Core.Messages;
using Sceelix.Core.Resources;
using Sceelix.Extensions;
using Sceelix.Logging;

namespace Sceelix.Core.Environments
{
    /// <summary>
    /// The default implementation of the IProcedureEnvironment interface
    /// which simply assumes some services by default, yet allowing others
    /// to be assigned.
    /// </summary>
    /// <seealso cref="Sceelix.Core.Environments.IProcedureEnvironment" />
    public class ProcedureEnvironment : IProcedureEnvironment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureEnvironment"/> class.
        /// </summary>
        public ProcedureEnvironment(params object[] services)
        {
            Services.AddRange(services);
        }



        public ProcedureEnvironment(IEnumerable<object> services)
        {
            Services.AddRange(services);
        }



        /// <summary>
        /// Gets or sets the execution binding used to listen and interact with the execution process
        /// </summary>
        public IExecutionBinding ExecutionBinding => GetService<IExecutionBinding>();


        /// <summary>
        /// Gets or sets the logger for logging purposes.
        /// </summary>
        /// <value> The logger. </value>
        public ILogger Logger => GetService<ILogger>();


        /// <summary>
        /// Gets or sets the messaging system.
        /// </summary>
        public IMessenger Messenger => GetService<IMessenger>();


        /// <summary>
        /// Gets or sets the Resource Manager used for accessing resources.
        /// </summary>
        /// <value> The resource manager. </value>
        public IResourceManager Resources => GetService<IResourceManager>();


        public List<object> Services
        {
            get;
        } = new List<object>();



        public void AddService(object service)
        {
            Services.Add(service);
        }



        /// <summary>
        /// Copy constructor of the procedureEnvironment.
        /// Does not clone the services, just the list of them.
        /// </summary>
        /// <param name="procedureEnvironment"></param>
        public static ProcedureEnvironment FromExisting(IProcedureEnvironment procedureEnvironment)
        {
            //copies the list, does not clone the services.
            var environment = new ProcedureEnvironment();
            environment.Services.AddRange(procedureEnvironment.GetServices<object>());
            return environment;
        }



        public T GetService<T>(bool defaultIfNull = true, bool throwIfNone = true) where T : class
        {
            object value = Services.FirstOrDefault(x => x is T);
            if (value == null)
            {
                if (defaultIfNull)
                {
                    var defaultValueAttribute = typeof(T).GetCustomAttribute<DefaultValueAttribute>();
                    if (defaultValueAttribute != null)
                    {
                        var type = defaultValueAttribute.Value as Type;
                        if (type == null)
                            throw new InvalidCastException("Default value " + defaultValueAttribute.Value + " is not a type.");

                        var instance = Activator.CreateInstance(type);
                        if (!(instance is T))
                            throw new InvalidCastException("Default value type " + type + " is not a subtype of " + typeof(T) + ".");

                        Services.Add(value = instance);

                        return (T) value;
                    }
                }


                if (throwIfNone)
                    throw new KeyNotFoundException("Could not find service of type " + typeof(T));

                return null;
            }

            return (T) value;
        }



        public IEnumerable<T> GetServices<T>() where T : class
        {
            return Services.OfType<T>();
        }



        public void RemoveServices<T>()
        {
            Services.RemoveAll(x => x is T);
        }
    }
}