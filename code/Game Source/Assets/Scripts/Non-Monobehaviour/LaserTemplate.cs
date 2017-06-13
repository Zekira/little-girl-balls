using UnityEngine;
using System.Collections;

public class LaserTemplate {

    public int warnDuration = 0; //In ticks
    public int shotDuration = 0; //In ticks
    public float width = 0.5f; //Target width
    public Color outerColor = new Color(0, 0, 0, 0); //These two the same way as regular bullets
    public Color innerColor = new Color(0, 0, 0, 0);

    public LaserTemplate() {
    }

    public LaserTemplate(LaserTemplate template) {
        warnDuration = template.warnDuration;
        shotDuration = template.shotDuration;
        outerColor = new Color(1, 0, 0, 1);
        innerColor = new Color(1, 0, 0, 1);
    }
}
