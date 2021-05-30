using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;

namespace Sceelix.Core.IO
{
    public class SingleInput : Input
    {
        public SingleInput(string label, Type entityType)
            : base(label, entityType)
        {
        }



        public override InputNature InputNature => InputNature.Single;



        protected override IEnumerable<IEntity> PrepareNextData()
        {
            if (CurrentObjects.Count > 0)
                yield return CurrentObjects.Dequeue();
        }



        public override object Read()
        {
            return RoundEntities.FirstOrDefault();
        }
    }


    public class SingleInput<T> : SingleInput where T : IEntity
    {
        public SingleInput(string label)
            : base(label, typeof(T))
        {
        }



        /// <summary>
        /// Reads the entity waiting at this input to be processed in this round. This functions does not empty the input queue, so it can be called several times through the procedure, if needed.
        /// </summary>
        /// <returns>The entity waiting at this input to be processed in this round</returns>
        public new T Read()
        {
            if (RoundEntities.Any())
                return (T) RoundEntities.FirstOrDefault();

            return default(T);
        }
    }
}