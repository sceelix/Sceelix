using System;
using Sceelix.Annotations;

namespace Sceelix.Designer.Actors.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MaterialTranslatorAttribute : TypeKeyAttribute
    {
        /*public MaterialTranslatorAttribute(String materialType) :base(materialType)
        {
        }



        public MaterialTranslatorAttribute(Type materialType) : base(materialType.Name)
        {
        }*/



        public MaterialTranslatorAttribute(Type typeKey) 
            : base(typeKey)
        {
        }
    }
}