﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the "animation" of being spawned; during that time the bullet doesn't do anything.
/// Also handles some stuff about spawning bullets.
/// </summary>
public class BulletMaterialisation : MonoBehaviour {

    private BulletTemplate template;
    public static Sprite materialiseSprite;
    public Sprite actualSprite;
    private Color color;
    public float scale;
    public int timer;

    private Bullet bullet;
    private Transform thisTransform;
    private SpriteRenderer spriteRenderer;

    void Start() {
        bullet = GetComponent<Bullet>();
        thisTransform = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable () {
        timer = 9; //This counts down from 9 to 0.
	}
	
	void Update () {
        if (!GlobalHelper.paused) {
            if (timer == 9) { //Initialising the process of spawning.
                transform.position += new Vector3(0f, 0f, -5f);
                template = bullet.bulletTemplate;
                actualSprite = spriteRenderer.sprite;
                if (template.enemyShot) { //Player bullets don't need the distracting spawn animation
                    bullet.SetSpriteDirectly(materialiseSprite);
                } else {
                    bullet.SetSpriteDirectly(null);
                }
                scale = template.scale;
                if (template.advancedAttackPath != null && template.advancedAttackPath != "") {
                    timer--; //spaghetti; timelineinterprenter takes a tick to setup
                }
            }
            if (timer == 0) { //Stopping and spawning the actual bullet
                timer = 9; //For the next time this gets spawned
                //If it's close to the player, prevent it from spawning if it is able to be cleared by bombs (and thus generally unimportant).
                if (!bullet.bulletTemplate.clearImmune && Vector2.SqrMagnitude((Vector2)transform.position - (Vector2)PlayerPosGetter.playerPos) < bullet.bulletTemplate.scale * bullet.bulletTemplate.scale/4) {
                    bullet.enabled = true;
                    this.enabled = false;
                    bullet.Deactivate();
                }
                thisTransform.position -= new Vector3(0f, 0f, -5f);
                bullet.enabled = true;
                bullet.SetSpriteDirectly(actualSprite);
                thisTransform.localScale = template.scale * Vector3.one;
                spriteRenderer.color = Vector4.one;
                //If this is an advanced bullet, enable it here.
                if (template.advancedAttackPath != "") {
                    TimelineInterprenter interprenter = GetComponent<TimelineInterprenter>();
                    interprenter.enabled = true;
                    interprenter.patternPath = template.advancedAttackPath;
                    interprenter.Reset(template.advancedAttackPath);
                }
                this.enabled = false;
            } else {
                color = spriteRenderer.color;
                color = new Color(color.r, color.b, color.g, ((9f - timer) / 13f));
                spriteRenderer.color = color;
                thisTransform.localScale = (0.5f + scale * timer / 5f) * Vector3.one;
                timer--;
            }
        }
	}
}
