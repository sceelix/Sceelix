using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Geometry;

namespace Sceelix.Designer.Renderer3D.Messages
{
    public class GeometricObjectClick
    {
        private readonly IGeometricObject _geometricObject;



        public GeometricObjectClick(IGeometricObject geometricObject)
        {
            _geometricObject = geometricObject;
        }



        public IGeometricObject GeometricObject
        {
            get { return _geometricObject; }
        }
    }
}
