using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;

namespace Sceelix.Core.IO
{
    public class CollectiveInput : Input
    {
        public CollectiveInput(string label, Type entityType)
            : base(label, entityType)
        {
        }



        public override InputNature InputNature => InputNature.Collective;



        protected override IEnumerable<IEntity> PrepareNextData()
        {
            IEnumerable<IEntity> objects = CurrentObjects.ToArray();

            CurrentObjects.Clear();

            return objects;
        }



        public override object Read()
        {
            return RoundEntities;
        }
    }


    public class CollectiveInput<T> : CollectiveInput where T : IEntity
    {
        public CollectiveInput(string label)
            : base(label, typeof(T))
        {
        }



        /// <summary>
        /// Reads all entities waiting at this input. This functions does not empty the input queue, so it can be called several times through the procedure, if needed.
        /// </summary>
        /// <returns>All entities waiting at this input</returns>
        public new IEnumerable<T> Read()
        {
            return RoundEntities.Cast<T>();
        }
    }
}