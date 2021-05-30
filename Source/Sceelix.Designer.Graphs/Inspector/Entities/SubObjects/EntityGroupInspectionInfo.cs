using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;

namespace Sceelix.Designer.Graphs.Inspector.Entities.SubObjects
{
    public class EntityGroupInspectionInfo : IInspectionInfo
    {
        private readonly SubEntityAttribute _key;
        private readonly List<EntityInspectionInfo> _subEntities;



        public EntityGroupInspectionInfo(SubEntityAttribute key, IEnumerable subEntities)
        {
            _key = key;
            _subEntities = subEntities.Cast<IEntity>().Select(x => new EntityInspectionInfo(x)).ToList();
        }



        public bool HasChildren
        {
            get { return _subEntities.Any(); }
        }


        public String Label
        {
            get { return _key.Label + " (" + _subEntities.Count + ")"; }
        }



        public IEnumerable<object> Children
        {
            get
            {
                return _subEntities;
            }
        }

        public bool IsInitiallyExpanded
        {
            get { return false; }
        }


        public string Description
        {
            get { return _key.Label + " contained within the entity."; }
        }
    }
}
