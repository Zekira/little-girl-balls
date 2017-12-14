using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {

    public LaserTemplate template = LaserTemplate.basic;
    public int timer = 0;
    Vector3 playerPos;
    float sin, cos, angle;
    Vector3 rotationVector;
    Vector3 movementVector;
    private int grazeCooldown = 0;
    private Transform thisTransform;

    void Start() {
        thisTransform = transform;
        thisTransform.Rotate(new Vector3(0f, 0f, template.rotation * Mathf.Rad2Deg));
        movementVector = new Vector3(template.movement.x, template.movement.y, 0f);
        rotationVector = new Vector3(0f, 0f, template.rotationSpeed * Mathf.Rad2Deg);
    }

    void Update() {
        if (!GlobalHelper.paused) {
            if (rotationVector.sqrMagnitude > 0) {
                thisTransform.position += movementVector;
            }
            if (rotationVector.z != 0) {
                thisTransform.Rotate(rotationVector);
            }

            if (timer < template.warnDuration && timer > template.warnDuration - 10) {
                //widening animation that lasts for 10 ticks
                thisTransform.localScale = new Vector3(Mathf.Lerp(0.06f, template.width, (timer-template.warnDuration+10) / 10f), 99f, 1f);
            } else if (timer > template.warnDuration && timer < template.warnDuration + template.shotDuration) {
                //collision check
                playerPos = PlayerPosGetter.playerPos; //The player position in the regular coordinate system
                playerPos -= thisTransform.position; //The player position in a coordinate system through this object, parallel with the previous one
                angle = thisTransform.localEulerAngles.z;
                sin = Mathf.Sin(angle * Mathf.Deg2Rad);
                cos = Mathf.Cos(angle * Mathf.Deg2Rad);
                playerPos = new Vector3(playerPos.x * cos + playerPos.y * sin, -playerPos.x * sin + playerPos.y * cos, 0f); //The player position in a coordinate system where the laser points straight down.
                if (Mathf.Abs(playerPos.x) < template.width / 4 && playerPos.y > 0) {
                    GlobalHelper.stats.TakeDamage();
                }
                if (grazeCooldown <= 0 && Mathf.Abs(playerPos.x) < (template.width + 0.4) / 2 && playerPos.y > 0) {
                    PlayerStats.Graze();
                    grazeCooldown = 4;
                }
            } else if (timer > template.warnDuration + template.shotDuration) {
                Destroy(this.gameObject);
            }

            if (GlobalHelper.bulletClear.destroyBulletsHeight < thisTransform.position.y && GlobalHelper.bulletClear.bulletClearType == BulletClear.BulletClearType.FULLCLEAR) {
                Destroy(this.gameObject);
            }

            if (grazeCooldown > 0) {
                grazeCooldown--;
            }
            timer++;
        }
    }
}
