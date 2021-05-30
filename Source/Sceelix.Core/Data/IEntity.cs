using System.Collections.Generic;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;

namespace Sceelix.Core.Data
{
    [Entity("Entity")]
    public interface IEntity
    {
        AttributeCollection Attributes
        {
            get;
        }


        IEnumerable<IEntity> SubEntityTree
        {
            get;
        }


        IEntity DeepClone();

        //Object this[String attributeName] { get; set; }

        //Object GetGlobalAttribute(String attributeName);
    }
}