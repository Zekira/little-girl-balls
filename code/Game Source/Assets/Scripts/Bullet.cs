using UnityEngine;
using System.Collections;
/// <summary>
/// A class representing the physical instance of a BulletTemplate attached to a GameObject.
/// </summary>
/// //TODO: Make collisions' wait between checks longer if further from the player.
public class Bullet : MonoBehaviour {

    public BulletTemplate bulletTemplate;
    public bool grazed;

    public Snake relatedSnake = null;
    public int relatedSnakeIndex = -1;

    public float posx, posy, posz;
    private static float deltax = 0f;
    private static float deltay = 0f;
    private static float d = 0f;
    private int updateCollisions = 0;
    private bool deactivated = false;
    private static Vector3 otherpos;
    private Transform thisTransform; //This and the next set in globalhelper if the bullet doesn't exist
    private SpriteRenderer spriteRenderer; 
    private BulletMaterialisation materialisation;

    private void SetBasicReferences() {
        thisTransform = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        materialisation = GetComponent<BulletMaterialisation>();
    }

    /// <summary>
    /// Set all values to default, and the template to the argument. Does NOT affect a TimelineInterprenter attached; use its own reset for that.
    /// </summary>
    public void Reset(BulletTemplate template) {
        SetBasicReferences();
        bulletTemplate = template;
        grazed = false;
        deactivated = false;
        updateCollisions = 0;
        /*posx = 0; posy = 0; posz = 0; Set by GlobalHelper.Createbullet()*/ /*deltax = 0; deltay = 0; done every tick without saving data, doesn't need to be reset*/
    }

	void Update () {
        if (!GlobalHelper.paused) {
            //Move it
            posx += bulletTemplate.movement.x;
            posy += bulletTemplate.movement.y;
            thisTransform.position = new Vector3(posx, posy, posz);
            //Do collision checks only once every other frame because they are intensive.
            if (updateCollisions <= 0) {
                if (posx * posx + posy * posy > 64) { //AKA when it's so far out of the field it's irrelevant
                    Deactivate();
                }
                //This block only checks collision; a harmless bullet can't collide with anything.
                if (!bulletTemplate.harmless) {
                    //Check whether colliding with the player is lethal, and if so, either be grazed or be lethal.
                    //A bullet is 1 unit long if its scale is 1.
                    if (bulletTemplate.enemyShot) {
                        if (posy > GlobalHelper.bulletClear.destroyBulletsHeight) { //Destroy it if the clear "animation" is happening and it's above the height.
                            if ((int)GlobalHelper.bulletClear.bulletClearType <= 1) { //If the clear is due to death/bombs...
                                if (!bulletTemplate.clearImmune) {
                                    Deactivate();
                                }
                                //Here the bullet is immune to this "some" clear.
                            } else { //It's "all" and all bullets should be cleared
                                Deactivate();
                            }
                        }
                        CheckPlayerCollision();
                        return; //it's updateCollisions shouldn't be set to 1 here, but to whatever heckPlayerCollisions() decides
                    } else { //If the bullet is not harmful to the player, it should check enemies and damage them.
                        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) {
                            if (enemy.activeSelf == true) {
                                otherpos = enemy.transform.position;
                                deltax = otherpos.x - posx;
                                deltay = otherpos.y - posy;
                                if (deltax * deltax + deltay * deltay < 1) {
                                    enemy.GetComponent<Enemy>().TakeDamage(bulletTemplate.bulletDamage);
                                    PlayerStats.SetScore(PlayerStats.score + 10);
                                    Deactivate();
                                }
                            }
                        }
                    }
                }
                updateCollisions = 1;
            } else {
                updateCollisions--;
            }
        }
    }

    private void CheckPlayerCollision() {
        otherpos = PlayerPosGetter.playerPos;
        deltax = otherpos.x - posx;
        deltay = otherpos.y - posy;
        d = deltax * deltax + deltay * deltay;
        if (!PlayerStats.noMovement && d < 0.5f * bulletTemplate.scale / 2f * bulletTemplate.scale / 2f + PlayerStats.hitboxRadius * PlayerStats.hitboxRadius * 0.33f) {
            GlobalHelper.stats.TakeDamage();
            Deactivate();
        } else if (!grazed && d < PlayerStats.grazeRadius * PlayerStats.grazeRadius) {
            PlayerStats.Graze();
            grazed = true;
        }
        if (d < 1) { //doing this manually as it's faster than a sqrt-based formula
            updateCollisions = 1;
            return;
        } if (d < 4) {
            updateCollisions = 5;
            return;
        } if (d < 9) {
            updateCollisions = 9;
            return;
        } if (d < 16) {
            updateCollisions = 13;
            return;
        } if (d < 25) {
            updateCollisions = 17;
            return;
        } if (d < 36) {
            updateCollisions = 21;
            return;
        } if (d < 49) {
            updateCollisions = 25;
            return;
        }
        updateCollisions = 29;

    }

    /// <summary>
    /// Sets the bullet to inactive and into GlobalHelper's bullet queue.
    /// </summary>
    public void Deactivate() {
        if (!deactivated) {
            if (relatedSnake != null) {
                relatedSnake.Remove(relatedSnakeIndex);
                relatedSnake = null;
                relatedSnakeIndex = -1;
            }
            SetSprite(null);
            deactivated = true;
            GlobalHelper.currentBullets--;
            GlobalHelper.backupBullets.Add(gameObject);
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sets the sprite of a bullet to be something, or if it's materialising, what the bullet should be. To be used when the bullet already exists
    /// </summary>
    public void SetSprite(Sprite sprite) {
        if (materialisation == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
            materialisation = GetComponent<BulletMaterialisation>();
        }
        materialisation.actualSprite = sprite;
        if (!materialisation.enabled) {
            SetSpriteDirectly(sprite);
        }
    }

    /// <summary>
    /// Sets the sprite of a bullet to be something directly. Only to be used when creating the bullet as it would otherwise fail when BulletMaterialisation is active.
    /// </summary>
    public void SetSpriteDirectly (Sprite sprite) {
        spriteRenderer.sprite = sprite;
    }
}
