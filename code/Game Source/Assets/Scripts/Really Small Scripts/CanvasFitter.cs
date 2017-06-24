using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CanvasFitter : MonoBehaviour {

    Canvas canvas;
    CanvasScaler scaler;

    void Start () {
        canvas = GetComponent<Canvas>();
        scaler = canvas.GetComponent<CanvasScaler>();
        fitCanvas();
    }
	
	void Update () {
        fitCanvas();
    }

    void fitCanvas() {
        if (scaler == null) {
            scaler = canvas.GetComponent<CanvasScaler>();
        }
        if ((float)Screen.height / Screen.width < 0.75f) { //Bars on the side
            scaler.matchWidthOrHeight = 1f;
        } else { //Bars on top (or no bars)
            scaler.matchWidthOrHeight = 0f;
        }
    }
}
