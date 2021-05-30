using System;

namespace Sceelix.Resolution
{
    public interface ITypeResolver
    {
        Type Resolve(string typeName);
    }
}