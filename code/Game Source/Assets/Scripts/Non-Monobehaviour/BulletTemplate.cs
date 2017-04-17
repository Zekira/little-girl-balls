using UnityEngine;
using System.Collections;
/// <summary>
/// A class representing the data belonging to a bullet, whether it actually exists or not.
/// </summary>
public class BulletTemplate {

    public Vector2 movement = new Vector2(0, -1 / 60f); //Tiles PER SECOND. That is, 60 ticks.
    public bool isHarmful = true; //Whether it hurts the player and can be grazed.

    public float scale = 1f; //Diameter of the bullet.
    public float rotation = 0f; //Z-axis rotation of the bullet.
    public float rotationSpeed = 0f;
    public byte bulletID = 0; //What texture to grab.
    public Color innerColor = Color.white; //What color the outer color (green in the sprite) should become.
    public Color outerColor = Color.white; //What color the inner color (red in the sprite) should become.
    public int bulletDamage = 1;

    public BulletTemplate() {
    }

    public BulletTemplate(BulletTemplate template) {
        movement = template.movement;
        isHarmful = template.isHarmful;
        scale = template.scale;
        rotation = template.rotation;
        rotationSpeed = template.rotationSpeed;
        bulletID = template.bulletID;
        innerColor = template.innerColor;
        outerColor = template.outerColor;
        bulletDamage = template.bulletDamage;
    }

    /// <summary>
    /// Changes the length of the movement vector.
    /// </summary>
    /// <param name="speed">The speed in units per tick.</param>
    /// <returns>Returns the modified vector.</returns>
    public Vector2 SetSpeed(float speed) {
        movement = speed * movement.normalized;
        return movement;
    }
}
