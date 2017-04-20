using UnityEngine;
using System.Collections;

public class BulletClear : MonoBehaviour {

    /// <summary>
    /// Clears the bullets from top to bottom, lowering "speed" units per second.
    /// </summary>
    public IEnumerator Clear(float speed) {
        float currentHeight = 5f; //The top of the screen
        while (currentHeight > -5f) {
            currentHeight -= speed;
            GlobalHelper.destroyBulletsHeight = currentHeight;
            yield return null;
        }
        GlobalHelper.destroyBulletsHeight = 99f; //Reset it after having cleared.
    }

    /// <summary>
    /// Clears the bullets from top to bottom, lowering "speed" units per second, ending after "time" ticks.
    /// </summary>
    public IEnumerator Clear(float speed, int time) {
        float currentHeight = 5f;
        int currentTime = 0;
        while (currentTime < time) {
            currentHeight -= speed;
            currentTime++;
            GlobalHelper.destroyBulletsHeight = currentHeight;
            yield return null;
        }
        GlobalHelper.destroyBulletsHeight = 99f;
    }
}
