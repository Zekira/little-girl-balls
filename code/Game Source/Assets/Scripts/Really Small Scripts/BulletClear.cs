using UnityEngine;
using System.Collections;

/// <summary>
/// Class meant to handle clearing bullets for whatever reason.
/// </summary>
public class BulletClear : MonoBehaviour {

    public float destroyBulletsHeight = 99f;
    public enum BulletClearType { SOME, ALL }; //"Some" with things such as deaths / bombs, "all" when switching spellcards.
    public BulletClearType bulletClearType = BulletClearType.ALL;

    /// <summary>
    /// Clears the bullets from top to bottom, lowering "speed" units per second.
    /// </summary>
    public IEnumerator Clear(float speed, BulletClearType type) {
        bulletClearType = type;
        float currentHeight = 5f; //The top of the screen
        while (currentHeight > -5f) {
            currentHeight -= speed;
            destroyBulletsHeight = currentHeight;
            yield return null;
        }
        destroyBulletsHeight = 99f; //Reset it after having cleared.
    }

    /// <summary>
    /// Clears the bullets from top to bottom, lowering "speed" units per second, ending after "time" ticks, no matter whether it reached the end or not.
    /// </summary>
    public IEnumerator Clear(float speed, BulletClearType type, int time) {
        bulletClearType = type;
        float currentHeight = 5f;
        int currentTime = 0;
        while (currentTime < time) {
            currentHeight -= speed;
            currentTime++;
            destroyBulletsHeight = currentHeight;
            yield return null;
        }
        destroyBulletsHeight = 99f;
    }
}
