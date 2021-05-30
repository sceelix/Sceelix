using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Meshes.SceneNodes;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;

namespace Sceelix.Designer.Meshes.Translators
{
    [EntityTranslator(typeof(MeshInstanceEntity))]
    public class MeshInstanceTranslator : EntityTranslator<MeshInstanceEntity>, IServiceable
    {
        private MeshRenderNodeFactory _meshRenderNodeFactory;


        public void Initialize(IServiceLocator services)
        {
            _meshRenderNodeFactory = services.Get<MeshRenderNodeFactory>();
        }



        public override void Process(List<MeshInstanceEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            foreach (var grouping in entities.GroupBy(x => x.MeshEntity))
            {
                //we need to define an InvertedZUpRotation Pose here because internally the positions are being rotated and later the rotated PoseWorld is being set per mesh
                //MeshRenderNode meshRenderNode = new MeshRenderNode(new [] {grouping.Key}, _materialTranslators, entityObjectDomain.ContentLoader) { PoseWorld = new Pose(DigitalRuneUtils.InvertedZUpRotation) };

                MeshRenderNode meshRenderNode = _meshRenderNodeFactory.Create(grouping.Key, entityObjectDomain.ContentLoader, new Pose(DigitalRuneUtils.YUpToZUpRotationMatrix));
                
                CreateMeshInstance(grouping, meshRenderNode, entityObjectDomain);
            }
        }



        private void CreateMeshInstance(IGrouping<MeshEntity, MeshInstanceEntity> grouping, MeshRenderNode meshRenderNode, EntityObjectDomain entityObjectDomain)
        {
            Mesh mesh = MeshHelper.Merge(grouping.Select(shapeInstance =>
            {
                var newShapeRenderNode = (MeshRenderNode) meshRenderNode.Clone();

                newShapeRenderNode.PoseWorld = shapeInstance.BoxScope.ToPoseWithoutScale() * newShapeRenderNode.PoseWorld;
                newShapeRenderNode.ScaleLocal = shapeInstance.Scale.ToVector3F(false);//new Vector3F(shapeInstance.BoxScope.Sizes.X*shapeInstance.Scale.X, shapeInstance.BoxScope.Sizes.Y*shapeInstance.Scale.Y, shapeInstance.BoxScope.Sizes.Z*shapeInstance.Scale.Z); //shapeInstance.Scale.X, shapeInstance.Scale.Y, shapeInstance.Scale.Z

                return newShapeRenderNode;
            }));


            EntityObject componentObject = new EntityObject(entityObjectDomain);
            componentObject.AddComponent(new SceneComponent(new MeshNode(mesh)));
            entityObjectDomain.ComponentObjects.Add(componentObject);
        }



        /// <summary>
        /// Because Monogame does not support hardware instancing, we just merge all the mesh nodes.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="grouping"></param>
        /// <param name="meshRenderNode"></param>
        /*private void MergeMeshes(IScene scene, IGrouping<MeshEntity, MeshInstanceEntity> grouping, MeshRenderNode meshRenderNode)
        {
            List<Pose> poses = new List<Pose>();
            List<Vector3F> scales = new List<Vector3F>();
            foreach (MeshInstanceEntity shapeInstance in grouping)
            {
                poses.Add(shapeInstance.BoxScope.ToPoseWithoutScale());
                scales.Add(new Vector3F(shapeInstance.BoxScope.Sizes.X, shapeInstance.BoxScope.Sizes.Y, shapeInstance.BoxScope.Sizes.Z));
            }

            var meshNodes = meshRenderNode.Children.OfType<MeshNode>().Select(x => x.Mesh);
            foreach (Mesh mesh1 in meshNodes)
            {
                Mesh mesh = MeshHelper.Merge((Mesh) mesh1.Clone(), scales.ToArray(), poses.ToArray());
                _sceneManager.Add(new MeshNode(mesh) {UserData = this});
            }
        }*/
    }
}