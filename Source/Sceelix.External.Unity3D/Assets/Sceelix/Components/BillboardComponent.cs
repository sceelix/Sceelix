using UnityEngine;

namespace Assets.Sceelix.Components
{
    /// <summary>
    /// A simple component used to define billboards that should always be facing the camera.
    /// </summary>
    [AddComponentMenu("Sceelix/Billboard")]
    public class BillboardComponent : MonoBehaviour
    {
        private static Mesh _billboardMesh = null;

        public Camera MainCamera;

        void Update()
        {
            if (MainCamera == null)
                MainCamera = Camera.main;

            transform.LookAt(transform.position + MainCamera.transform.rotation * Vector3.forward,MainCamera.transform.rotation * Vector3.up);
        }

        public static Mesh GetMesh()
        {
            if (_billboardMesh == null)
            {
                _billboardMesh = new Mesh();
                _billboardMesh.vertices = new[] {new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0)};
                _billboardMesh.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
                _billboardMesh.triangles = new[] { 0, 1, 2, 0, 2, 3};
                _billboardMesh.RecalculateNormals();
            }

            return _billboardMesh;
        }
    }
}