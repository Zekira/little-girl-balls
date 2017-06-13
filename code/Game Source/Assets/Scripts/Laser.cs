using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {

    public LaserTemplate template = new LaserTemplate();
    public int timer = 0;
    Vector3 playerPos;
    float sin, cos, angle;

    void Start() {
        template.width = 0.5f;
        template.shotDuration = 60;
        template.warnDuration = 60;
    }

    void Update() {
        if (!GlobalHelper.paused) {
            if (timer < template.warnDuration) {
                //widening animation
                transform.localScale = new Vector3(Mathf.Lerp(0.03f, template.width, timer / (1f * template.warnDuration)), 99f, 1f);
            } else if (timer > template.warnDuration) {
                //collision check
                playerPos = GlobalHelper.GetPlayer().transform.position; //The player position in the regular coordinate system
                playerPos -= transform.position; //The player position in a coordinate system through this object, parallel with the previous one
                angle = transform.localEulerAngles.z;
                sin = Mathf.Sin(angle * Mathf.Deg2Rad);
                cos = Mathf.Cos(angle * Mathf.Deg2Rad);
                playerPos = new Vector3(playerPos.x * cos + playerPos.y * sin, -playerPos.x * sin + playerPos.y * cos, 0f); //The player position in a coordinate system where the laser points straight down.
                if (Mathf.Abs(playerPos.x) < template.width / 2 && playerPos.y < 0) {
                    GlobalHelper.GetStats().TakeDamage();
                }
            } else if (timer > template.warnDuration + template.shotDuration) {
                //GetComponent<Bullet>().Deactivate();
                Destroy(this.gameObject);
            }

            //TODO: Graze
            timer++;
        }
    }
}
