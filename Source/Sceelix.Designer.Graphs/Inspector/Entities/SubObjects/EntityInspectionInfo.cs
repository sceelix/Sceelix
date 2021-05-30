using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.Inspector.Entities.SubObjects
{
    public class EntityInspectionInfo : IInspectionInfo
    {
        //cache the propertyinfos, obtained by reflection
        private static readonly Dictionary<Type, List<KeyValuePair<SubEntityAttribute, PropertyInfo>>> _subEntityCache 
            = new Dictionary<Type, List<KeyValuePair<SubEntityAttribute, PropertyInfo>>>();

        private static readonly Dictionary<Type, List<KeyValuePair<EntityPropertyAttribute, PropertyInfo>>> _propertyCache
            = new Dictionary<Type, List<KeyValuePair<EntityPropertyAttribute, PropertyInfo>>>();

        private readonly IEntity _entity;



        public EntityInspectionInfo(IEntity entity)
        {
            _entity = entity;
        }



        public bool HasChildren
        {
            get { return _entity.Attributes.OfKeyType<AttributeKey>().Any() 
                         || SubEntities.Any() 
                         || Properties.Any();
            }
        }



        public IEnumerable<object> Children
        {
            get
            {
                foreach (var attributeEntry in _entity.Attributes.OfKeyType<AttributeKey>())
                {
                    yield return new KeyValueInspectionInfo(attributeEntry.Key.ToString(), attributeEntry.Value);
                }

                //only show the aggregator if there are any properties
                if (Properties.Any())
                {
                    yield return new PropertyGroupInspectionInfo(_entity, Properties);
                }

                foreach (KeyValuePair<SubEntityAttribute, PropertyInfo> keyValuePair in SubEntities)
                {
                    yield return new EntityGroupInspectionInfo(keyValuePair.Key, (IEnumerable)keyValuePair.Value.GetValue(_entity, null));
                }
            }
        }



        public bool IsInitiallyExpanded
        {
            get { return true; }
        }



        public string Description
        {
            get;
            private set;
        }



        public String Label
        {
            get {
                return Core.Data.Entity.GetDisplayName(_entity.GetType());
            }
        }



        private List<KeyValuePair<SubEntityAttribute, PropertyInfo>> SubEntities
        {
            get
            {
                var type = _entity.GetType();

                return _subEntityCache.GetOrCompute(type, () =>
                {
                    var properties = type.GetProperties();

                    var infos = new List<KeyValuePair<SubEntityAttribute, PropertyInfo>>();

                    foreach (var propertyInfo in properties.Where(x => typeof(IEnumerable).IsAssignableFrom(x.PropertyType)))
                    {
                        var attribute = propertyInfo.GetCustomAttribute<SubEntityAttribute>();
                        if (attribute != null)
                            infos.Add(new KeyValuePair<SubEntityAttribute, PropertyInfo>(attribute, propertyInfo));
                    }

                    return infos;
                });
            }
        }



        private List<KeyValuePair<EntityPropertyAttribute, PropertyInfo>> Properties
        {
            get
            {
                var type = _entity.GetType();

                return _propertyCache.GetOrCompute(type, () =>
                {
                    var infos = new List<KeyValuePair<EntityPropertyAttribute, PropertyInfo>>();

                    foreach (var propertyInfo in type.GetProperties())
                    {
                        var attribute = propertyInfo.GetCustomAttribute<EntityPropertyAttribute>();
                        if (attribute != null)
                        {
                            if (String.IsNullOrWhiteSpace(attribute.ReadableName))
                                attribute.ReadableName = propertyInfo.Name;

                            infos.Add(new KeyValuePair<EntityPropertyAttribute, PropertyInfo>(attribute, propertyInfo));
                        }
                    }

                    return infos;
                });
            }
        }



        public IEntity Entity
        {
            get { return _entity; }
        }



        protected bool Equals(EntityInspectionInfo other)
        {
            return Equals(_entity, other._entity);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityInspectionInfo) obj);
        }



        public override int GetHashCode()
        {
            return (_entity != null ? _entity.GetHashCode() : 0);
        }



        public static bool operator ==(EntityInspectionInfo left, EntityInspectionInfo right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(EntityInspectionInfo left, EntityInspectionInfo right)
        {
            return !Equals(left, right);
        }
    }
}
