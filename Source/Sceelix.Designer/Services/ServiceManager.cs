using System;
using System.Collections.Generic;
using System.Linq;

namespace Sceelix.Designer.Services
{
    internal class ServiceRegistry
    {
        internal Type RegisteredType
        {
            get;
            set;
        }


        internal String RegisteredName
        {
            get;
            set;
        }


        internal Object Service
        {
            get;
            set;
        }
    }

    public class ServiceManager : IServiceLocator
    {
        private readonly List<ServiceRegistry> _serviceRegistries = new List<ServiceRegistry>();
        private readonly IServiceLocator _parentLocator;


        public ServiceManager()
        {
        }


        public ServiceManager(IServiceLocator parentLocator)
        {
            _parentLocator = parentLocator;
        }


        /// <summary>
        /// Gets the service object of the specified type. Implementation of System.IServiceProvider.GetService(type). Same as Get(type).
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.
        /// </returns>
        public object GetService(Type serviceType)
        {
            return Get(serviceType);
        }


        public object TryGetService(Type serviceType)
        {
            return TryGet(serviceType);
        }


        public Object TryGet(Type serviceType, String name = null)
        {
            var obj = _serviceRegistries.FirstOrDefault(val => val.RegisteredType == serviceType && val.RegisteredName == name);
            if (obj != null)
                return obj.Service;

            if (_parentLocator != null)
                return _parentLocator.TryGetService(serviceType);

            return null;
        }


        public Object Get(Type serviceType, String name = null)
        {
            var obj = TryGet(serviceType, name);
            if (obj == null)
                throw new ArgumentException("Service '" + serviceType.Name + "'" + (name != null ? " with name '" + name + "'" : "") + " is not registered in the container.");

            return obj;
        }


        public T Get<T>(String name = null)
        {
            return (T) Get(typeof(T), name);
        }


        public T TryGet<T>(String name = null)
        {
            return (T) TryGet(typeof(T), name);
        }


        public void Register(Type type, object service, string name = null)
        {
            _serviceRegistries.Add(new ServiceRegistry {RegisteredName = name, RegisteredType = type, Service = service});
        }


        public void Register<T>(T service, string name = null)
        {
            _serviceRegistries.Add(new ServiceRegistry {RegisteredName = name, RegisteredType = typeof(T), Service = service});
        }
    }
}