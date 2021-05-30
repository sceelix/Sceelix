using DigitalRune.Graphics.SceneGraph;
using Sceelix.Actors.Data;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    public interface IMeshMaterialTranslator
    {
        SceneNode CreateSceneNode(ContentLoader contentLoader, Material sceelixMaterial, object data);
    }
}