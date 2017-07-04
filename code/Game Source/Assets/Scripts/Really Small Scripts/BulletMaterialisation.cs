﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the "animation" of being spawned; during that time the bullet doesn't do anything.
/// Also handles some stuff about spawning bullets.
/// </summary>
public class BulletMaterialisation : MonoBehaviour {

    private BulletTemplate template;
    private SpriteRenderer spriteRenderer;
    public Sprite actualSprite;
    public static Sprite materialiseSprite;
    private Color color;
    public float scale;
    public int timer;

	void OnEnable () {
        timer = 9; //This counts down from 9 to 0.
	}
	
	void Update () {
        if (!GlobalHelper.paused) {
            if (timer == 9) { //Initialising the process of spawning.
                transform.position += new Vector3(0f, 0f, -5f);
                template = GetComponent<Bullet>().bulletTemplate;
                spriteRenderer = GetComponent<SpriteRenderer>();
                actualSprite = spriteRenderer.sprite;
                spriteRenderer.sprite = materialiseSprite;
                scale = template.scale;
            }
            if (timer == 0) { //Stopping and spawning the actual bullet
                timer = 9; //For the next time this gets spawned
                //If it's close to the player, prevent it from spawning if it is able to be cleared by bombs (and thus generally unimportant).
                Bullet bullet = GetComponent<Bullet>();
                if (!bullet.bulletTemplate.clearImmune && Vector2.Distance(transform.position, PlayerPosGetter.playerPos) < bullet.bulletTemplate.scale/2) {
                    bullet.enabled = true;
                    this.enabled = false;
                    bullet.Deactivate();
                }
                transform.position -= new Vector3(0f, 0f, -5f);
                bullet.enabled = true;
                spriteRenderer.sprite = actualSprite;
                transform.localScale = template.scale * Vector3.one;
                spriteRenderer.color = Vector4.one;
                //If this is an advanced bullet, enable it here.
                if (template.advancedAttackPath != "") {
                    GetComponent<TimelineInterprenter>().enabled = true;
                    GetComponent<TimelineInterprenter>().patternPath = template.advancedAttackPath;
                }
                //If this actually is a laser and has a laser component, enable it here.
                if (GetComponent<Laser>() != null) {
                    GetComponent<Laser>().enabled = true;
                }
                this.enabled = false;
            } else {
                color = spriteRenderer.color;
                color = new Color(color.r, color.b, color.g, ((9f - timer) / 13f));
                spriteRenderer.color = color;
                transform.localScale = (0.5f + scale * timer / 5f) * Vector3.one;
                timer--;
            }
        }
	}
}
