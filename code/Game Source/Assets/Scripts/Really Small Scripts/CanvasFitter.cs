using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Changes the canvas' size to fit regardless of resolution
/// </summary>
[ExecuteInEditMode]
public class CanvasFitter : MonoBehaviour {

    Canvas canvas;
    CanvasScaler scaler;

    void Start () {
        canvas = GetComponent<Canvas>();
        scaler = canvas.GetComponent<CanvasScaler>();
        FitCanvas();
    }
	
	void Update () {
        FitCanvas();
    }

    void FitCanvas() {
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
