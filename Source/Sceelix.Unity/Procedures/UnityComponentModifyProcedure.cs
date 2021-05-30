using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Unity.Data;

namespace Sceelix.Unity.Procedures
{
    public abstract class ComponentModifyParameter : CompoundParameter
    {
        protected ComponentModifyParameter(string label)
            : base(label)
        {
        }



        public abstract void Run(List<UnityEntity> unityEntities);
    }

    public class AddSurfaceTreeModifyParameter : ComponentModifyParameter
    {
        /// <summary>
        /// Actors whose position will represent tree positions on the terrain component
        /// </summary>
        private readonly SingleOrCollectiveInputChoiceParameter<IActor> _inputActor = new SingleOrCollectiveInputChoiceParameter<IActor>("Actor", "Single");

        /// <summary>
        /// Tree prefab to be used.
        /// </summary>
        private readonly StringParameter _parameterPrefab = new StringParameter("Prefab", "") {EntityEvaluation = true};

        /// <summary>
        /// Bend factor of the tree prefab, as understood by Unity.
        /// </summary>
        private readonly FloatParameter _parameterBendFactor = new FloatParameter("Bend Factor", 0) {EntityEvaluation = true};

        /// <summary>
        /// Rotation of the tree prefab.
        /// </summary>
        private readonly FloatParameter _parameterRotation = new FloatParameter("Rotation", 0) {EntityEvaluation = true};

        /// <summary>
        /// Scale of the tree prefab.
        /// </summary>
        private readonly Vector2DParameter _parameterScale = new Vector2DParameter("Scale", Vector2D.One) {EntityEvaluation = true};



        protected AddSurfaceTreeModifyParameter()
            : base("Add Surface Tree")
        {
        }



        public override void Run(List<UnityEntity> unityEntities)
        {
            var treeActors = _inputActor.Read().ToList();

            foreach (var unityEntity in unityEntities)
            {
                var component = unityEntity.GameComponents.FirstOrDefault(x => x is SurfaceComponent) as SurfaceComponent;
                if (component == null)
                    throw new InvalidOperationException("Surface component must be added first to the Unity Entity!");


                var surfaceEntityBoundingRectangle = unityEntity.BoxScope.BoundingRectangle;
                foreach (var treeActor in treeActors)
                    if (treeActor.BoxScope.BoundingRectangle.Intersects(surfaceEntityBoundingRectangle))
                    {
                        var scope = new BoxScope(treeActor.BoxScope, translation: treeActor.BoxScope.Translation - unityEntity.BoxScope.Translation);

                        component.TreeInstances.Add(new TreeInstance(scope, _parameterPrefab.Get(treeActor))
                        {
                            BendFactor = _parameterBendFactor.Get(treeActor),
                            Rotation = _parameterRotation.Get(treeActor),
                            Scale = _parameterScale.Get(treeActor)
                        });
                    }
            }
        }
    }


    [Procedure("b5874617-2bc3-4da8-b9f5-d051ac46ee82", Label = "Unity Component Modify")]
    public class UnityComponentModifyProcedure : SystemProcedure
    {
        /// <summary>
        /// Unity Entity whose component is to be modified.
        /// </summary>
        private readonly CollectiveInput<UnityEntity> _input = new CollectiveInput<UnityEntity>("Input");

        /// <summary>
        /// Unity Entity whose component was modified.
        /// </summary>
        private readonly Output<UnityEntity> _output = new Output<UnityEntity>("Output");

        /// <summary>
        /// Component to be modified.
        /// </summary>
        private readonly SelectListParameter<ComponentModifyParameter> _parameterComponent = new SelectListParameter<ComponentModifyParameter>("Component");



        protected override void Run()
        {
            var unityEntities = _input.Read().ToList();

            _parameterComponent.SelectedItem.Run(unityEntities);

            _output.Write(unityEntities);
        }
    }
}