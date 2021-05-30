using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;

namespace Sceelix.Core.Data
{
    public class EntityGroup : Entity, IEntityGroup
    {
        public EntityGroup()
        {
        }



        public EntityGroup(params IEntity[] entities)
        {
            Entities = new List<IEntity>(entities);
        }



        public EntityGroup(IEnumerable<IEntity> entities)
        {
            Entities = new List<IEntity>(entities);
        }



        public List<IEntity> Entities
        {
            get;
        } = new List<IEntity>();


        public string Name
        {
            get;
            set;
        }


        [SubEntity("Entities")]
        public IEnumerable<IEntity> SubEntities => Entities;



        public override IEnumerable<IEntity> SubEntityTree
        {
            get { return Entities.SelectMany(x => new[] {x}.Concat(x.SubEntityTree)); }
        }



        public override IEntity DeepClone()
        {
            return new EntityGroup(Entities.Select(val => val.DeepClone())) {Name = Name};
        }



        public void MergeAttributes()
        {
            for (int i = 0; i < Entities.Count; i++)
                if (i == 0)
                    Entities[i].Attributes.SetAttributesTo(Attributes);
                else
                    Attributes.IntersectAttributes(Entities[i].Attributes);
        }



        /*public IEnumerable<IEntity> SubEntitityTree
        {
            get 
            {
                foreach (IEntity entity in _entities)
                {
                    var group = entity as IEntityGroup;
                    if (group != null)
                    {
                        foreach (var subEntity in group.SubEntitityTree)
                            yield return subEntity;
                    }
                }
            }
        }*/
    }
}