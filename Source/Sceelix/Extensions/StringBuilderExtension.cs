using System.Text;

namespace Sceelix.Extensions
{
    public static class StringBuilderExtension
    {
        public static StringBuilder AppendTabs(this StringBuilder stringBuilder, int numTabs)
        {
            for (int i = 0; i < numTabs; i++) stringBuilder.Append("\t");

            return stringBuilder;
        }
    }
}