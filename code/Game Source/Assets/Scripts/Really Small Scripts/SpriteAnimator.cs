﻿using UnityEngine;
using System.Collections;

public class SpriteAnimator : MonoBehaviour {

    public Texture2D texture;
    public int delay = 6;

    private Sprite[] sprites;
    private int currentFrame;
    private int currentDelay = 0;
    private SpriteRenderer spriteRenderer;

    void OnEnable() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (texture != null) {
            SetSprite(texture);
        }
    }

    void Update() {
        if (sprites != null) {
            if (!GlobalHelper.paused) {
                currentDelay--;
            }
            if (currentDelay <= 0) {
                SetFrame(currentFrame + 1);
                currentDelay = delay;
            }
        }
    }

    public Sprite[] SetSprite(Texture2D texture) {
        int frames = texture.width/texture.height;
        sprites = new Sprite[frames];
        for (int i = 0; i < frames; i++) {
            sprites[i] = Sprite.Create(texture, new Rect(i * texture.height, 0, texture.height, texture.height), Vector2.one * 0.5f);
        }
        return sprites;

    }

    public void SetSprites(Sprite[] sprites) {
        this.sprites = sprites;
    }

    public void SetFrame(int frame) {
        if (frame >= sprites.Length) {
            frame -= sprites.Length;
        }
        currentFrame = frame;
        spriteRenderer.sprite = sprites[frame];
    }

}
