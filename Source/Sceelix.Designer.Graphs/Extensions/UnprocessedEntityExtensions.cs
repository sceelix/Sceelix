using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.Extensions;

namespace Sceelix.Designer.Graphs.Extensions
{
    public static class UnprocessedEntityExtensions
    {
        public static AttributeKey unprocessedEntityKey = new GlobalAttributeKey("Unprocessed");

        public static void SetIsUnprocessedEntity(this IEntity entity, bool value)
        {
            if (value)
            {
                entity.SetAttribute(unprocessedEntityKey, true);
            }
        }

        public static bool GetIsUnprocessedEntity(this IEntity entity)
        {
            var isUnprocessed= entity.GetAttribute(unprocessedEntityKey);

            return (bool?) isUnprocessed ?? false;
        }
    }
}
