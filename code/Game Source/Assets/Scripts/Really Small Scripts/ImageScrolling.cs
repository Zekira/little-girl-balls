﻿using UnityEngine;
using System.Collections;

/// <summary>
/// A class meant to work with the OffsetTexture shader to cause looping. All vars are meant to be set in the inspector.
/// TODO: remove the seam at the edges of the original image.
/// </summary>
public class ImageScrolling : MonoBehaviour {

    public enum Direction { UP, DOWN, LEFT, RIGHT};
    public Direction direction;
    public float speed = 0.016f;

    private float offsetx = 0f;
    private float offsety = 0f;
    public SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Start() {
        propertyBlock = new MaterialPropertyBlock();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.GetPropertyBlock(propertyBlock);
    }

	void Update () {
        if (!GlobalHelper.paused) {
            switch (direction) {
                case Direction.UP:
                    offsety = (offsety + speed) % 1;
                    break;
                case Direction.DOWN:
                    offsety = (offsety - speed) % 1;
                    break;
                case Direction.LEFT:
                    offsetx = (offsetx - speed) % 1;
                    break;
                case Direction.RIGHT:
                    offsetx = (offsetx + speed) % 1;
                    break;
            }
            propertyBlock.SetFloat("_AmountX", offsetx);
            propertyBlock.SetFloat("_AmountY", offsety);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
