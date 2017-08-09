using UnityEngine;
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
            if (sprites.Length == 1) {
                return;
            }
            if (!GlobalHelper.paused) {
                currentDelay--;
            }
            if (currentDelay <= 0) {
                SetFrame(currentFrame + 1);
                currentDelay = delay;
            }
        }
    }

    public void SetSprites(Sprite[] sprites) {
        this.sprites = sprites;
    }

    public void SetSprite(Texture2D texture) {
        sprites = GetSprites(texture);
    }

    public static Sprite[] GetSprites(Texture2D texture) {
        int frames = texture.width / texture.height;
        Sprite[] sprites = new Sprite[frames];
        for (int i = 0; i < frames; i++) {
            sprites[i] = Sprite.Create(texture, new Rect(i * texture.height, 0, texture.height, texture.height), Vector2.one * 0.5f, texture.height);
        }
        return sprites;
    }

    public void SetFrame(int frame) {
        while (frame >= sprites.Length) {
            frame -= sprites.Length;
        }
        currentFrame = frame;
        spriteRenderer.sprite = sprites[frame];
    }

}
