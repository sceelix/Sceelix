using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Conversion;

namespace Sceelix.Actors.Conversions
{
    [ConversionFunctions]
    public class MaterialConversion
    {
        public static SceeList SceelistToMaterialConversion(Material material)
        {
            //simple, for now
            return material.Attributes.ToSceeList();
        }
    }
}