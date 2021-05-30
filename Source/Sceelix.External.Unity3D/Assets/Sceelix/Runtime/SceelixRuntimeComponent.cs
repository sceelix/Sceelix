using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Sceelix.Runtime
{
    [AddComponentMenu("Sceelix/Runtime Component")]
    public class SceelixRuntimeComponent : MonoBehaviour
    {
        public TextAsset PackageAsset;
        public String GraphLocation = "SubDir/MyGraph.slxg";
        

        private RemoteGraphProcedure _procedure;

        public void Start()
        {
            _procedure = new RemoteGraphProcedure(PackageAsset, GraphLocation);
            _procedure.Ready += ProcedureOnReady;
            _procedure.Execute();
        }

        private void ProcedureOnReady(IEnumerable<GameObject> gameObjects)
        {
            //destroy any children that may exist
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            //set the new objects as children
            foreach (GameObject childGameObject in gameObjects)
                childGameObject.transform.parent = this.gameObject.transform;
        }

        public void SetParameter(string key, object value)
        {
            _procedure.SetParameter(key,value);
        }

        public void Reexecute()
        {
            _procedure.Execute();
        }
    }
}
