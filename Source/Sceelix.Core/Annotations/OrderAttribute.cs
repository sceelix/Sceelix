using System;

namespace Sceelix.Core.Annotations
{
    public class OrderAttribute : Attribute
    {
        public OrderAttribute(int orderIndex)
        {
            OrderIndex = orderIndex;
        }



        public int OrderIndex
        {
            get;
        }
    }
}