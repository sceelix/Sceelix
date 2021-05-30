using UnityEngine;

namespace Assets.Cameras
{
    public class FlyCam : MonoBehaviour {
	
	
        public float speed = 50.0f;		// max speed of camera
        public float sensitivity = 0.25f; 		// keep it from 0..1
        public bool inverted = false;
	
	
        private Vector3 lastMouse = new Vector3(255, 255, 255);
	
	
        // smoothing
        public bool smooth = true;
        public float acceleration = 0.1f;
        private float actSpeed = 0.0f;			// keep it from 0 to 1
        private Vector3 lastDir = new Vector3();
	
	
        // Use this for initialization
        void Start () {
	
        }
	
        // Update is called once per frame
        void Update () {
		
		
            // Mouse Look
            lastMouse = Input.mousePosition - lastMouse;
            if ( ! inverted ) lastMouse.y = -lastMouse.y;
            lastMouse *= sensitivity;
            lastMouse = new Vector3( transform.eulerAngles.x + lastMouse.y, transform.eulerAngles.y + lastMouse.x, 0);
            transform.eulerAngles = lastMouse;
            lastMouse = Input.mousePosition;
		
		
		
		
		
            // Movement of the camera
		
            Vector3 dir = new Vector3();			// create (0,0,0)
		
            if (Input.GetKey(KeyCode.W)) dir.z += 1.0f;
            if (Input.GetKey(KeyCode.S)) dir.z -= 1.0f;
            if (Input.GetKey(KeyCode.A)) dir.x -= 1.0f;
            if (Input.GetKey(KeyCode.D)) dir.x += 1.0f;
            dir.Normalize();
		
		
            if (dir != Vector3.zero) {
                // some movement 
                if (actSpeed < 1)
                    actSpeed += acceleration * Time.deltaTime * 40;
                else 
                    actSpeed = 1.0f;
			
                lastDir = dir;
            } else {
                // should stop
                if (actSpeed > 0)
                    actSpeed -= acceleration * Time.deltaTime * 20;
                else 
                    actSpeed = 0.0f;
            }
		
		
		
		
            if (smooth) 
                transform.Translate( lastDir * actSpeed * speed * Time.deltaTime );
		
            else 
                transform.Translate ( dir * speed * Time.deltaTime );
		
		
        }
	
        void OnGUI() {
            //GUILayout.Box ("actSpeed: " + actSpeed.ToString());
        }
    }
}
