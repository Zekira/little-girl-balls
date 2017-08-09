using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Keeps track of the framerate because unity's is weird
/// </summary>
public class FramerateCounter : MonoBehaviour {

    private Text text;
    public int updateSpeed = 30;
    private int ticksPassed = 0;
    private float timePassed = 0f;

	void Start () {
        text = GetComponent<Text>();
	}
	
	void Update () {
        if (ticksPassed < updateSpeed) {
            ticksPassed++;
            timePassed += Time.unscaledDeltaTime;
        } else {
            text.text = (ticksPassed / timePassed).ToString().Substring(0,4);
            ticksPassed = 0;
            timePassed = 0f;
        }
	}
}
