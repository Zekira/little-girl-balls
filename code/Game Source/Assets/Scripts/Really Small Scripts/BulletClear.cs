using UnityEngine;
using System.Collections;

/// <summary>
/// Class meant to handle clearing bullets for whatever reason.
/// </summary>
public class BulletClear : MonoBehaviour {

    public float destroyBulletsHeight = 99f;
    public enum BulletClearType { BOMB, DEATH, FULLCLEAR }; //"Some" with things such as deaths / bombs, "all" when switching spellcards.
    public BulletClearType bulletClearType = BulletClearType.FULLCLEAR;

    /// <summary>
    /// Clears the bullets from top to bottom, lowering "speed" units per second.
    /// </summary>
    public IEnumerator Clear(float speed, BulletClearType type) {
        bulletClearType = type;
        float currentHeight = 5f; //The top of the screen
        while (currentHeight > -5f) {
            if (!GlobalHelper.paused) {
                currentHeight -= speed;
                destroyBulletsHeight = currentHeight;
            }
            yield return null;
        }
        destroyBulletsHeight = 99f; //Reset it after having cleared.
    }

    /// <summary>
    /// Clears the bullets from top to bottom, lowering "speed" units per tick, ending after "time" ticks, no matter whether it reached the end or not.
    /// Put as a function in this class because Unity stops any coroutine when its caller is destroyed.
    /// </summary>
    public void Clear(float speed, BulletClearType type, int time) {
        StartCoroutine(CoClear(speed, type, time));
    }

    /// <summary>
    /// Clears the bullets from top to bottom, lowering "speed" units per tick, ending after "time" ticks, no matter whether it reached the end or not.
    /// </summary>
    private IEnumerator CoClear(float speed, BulletClearType type, int time) {
        bulletClearType = type;
        float currentHeight = 5f;
        int currentTime = 0;
        while (currentTime < time) {
            if (!GlobalHelper.paused) {
                currentHeight -= speed;
                currentTime++;
                destroyBulletsHeight = currentHeight;
            }
            yield return null;
        }
        destroyBulletsHeight = 99f;
    }
}
