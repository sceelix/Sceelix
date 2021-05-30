using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Extensions;

namespace Sceelix.Annotations
{
    /*class AssemblyReferenceComparison : IComparer<Assembly>
    {
        public static Dictionary<Assembly, HashSet<Assembly>> referencedAssemblies = new Dictionary<Assembly, HashSet<Assembly>>();
        
        public int Compare(Assembly x, Assembly y)
        {
            if (x == y) return 0;
            if (GetReferencesAssemblies(x).Contains(y)) return -1;
            if (GetReferencesAssemblies(y).Contains(x)) return 1;
            return 0;
        }


        private bool ContainsReference(Assembly main, Assembly sub)
        {
            HashSet<Assembly> references;
            if (!referencedAssemblies.TryGetValue(main, references))
                references.Add(main, new HashSet<Assembly>(main.GetReferencedAssemblies(),));
            
        }
    }*/

    public class AttributeReader
    {
        public AttributeReader(IEnumerable<Assembly> assemblies)
        {
            Assemblies = assemblies.Where(x => x.HasCustomAttribute<SceelixLibraryAttribute>()).ToList();

            if (!Assemblies.Any())
                Assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.HasCustomAttribute<SceelixLibraryAttribute>()).ToList();

            //_assemblies.Sort();
        }



        public List<Assembly> Assemblies
        {
            get;
        }



        public static SimpleAttributeReader<T> OfAttribute<T>(params Assembly[] assemblies) where T : Attribute
        {
            return new SimpleAttributeReader<T>(assemblies);
        }



        public static StringKeyAttributeReader<StringKeyAttribute> OfStringKeyAttribute(params Assembly[] assemblies)
        {
            return new StringKeyAttributeReader<StringKeyAttribute>(assemblies);
        }



        public static StringKeyAttributeReader<T> OfStringKeyAttribute<T>(params Assembly[] assemblies)
            where T : StringKeyAttribute
        {
            return new StringKeyAttributeReader<T>(assemblies);
        }



        public static TypeKeyAttributeReader<TypeKeyAttribute> OfTypeKeyAttribute(params Assembly[] assemblies)
        {
            return new TypeKeyAttributeReader<TypeKeyAttribute>(assemblies);
        }



        public static TypeKeyAttributeReader<T> OfTypeKeyAttribute<T>(params Assembly[] assemblies) where T : TypeKeyAttribute
        {
            return new TypeKeyAttributeReader<T>(assemblies);
        }
    }
}