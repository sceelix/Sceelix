using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Loading;

namespace Sceelix.Core.Data
{
    internal class EntityManager
    {
        public static List<Type> Types
        {
            get;
        } = new List<Type>();



        internal static void Initialize()
        {
            Types.AddRange(SceelixDomain.Types.Where(type => typeof(IEntity).IsAssignableFrom(type)));
        }
    }
}