using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Spatial;

namespace Sceelix.Actors.Data
{
    [Entity("Actor Group")]
    public class ActorGroup : Entity, IActor, IEntityGroup
    {
        private BoxScope _boxScope;



        public ActorGroup(IEnumerable<IActor> actors)
        {
            Actors = actors.ToList();

            /*List<BoundingBox> bboxes = new List<BoundingBox>();
            foreach (var actor in _actors)
            {
                var bbox = actor.BoxScope.GetBoundingBox();

                if (bbox.Min.IsNaN)
                    Console.WriteLine();

                if (bbox.Max.IsNaN)
                    Console.WriteLine();

                bboxes.Add(bbox);
            }*/
            var boundingBoxes = Actors.Select(x => x.BoxScope.BoundingBox).ToList();
            var mergedBox = BoundingBox.Union(boundingBoxes);

            //ideally, we should pick the first scope and enlarge it
            _boxScope = new BoxScope(mergedBox);
        }



        [SubEntity("Actors")]
        public List<IActor> Actors
        {
            get;
            private set;
        }



        public BoxScope BoxScope
        {
            get { return _boxScope; }
            set
            {
                _boxScope = value;
                AdjustScope();

                /*foreach (var actor in _actors)
                {
                    var relative = BoxScope.ToRelativeScope(actor.BoxScope);

                    var relativeTarget = value.FromRelativeScope(relative);

                    //actor.InsertInto(relativeTarget);
                    actor.BoxScope = relativeTarget;
                }

                _boxScope = value;*/
            }
        }



        public IEnumerable<IEntity> SubEntities => Actors;



        public override IEnumerable<IEntity> SubEntityTree
        {
            get { return Actors.SelectMany(x => new[] {x}.Concat(x.SubEntityTree)); }
        }



        private void AdjustScope()
        {
            //_boxScope = _boxScope.Adjust(BoundingBox.Merge(_actors.Select(x => x.BoxScope.BoundingBox).ToList()).Corners);
            //_boxScope = _boxScope.Adjust(_actors.SelectMany(x => x.BoxScope.CornerPositions));

            var boundingBoxes = Actors.Select(x => x.BoxScope.BoundingBox).ToList();
            var mergedBox = BoundingBox.Union(boundingBoxes);

            //ideally, we should pick the first scope and enlarge it
            _boxScope = new BoxScope(mergedBox);
        }



        public override IEntity DeepClone()
        {
            var clone = (ActorGroup) base.DeepClone();
            clone.Actors = Actors.Select(x => (IActor) x.DeepClone()).ToList();
            clone._boxScope = _boxScope;

            return clone;
        }



        /*public void Translate(Vector3D direction, bool scopeRelative)
        {
            throw new NotImplementedException();
        }

        public void Scale(Vector3D scaling, Vector3D pivot, bool scopeRelative)
        {
            throw new NotImplementedException();
        }*/



        public void InsertInto(BoxScope target)
        {
            foreach (var actor in Actors)
            {
                /*var relative = BoxScope.ToRelativeScope(actor.BoxScope);

                var relativeTarget = target.FromRelativeScope(relative);    */

                var mainPoints = BoxScope.ToRelativeMainPoints(actor.BoxScope);
                var relativeTarget = target.FromRelativeMainPoints(mainPoints);
                actor.InsertInto(relativeTarget);
            }

            _boxScope = target;
        }



        public void MergeAttributes()
        {
            for (int i = 0; i < Actors.Count; i++)
                if (i == 0)
                    Actors[i].Attributes.SetAttributesTo(Attributes);
                else
                    Attributes.IntersectAttributes(Actors[i].Attributes);
        }
    }
}