using UnityEngine;
using System.Collections;

/// <summary>
/// A class meant to work with the OffsetTexture shader to cause looping. All vars are meant to be set in the inspector.
/// If there's a seam at the edges of the texture, or something else is wrong, set the wrapmode to "repeat" before trying to fix.
/// </summary>
public class ImageScrolling : MonoBehaviour {

    //Set in the inspector
    public Vector2 direction;
    public float speed = 0.016f;

    private float offsetx = 0f;
    private float offsety = 0f;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Start() {
        propertyBlock = new MaterialPropertyBlock();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.GetPropertyBlock(propertyBlock);
        SetScrollDirection(direction, speed);
    }

    public void SetScrollDirection(Vector2 direction, float speed) {
        this.direction = direction.normalized * speed;
    }

	void Update () {
        if (!GlobalHelper.paused) {
            offsetx += direction.x;
            offsety += direction.y;
            propertyBlock.SetFloat("_AmountX", offsetx);
            propertyBlock.SetFloat("_AmountY", offsety);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
