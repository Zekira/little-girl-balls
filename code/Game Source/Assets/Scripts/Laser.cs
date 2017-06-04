using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {

    public LaserTemplate template = new LaserTemplate();
    public int timer = 0;

    void Start() {
        BulletTemplate btemplate = new BulletTemplate();
        btemplate.movement = new Vector2(1f, 0f);
        btemplate.scale = 0.3f;
        template.shotDuration = 60;
        template.warnDuration = 60;
        template.FixValues();
        transform.Rotate(0f, 0f, Mathf.Atan2(template.shotBullet.movement.y, template.shotBullet.movement.x));
    }

    void Update() {
        if (timer < template.warnDuration) {
            //widening animation
            //transform.localScale = new Vector3(Mathf.Lerp(0.03f, template.shotBullet.scale, timer / (1f * template.warnDuration)), 99f, 1f);
        } else if (timer == template.warnDuration) {
            //transition to shooting bullets
            //GetComponent<SpriteRenderer>().enabled = false;
        } else if (timer > template.warnDuration) {
            GlobalHelper.CreateBullet(template.shotBullet, transform.position);
            //shoot bullets
        } else if (timer > template.warnDuration + template.shotDuration) {
            GetComponent<Bullet>().Deactivate();
        }

        timer++;
    }
}
