using System;
using System.Linq;
using Sceelix.Extensions;
using Sceelix.Resolution;

namespace Sceelix.Mathematics.Resolution
{
    /// <summary>
    /// Resolves parameter types after the refactoring at version 0.8.4.1.
    /// </summary>
    [TypeResolver]
    public class ParameterResolver : ITypeResolver
    {
        public Type Resolve(string typeName)
        {
            if (typeName.StartsWith("Sceelix.Geometry3D.Parameters"))
            {
                var name = typeName.SplitInTwo(',')[0].Split('.').Last();

                return GetType().Assembly.GetTypes().FirstOrDefault(x => x.Name == name);
            }

            return null;
        }
    }
}