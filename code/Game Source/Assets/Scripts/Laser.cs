using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {

    public LaserTemplate template;
    public int timer = 0;

    void Update() {
        if (timer == template.warnDuration) {
            //transition to shooting bullets
        } else if (timer > template.warnDuration) {
            //shoot bullets
        } else if (timer > template.warnDuration + template.shotDuration) {
            GetComponent<Bullet>().Deactivate();
        }

        timer++;
    }
}
