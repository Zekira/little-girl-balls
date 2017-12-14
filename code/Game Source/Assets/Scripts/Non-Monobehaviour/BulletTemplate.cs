using UnityEngine;
using System.Collections;

public struct BulletTemplate {

    public static BulletTemplate basic = new BulletTemplate(true);

    public Vector2 movement;
    public bool enemyShot;
    public bool harmless;
    public float scale;
    public float rotation;
    public float rotationSpeed;
    public byte bulletID;
    public Color innerColor;
    public Color outerColor;
    public int bulletDamage;
    public Vector2 position;
    public bool positionIsRelative;
    public bool clearImmune;
    public string advancedAttackPath;
    public float scriptRotation;
    public int snakeLength;
    public Vector4 scriptRotationMatrix;

    private BulletTemplate(bool basic) {
        movement = new Vector2();
        enemyShot = true;
        harmless = false;
        scale = 1f;
        rotation = 0f;
        rotationSpeed = 0f;
        bulletID = 0;
        innerColor = Color.white;
        outerColor = Color.white;
        bulletDamage = 1;
        position = new Vector2();
        positionIsRelative = true;
        clearImmune = false;
        advancedAttackPath = "";
        scriptRotation = 0f;
        scriptRotationMatrix = new Vector4(1f, 0f, 0f, 1f);
        snakeLength = 0;
    }

    /// <summary>
    /// Sets the rotationMatrix to what it needs to be with angle (in rad).
    /// This influences the reading of TimelineInterprenter with bulletproperty/movement, bulletproperty/position, and moveparent.
    /// To make it compatible with playerangle, it's not going counter-clockwise but clockwise.
    /// </summary>
    public BulletTemplate Rotate(float angle) {
        scriptRotation = angle;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        scriptRotationMatrix = new Vector4(cos, sin, -sin, cos);
        return this;
    }
}