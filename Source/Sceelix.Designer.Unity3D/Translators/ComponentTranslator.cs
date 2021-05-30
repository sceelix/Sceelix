using System.Collections.Generic;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Services;
using Sceelix.Unity.Data;

namespace Sceelix.Designer.Unity3D.Translators
{
    public abstract class ComponentTranslator
    {
        public abstract IEnumerable<EntityObjectComponent> Process(UnityEntity unityEntity, UnityComponent unityComponent, EntityObjectDomain entityObjectDomain);

        public abstract void PostProcess(IEnumerable<EntityObject> entityObjects, EntityObjectDomain contentLoader);



        public abstract bool IsDrawable
        {
            get;
        }
    }
}