using System;
using System.Collections.Generic;
using Sceelix.Collections;

namespace Sceelix.Core.Data
{
    public class GraphTrail : ICloneable, IMergeable
    {
        private GraphTrail()
        {
        }



        public GraphTrail(object startingObject)
        {
            Objects = new HashSet<object> {startingObject};
        }



        public HashSet<object> Objects
        {
            get;
            private set;
        }



        public object Clone()
        {
            //do not clone the nodes, just the list
            return new GraphTrail {Objects = new HashSet<object>(Objects)};
        }



        public object MergeWith(object other)
        {
            if (other is GraphTrail)
                foreach (object otherObject in ((GraphTrail) other).Objects)
                    Objects.Add(otherObject);

            return this;
        }
    }
}