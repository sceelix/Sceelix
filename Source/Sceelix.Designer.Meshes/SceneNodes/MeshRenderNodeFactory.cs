using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using Sceelix.Annotations;
using Sceelix.Designer.Actors.Annotations;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Meshes.Translators.Materials;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Renderer3D.Services;
using Sceelix.Designer.Services;
using Sceelix.Extensions;
using Sceelix.Meshes.Data;

namespace Sceelix.Designer.Meshes.SceneNodes
{
    [Renderer3DService]
    public class MeshRenderNodeFactory : IServiceable
    {
        private readonly Dictionary<Type, IMeshMaterialTranslator> _materialTranslators = AttributeReader.OfTypeKeyAttribute<MaterialTranslatorAttribute>().GetInstancesOfType<IMeshMaterialTranslator>();

        
        public void Initialize(IServiceLocator services)
        {
            _materialTranslators.Values.OfType<IServiceable>().ForEach(x => x.Initialize(services));
        }


        public MeshRenderNode Create(IEnumerable<MeshEntity> meshEntities, ContentLoader contentLoader)
        {
            return new MeshRenderNode(meshEntities, _materialTranslators, contentLoader);
        }
        
        public MeshRenderNode Create(IEnumerable<MeshEntity> meshEntities, ContentLoader contentLoader, Pose poseWorld)
        {
            return new MeshRenderNode(meshEntities, _materialTranslators, contentLoader){PoseWorld = poseWorld};
        }
        
        
        public MeshRenderNode Create(MeshEntity meshEntity, ContentLoader contentLoader)
        {
            return Create(new []{meshEntity}, contentLoader);
        }

        public MeshRenderNode Create(MeshEntity meshEntity, ContentLoader contentLoader, Pose poseWorld)
        {
            return Create(new []{meshEntity}, contentLoader, poseWorld);
        }

    }
}