using UnityEngine;
using System.Collections;

public class LaserTemplate {

    public BulletTemplate shotBullet; //The bullet to spam-shoot. It's rotation also decides the warning rotation.

    public int warnDuration; //In ticks
    public int shotDuration; //In ticks
    //public bool visible; unneccessary with alpha 0
    public Color outerColor; //These two the same way as regular bullets
    public Color innerColor;

    public LaserTemplate() { }

    public LaserTemplate(LaserTemplate template) {
        shotBullet = template.shotBullet;
        warnDuration = template.warnDuration;
        shotDuration = template.shotDuration;
        outerColor = template.outerColor;
        innerColor = template.innerColor;
    }
}
