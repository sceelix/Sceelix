using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Annotations;
using Sceelix.Designer.Actors.Components;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Meshes.Components;
using Sceelix.Designer.Meshes.SceneNodes;
using Sceelix.Designer.Meshes.Utils;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Sceelix.Meshes.Data;

namespace Sceelix.Designer.Meshes.Translators
{
    [EntityTranslator(typeof(MeshEntity))]
    public class MeshTranslator : EntityTranslator<MeshEntity>, IServiceable
    {
        private MeshRenderNodeFactory _meshRenderNodeFactory;
        private RenderTargetControl _renderTargetControl;
        private IScene _scene;
        private Renderer3DSettings _render3DSettings;



        public void Initialize(IServiceLocator services)
        {
            if (BuildPlatform.IsWindows)
            {
                _render3DSettings = services.Get<SettingsManager>().Get<Renderer3DSettings>();
                _render3DSettings.ShowFaceEdges.Changed += ShowFaceEdgesOnChanged;
            }

            _meshRenderNodeFactory = services.Get<MeshRenderNodeFactory>();
            _scene = services.Get<IScene>(); 
            _renderTargetControl = services.Get<RenderTargetControl>();
        }



        private void ShowFaceEdgesOnChanged(ApplicationField<bool> field, bool oldValue, bool newValue)
        {
            _scene.Children.SelectMany(x => x.GetDescendants()).OfType<MeshRenderNode>().ForEach(x => x.EdgesVisible = newValue);

            //refresh the view
            _renderTargetControl.ShouldRender = true;
        }

        

        public override void Process(List<MeshEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            //first, create the object that will contain all the meshes
            EntityObject renderMeshGameObject = new EntityObject(entityObjectDomain);

            var meshRenderNode = _meshRenderNodeFactory.Create(entities, entityObjectDomain.ContentLoader);


            if(BuildPlatform.IsWindows)
                meshRenderNode.EdgesVisible = _render3DSettings.ShowFaceEdges.Value;
            else
                renderMeshGameObject.AddComponent(new MeshWireFrameRenderComponent(entities));
            

            renderMeshGameObject.AddComponent(new SceneComponent(meshRenderNode));
            entityObjectDomain.ComponentObjects.Add(renderMeshGameObject);


            //now, for over all the entities themselves to add collisions and such
            foreach (MeshEntity entity in entities)
            {
                EntityObject componentObject = new EntityObject(entityObjectDomain);
                componentObject.AddComponent(new CollisionComponent(MeshTranslationHelper.CalculateTriangleMesh(entity)));
                componentObject.AddComponent(new SelectableEntityComponent(entity));
                componentObject.AddComponent(new ScopeHighlightComponent(entity.BoxScope));
                componentObject.AddComponent(new CollisionHighlightComponent());
                componentObject.AddComponent(new MeshDrawComponent(entity));

                entityObjectDomain.ComponentObjects.Add(componentObject);
            }
        }

        


        /// <summary>
        /// We may to need this code is the future for collision tests, so keep this
        /// </summary>
        /// <param name="sceelixMeshEntity"></param>
        private void CollisionViewing(MeshEntity sceelixMeshEntity)
        {
            var rectangleShape = new BoxShape(sceelixMeshEntity.BoxScope.Sizes.ToVector3F());
            var geo = new GeometricObject(rectangleShape, sceelixMeshEntity.BoxScope.ToPoseWithoutScale());
            geo.Pose = new Pose(geo.Pose.Position + geo.Pose.ToWorldDirection(new Vector3F(rectangleShape.WidthX/2f, rectangleShape.WidthY/2f, rectangleShape.WidthZ/2f)), geo.Pose.Orientation);
        }
    }
}