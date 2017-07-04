using UnityEngine;
using System.Collections;
/// <summary>
/// A small class handling the animation and other effects of death: not being able to move and going to the starting position.
/// </summary>
public class DeathAnimation : MonoBehaviour {

    public float scale = 0;
    public int time = 0;
    void Update() {
        if (!GlobalHelper.paused) {
            scale = time / 15f - time * time / 900f; //A nice parabola going through (0,0), (60,0) and (30,150)
            transform.localScale = scale * Vector3.one;
            if (time >= 60) {
                scale = 0;
                time = 0;
                GlobalHelper.stats.noMovement = false;
                transform.parent.position = GlobalHelper.stats.startPosition;
                gameObject.SetActive(false);
            }
            time++;
        }
    }
}
