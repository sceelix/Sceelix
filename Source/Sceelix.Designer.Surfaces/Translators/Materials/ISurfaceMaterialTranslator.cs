using DigitalRune.Graphics.SceneGraph;
using Sceelix.Actors.Data;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Surfaces.Translators.Materials
{
    public interface ISurfaceMaterialTranslator
    {
        void Initialize(IServiceLocator services);

        SceneNode CreateSceneNode(ContentLoader contentLoader, Material sceelixMaterial, object data);
    }
}