using UnityEngine;
using System.Collections;

public class LaserTemplate {

    public int warnDuration = 0; //In ticks
    public int shotDuration = 0; //In ticks
    public float width = 0.5f; //Target width
    public Vector2 movement = new Vector2(0, 0); //In units per tick
    public float rotation = 0f; //In radians
    public float rotationSpeed = 0f; //In radians per tick

    public Color outerColor = new Color(1, 1, 1, 1); //These two the same way as regular bullets
    public Color innerColor = new Color(1, 1, 1, 1);

    public LaserTemplate() {
    }

    public LaserTemplate(LaserTemplate template) {
        warnDuration = template.warnDuration;
        shotDuration = template.shotDuration;
        outerColor = template.outerColor;
        innerColor = template.innerColor;
        width = template.width;
        movement = template.movement;
        rotation = template.rotation;
        rotationSpeed = template.rotationSpeed;
    }
}
