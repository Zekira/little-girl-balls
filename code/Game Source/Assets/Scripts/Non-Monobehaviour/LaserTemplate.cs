using UnityEngine;
using System.Collections;

public struct LaserTemplate {

    public static LaserTemplate basic = new LaserTemplate(true);

    public int warnDuration;
    public int shotDuration;
    public float width;
    public Vector2 movement;
    public float rotation;
    public float rotationSpeed;
    public Vector2 position;
    public bool positionIsRelative;

    public Color outerColor;
    public Color innerColor;

    private LaserTemplate(bool basic) {
        warnDuration = 0;
        shotDuration = 0;
        width = 0.5f;
        movement = Vector3.zero;
        rotation = 0f;
        rotationSpeed = 0f;
        position = Vector3.zero;
        positionIsRelative = true;

        outerColor = Color.white;
        innerColor = Color.white;
    }
}

/*public class LaserTemplate {

    public int warnDuration = 0; //In ticks
    public int shotDuration = 0; //In ticks
    public float width = 0.5f; //Target width
    public Vector2 movement = new Vector2(0, 0); //In units per tick
    public float rotation = 0f; //In radians
    public float rotationSpeed = 0f; //In radians per tick
    public Vector2 position = new Vector2(0, 0);
    public bool positionIsRelative = true;

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
        position = template.position;
        positionIsRelative = template.positionIsRelative;
    }
}
*/