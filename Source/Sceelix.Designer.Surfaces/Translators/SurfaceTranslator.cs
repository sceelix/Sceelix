using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Sceelix.Annotations;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Actors.Components;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Translators;
using Sceelix.Designer.Services;
using Sceelix.Designer.Surfaces.Translators.Materials;
using Sceelix.Designer.Surfaces.Utils;
using Sceelix.Extensions;
using Sceelix.Surfaces.Data;

namespace Sceelix.Designer.Surfaces.Translators
{
    [EntityTranslator(typeof(SurfaceEntity))]
    public class SurfaceTranslator : EntityTranslator<SurfaceEntity>, IServiceable
    {
        private readonly Dictionary<Type, ISurfaceMaterialTranslator> _surfaceMaterialTranslators = AttributeReader.OfTypeKeyAttribute<MaterialTranslatorAttribute>().GetInstancesOfType<ISurfaceMaterialTranslator>();


        public void Initialize(IServiceLocator services)
        {
            _surfaceMaterialTranslators.Values.ForEach(x => x.Initialize(services));
        }



        public override void Process(List<SurfaceEntity> entities, EntityObjectDomain entityObjectDomain)
        {
            lock (_surfaceMaterialTranslators)
            {
                foreach (var surface in entities)
                {
                    var materialHandler = _surfaceMaterialTranslators[surface.Material.GetType()];
                    var sceneNode = materialHandler.CreateSceneNode(entityObjectDomain.ContentLoader, surface.Material, surface);
                    sceneNode.Name = "Surface";
                    //sceneNode.PoseWorld = new Pose(DigitalRuneUtils.ZUpRotation);

                    //in theory, the heighfield collision shape is far more efficient for picking
                    //however, it will collide on the sides, and we don't want that (especially if we have objects below the surface)
                    //so until we figure out how to improve it, we better leave the triangle mesh
                    var shape = new TriangleMeshShape(((MeshNode)sceneNode).Mesh.ToTriangleMesh());
                    //var shape = surface.CalculateHeightField();

                    EntityObject componentObject = new EntityObject(entityObjectDomain);
                    componentObject.AddComponent(new SceneComponent(sceneNode));
                    componentObject.AddComponent(new CollisionComponent(shape, sceneNode.PoseWorld));
                    componentObject.AddComponent(new SelectableEntityComponent(surface));
                    componentObject.AddComponent(new CollisionHighlightComponent());
                    componentObject.AddComponent(new ScopeHighlightComponent(surface.BoxScope));

                    entityObjectDomain.ComponentObjects.Add(componentObject);
                }
            }
        }
    }
}