using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sceelix.Conversion;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Loading;

namespace Sceelix.Designer.Graphs.Inspector.Entities.SubObjects
{
    public class PropertyGroupInspectionInfo : IInspectionInfo
    {
        private readonly IEntity _entity;
        private readonly List<KeyValuePair<EntityPropertyAttribute, PropertyInfo>> _properties;



        public PropertyGroupInspectionInfo(IEntity entity, List<KeyValuePair<EntityPropertyAttribute, PropertyInfo>> properties)
        {
            _entity = entity;
            _properties = properties;
        }



        public bool HasChildren
        {
            get { return _properties.Count >= 0; }
        }

        public string Label
        {
            get { return "Properties (" + _properties.Count + ")"; }
        }


        public IEnumerable<object> Children
        {
            get
            {
                return _properties.Select(x =>
                {
                    var value = x.Value.GetValue(_entity, null);
                    
                    if (x.Key.HandleType != null)
                        value = ConvertHelper.Convert(value, x.Key.HandleType);

                    var description = CommentLoader.GetComment(x.Value).Summary;

                    return new KeyValueInspectionInfo(x.Key.ReadableName, value, description);
                });
            }
        }

        public bool IsInitiallyExpanded
        {
            get { return false; }
        }



        public string Description
        {
            get { return "Set of native/calculated properties of the entity."; }
        }
    }
}
