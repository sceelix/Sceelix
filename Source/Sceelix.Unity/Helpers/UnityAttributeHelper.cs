using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Unity.Attributes;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Helpers
{
    public static class UnityAttributeHelper
    {
        public static AttributeKey ComponentsAttributeKey = new GlobalAttributeKey("Components", new UnityMeta());



        public static UnityComponent AddComponent(this IEntity entity, UnityComponent component)
        {
            var components = entity.Attributes.TryGet(ComponentsAttributeKey) as List<UnityComponent>;
            if (components == null)
                entity.Attributes[ComponentsAttributeKey] = components = new List<UnityComponent>();

            components.Add(component);

            return component;
        }



        public static IEnumerable<UnityComponent> GetComponents(this IEntity entity)
        {
            var components = entity.Attributes.TryGet(ComponentsAttributeKey) as List<UnityComponent>;
            if (components != null)
                return components;

            return Enumerable.Empty<UnityComponent>();
        }
    }
}