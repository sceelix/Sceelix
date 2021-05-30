using System;

namespace Sceelix.Designer.Services
{
    public interface IServiceLocator: IServiceProvider
    {
        Object TryGetService(Type serviceType);

        T Get<T>(String name = null);

        T TryGet<T>(String name = null);
    }
}