using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Sceelix.Annotations;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Meshes.SceneNodes;
using Sceelix.Designer.Meshes.Utils;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Services;
using Sceelix.Designer.Unity3D.GameObjects.Components;
using Sceelix.Designer.Utils;
using Sceelix.Unity.Data;

namespace Sceelix.Designer.Unity3D.Translators
{
    [TypeKey(typeof(MeshComponent))]
    public class MeshComponentTranslator : ComponentTranslator, IServiceable
    {
        private MeshRenderNodeFactory _meshRenderNodeFactory;


        public void Initialize(IServiceLocator services)
        {
            _meshRenderNodeFactory = services.Get<MeshRenderNodeFactory>();
        }


        public override IEnumerable<EntityObjectComponent> Process(UnityEntity unityEntity, UnityComponent unityComponent, EntityObjectDomain entityObjectDomain)
        {
            MeshComponent meshComponent = (MeshComponent) unityComponent;

            var pose = unityEntity.BoxScope.ToPoseWithoutScale() * new Pose(DigitalRuneUtils.YUpToZUpRotationMatrix);
            var scale = unityEntity.Scale.ToVector3F(false);


            var meshRenderNode = _meshRenderNodeFactory.Create(meshComponent.MeshEntity, entityObjectDomain.ContentLoader, pose);
            meshRenderNode.ScaleLocal = scale;

            yield return new CollisionComponent(MeshTranslationHelper.CalculateTriangleMesh(meshComponent.MeshEntity), pose, scale);

            yield return unityEntity.Static ? (EntityObjectComponent) new StaticSceneComponent(meshRenderNode) : new DynamicSceneComponent(meshRenderNode);
        }


        public override void PostProcess(IEnumerable<EntityObject> entityObjects, EntityObjectDomain entityObjectDomain)
        {
            var sceneNodeList = entityObjects.SelectMany(x => x.Components).OfType<StaticSceneComponent>().Select(x => x.SceneNode).ToList();
            if (sceneNodeList.Count > 0)
            {
                var mesh = MeshHelper.Merge(sceneNodeList);

                EntityObject mergeEntityObject = new EntityObject(entityObjectDomain);
                mergeEntityObject.AddComponent(new SceneComponent(new MeshNode(mesh)));
            }
        }


        public override bool IsDrawable
        {
            get { return true; }
        }
    }
}