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