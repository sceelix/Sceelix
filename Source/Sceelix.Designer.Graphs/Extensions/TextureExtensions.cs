using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Graphs.Extensions
{
    public static class TextureExtensions
    {
        public static Texture2D ToNormalTexture(this Texture2D texture2D)
        {
            if (texture2D == null)
                return null;

            var editableTexture = new MipEditableTexture2D(texture2D);
            for (int k = 0; k < editableTexture.LevelCount; k++)
            {
                var editableSubTexture = editableTexture[k];
                for (int i = 0; i < editableSubTexture.Width; i++)
                {
                    for (int j = 0; j < editableSubTexture.Height; j++)
                    {
                        editableSubTexture[i, j] = new Color(0, 255 - editableSubTexture[i, j].G, 0, editableSubTexture[i, j].R);
                    }
                }
            }

            return editableTexture.ToTexture2D();
        }
    }
}
