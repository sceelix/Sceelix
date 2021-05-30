using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;

namespace Sceelix.Points.Data
{
    [Entity("Point", TypeBrowsable = true)]
    public class PointEntity : Entity, IActor
    {
        private BoxScope _boxScope;



        /// <summary>
        /// Initializes a new instance of <see cref="PointEntity"/> at the given location, oriented to the world.
        /// </summary>
        /// <param name="position">The position.</param>
        public PointEntity(Vector3D position)
        {
            _boxScope = new BoxScope(translation: position, sizes: new Vector3D(0, 0, 0));
        }



        public PointEntity(BoxScope scope)
        {
            _boxScope = new BoxScope(scope, sizes: new Vector3D(0, 0, 0));
        }



        public BoxScope BoxScope
        {
            get { return _boxScope; }
            set { _boxScope = value; }
        }



        /// <summary>
        /// The position of the pointEntity. Same as the boxscope.translation.
        /// </summary>
        public Vector3D Position => _boxScope.Translation;



        public void InsertInto(BoxScope target)
        {
            _boxScope = target;
        }
    }
}