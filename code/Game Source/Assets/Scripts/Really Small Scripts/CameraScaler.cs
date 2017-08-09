using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Changes the cameras' scale to be the same regardless of resolution
/// </summary>
[ExecuteInEditMode]
public class CameraScaler : MonoBehaviour {

    Camera gameCamera;

	void Start () {
        gameCamera = GetComponent<Camera>();
        SetCameraScale();
	}
    
    void Update () {
        SetCameraScale();
	}

    void SetCameraScale() {
        if ((float) Screen.height / Screen.width < 0.75f) { //Bars on the side
            gameCamera.orthographicSize = 5f; //Magic number, don't touch
        } else { //Bars on top (or no bars)
            gameCamera.orthographicSize = 6.65f * Screen.height / Screen.width; //Magic number, don't touch
        }
    }
}
