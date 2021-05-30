using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;
using Sceelix.Mathematics.Data;

namespace Sceelix.Meshes.Materials
{
    public class ColorMaterial : MeshMaterial
    {
        private static readonly AttributeKey ColorKey = new GlobalAttributeKey("DefaultColor");



        public ColorMaterial(Color defaultColor)
        {
            DefaultColor = defaultColor;
        }



        public Color DefaultColor
        {
            get { return this.GetAttribute<Color>(ColorKey); }
            set { this.SetAttribute(ColorKey, value); }
        }
    }
}