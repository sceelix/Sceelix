using Sceelix.Annotations;

namespace Sceelix.Designer.Annotations
{
    public class ApplicationSettingsAttribute : StringKeyAttribute
    {
        public ApplicationSettingsAttribute(string key) : base(key)
        {
        }
    }
}
