using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CameraScaler : MonoBehaviour {

    Camera gameCamera;

	void Start () {
        gameCamera = GetComponent<Camera>();
        setCameraScale();
	}
    
    void Update () {
        setCameraScale();
	}

    void setCameraScale() {
        if ((float) Screen.height / Screen.width < 0.75f) { //Bars on the side
            gameCamera.orthographicSize = 5f; //Magic, don't touch
        } else { //Bars on top (or no bars)
            gameCamera.orthographicSize = 6.65f * Screen.height / Screen.width; //Magic, don't touch
        }
    }
}
