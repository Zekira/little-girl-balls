﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the "animation" of being spawned; during that time the bullet doesn't do anything.
/// </summary>
public class BulletMaterialisation : MonoBehaviour {

    private BulletTemplate template;
    private SpriteRenderer spriteRenderer;
    public Sprite actualSprite;
    public Sprite materialiseSprite;
    private Color color;
    public float scale;
    public int timer;

	void OnEnable () {
        timer = 9;
	}
	
	void Update () {
        if (!GlobalHelper.paused) {
            if (timer == 9) { //Initialising
                transform.position += new Vector3(0f, 0f, -5f);
                template = GetComponent<Bullet>().bulletTemplate;
                spriteRenderer = GetComponent<SpriteRenderer>();
                actualSprite = spriteRenderer.sprite;
                spriteRenderer.sprite = materialiseSprite;
                scale = template.scale;
            }
            if (timer == 0) { //Stopping and spawning the actual bullet
                transform.position -= new Vector3(0f, 0f, -5f);
                GetComponent<Bullet>().enabled = true;
                spriteRenderer.sprite = actualSprite;
                transform.localScale = template.scale * Vector3.one;
                spriteRenderer.color = Vector4.one;
                timer = 9;
                //If this is an advanced bullet, enable it here.
                if (template.advancedAttackPath != "") {
                    GetComponent<TimelineInterprenter>().enabled = true;
                    GetComponent<TimelineInterprenter>().patternPath = template.advancedAttackPath;
                }
                this.enabled = false;
            } else {
                color = spriteRenderer.color;
                color = new Color(color.r, color.b, color.g, ((9f - timer) / 18f)); //TODO: The transparancy stacks. That shouldn't be.
                spriteRenderer.color = color;
                transform.localScale = (0.5f + scale * timer / 5f) * Vector3.one;
                timer--;
            }
        }
	}
}
