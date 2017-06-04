using UnityEngine;
using System.Collections;

public class LaserTemplate {

    public BulletTemplate shotBullet; //The bullet to spam-shoot. It's movement also decides the warning rotation.
                                      //The "id" is overwritten here to be a laser-looking bullet, and it's speed is modified to "really fucking fast".

    public int warnDuration = 0; //In ticks
    public int shotDuration = 0; //In ticks
    //public bool visible; unneccessary with alpha 0
    public Color outerColor = new Color(0, 0, 0, 0); //These two the same way as regular bullets
    public Color innerColor = new Color(0, 0, 0, 0);

    public LaserTemplate() {
        shotBullet = new BulletTemplate();
    }

    public LaserTemplate(LaserTemplate template) {
        shotBullet = template.shotBullet;
        shotBullet.rotation = Mathf.Atan2(shotBullet.movement.y, shotBullet.movement.x);
        shotBullet.movement = shotBullet.movement.normalized * shotBullet.scale;
        shotBullet.bulletID = 4; //TODO: Use laser id.
        warnDuration = template.warnDuration;
        shotDuration = template.shotDuration;
        outerColor = new Color(1, 0, 0, 1);
        innerColor = new Color(1, 0, 0, 1);
    }

    /// <summary>
    /// Changes movement, rotation, and ID to what they should be
    /// </summary>
    public void FixValues() {
        shotBullet.rotation = Mathf.Atan2(shotBullet.movement.y, shotBullet.movement.x);
        shotBullet.movement = shotBullet.movement.normalized * shotBullet.scale;
        shotBullet.bulletID = 4; //TODO: Use laser id.
    }
}
