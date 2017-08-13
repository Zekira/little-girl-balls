using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Keeps track of the framerate because unity's is weird, and sets whether the bullets should update their transform
/// </summary>
public class FramerateCounter : MonoBehaviour {

    private Text text;
    public int updateSpeed = 30;
    private int ticksPassed = 0;
    private float timePassed = 0f;
    public static float FPS;

	void Start () {
        text = GetComponent<Text>();
	}
	
	void Update () {
        if (ticksPassed < updateSpeed) {
            ticksPassed++;
            timePassed += Time.unscaledDeltaTime;
        } else {
            FPS = ticksPassed / timePassed;
            text.text = FPS.ToString().Substring(0,4);
            ticksPassed = 0;
            timePassed = 0f;
        }
        if (FPS >= 55) {
            Bullet.updatePosition = true;
        } else {
            Bullet.updatePosition = !Bullet.updatePosition;
        }
    }
}
