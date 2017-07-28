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
/*
/// <summary>
/// A class representing the data belonging to a bullet, whether it actually exists or not.
/// </summary>
public class BulletTemplate {

    public Vector2 movement = new Vector2(0, 0); //Tiles PER SECOND. That is, 60 ticks.
    public bool enemyShot = true; //Whether it hurts the player and can be grazed.
    public bool harmless = false; //Whether it can actually hurt anything.
    public float scale = 1f; //Diameter of the bullet.
    public float rotation = 0f; //Z-axis rotation of the bullet.
    public float rotationSpeed = 0f;
    public byte bulletID = 0; //What texture to grab.
    public Color innerColor = Color.white; //What color the outer color (green in the sprite) should become.
    public Color outerColor = Color.white; //What color the inner color (red in the sprite) should become.
    public int bulletDamage = 1; //What damage the bullet does when hitting an enemy.
    public Vector2 position = new Vector2(0f, 0f); //What position to spawn the bullet in.
    public bool positionIsRelative = true; //Whether position is added to its spawner's position.
    public bool clearImmune = false; //Whether this bullet is immune to clearing due to deaths/bombs/player on its spawn etc.
    public string advancedAttackPath = "";
    public float scriptRotation = 0f;
    public Vector4 scriptRotationMatrix = new Vector4(1, 0, 0, 1); //Rotates TimelineInterprenter's bulletproperty/movement,position, moveparent

    public BulletTemplate() {
    }

    public BulletTemplate(BulletTemplate template) {
        movement = template.movement;
        enemyShot = template.enemyShot;
        scale = template.scale;
        rotation = template.rotation;
        rotationSpeed = template.rotationSpeed;
        bulletID = template.bulletID;
        innerColor = template.innerColor;
        outerColor = template.outerColor;
        bulletDamage = template.bulletDamage;
        position = template.position;
        positionIsRelative = template.positionIsRelative;
        clearImmune = template.clearImmune;
        advancedAttackPath = template.advancedAttackPath;
        scriptRotationMatrix = template.scriptRotationMatrix;
        scriptRotation = template.scriptRotation;
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
*/
