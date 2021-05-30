using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Annotations;

namespace Sceelix.Resolution
{
    /// <summary>
    /// Performs type resolution, i.e. finds a way to transform a type name (as a string)
    /// into a valid type (which may have been changed for any reason). Uses resolvers
    /// that may have been defined throughout the assemblies.
    /// </summary>
    public class TypeResolverManager
    {
        private static readonly List<ITypeResolver> TypeResolvers = AttributeReader.OfAttribute<TypeResolverAttribute>().GetInstancesOfType<ITypeResolver>();



        /// <summary>
        /// Iterates over defined TypeResolvers until it finds
        /// a way to resolve the given type name into a valid type.
        /// </summary>
        /// <param name="typeName">Type name to search for.</param>
        /// <returns>The resolved type, if found, or null otherwise.</returns>
        public static Type Resolve(string typeName)
        {
            return TypeResolvers.Select(typeResolver => typeResolver.Resolve(typeName)).FirstOrDefault(type => type != null);
        }
    }
}