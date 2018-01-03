using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the "animation" of being spawned; during that time the bullet doesn't do anything.
/// Also handles some stuff about spawning bullets.
/// </summary>
public class BulletMaterialisation : MonoBehaviour {

    private BulletTemplate template;
    public static Sprite materialiseSprite;
    public Sprite actualSprite;
    private int timer = 9;
    private Color color;

    private Bullet bullet;
    private Transform thisTransform;
    private SpriteRenderer spriteRenderer;

    void Start() {
        bullet = GetComponent<Bullet>();
        thisTransform = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
	
	void Update () {
        if (!GlobalHelper.paused) {
            if (timer == 9) { //Initialising the process of spawning.
                color = spriteRenderer.color;
                thisTransform.position += new Vector3(0f, 0f, -5f);
                template = bullet.bulletTemplate;
                actualSprite = spriteRenderer.sprite;
                if (template.enemyShot) { //Player bullets don't need the distracting spawn animation
                    bullet.SetSpriteDirectly(materialiseSprite);
                } else {
                    bullet.SetSpriteDirectly(null);
                }
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
                thisTransform.localScale = template.scale * new Vector3(1,1,1);
                spriteRenderer.color = Vector4.one;
                //If this is an advanced bullet, enable it here.
                if (template.advancedAttackPath != "") {
                    TimelineInterprenter interprenter = GetComponent<TimelineInterprenter>();
                    interprenter.enabled = true;
                    interprenter.patternPath = template.advancedAttackPath;
                    //interprenter.Reset(template.advancedAttackPath); already called in timelineinterprenter's start
                }
                this.enabled = false;
            } else { //Times 1 through 8
                if ((timer & 1) == 1) {
                    color.a = ((9f - timer) / 13f);
                    spriteRenderer.color = color;
                    thisTransform.localScale = (0.5f + template.scale * timer / 5f) * new Vector3(1,1,1);
                }
                timer--;
            }
        }
	}
}
