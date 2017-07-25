using UnityEngine;
using System.Collections;
/// <summary>
/// A class representing the physical instance of a BulletTemplate attached to a GameObject.
/// </summary>
public class Bullet : MonoBehaviour {

    public BulletTemplate bulletTemplate;
    public bool grazed;

    public Snake relatedSnake = null;
    public int relatedSnakeIndex = 0;

    public float posx, posy, posz;
    private static float deltax = 0f;
    private static float deltay = 0f;
    private bool updateCollisions = true;
    private bool deactivated = false;
    private static Vector3 otherpos;
    private Transform thisTransform;

    private void Start() {
        thisTransform = transform;
    }
    /// <summary>
    /// Set all values to default, and the template to the argument. Does NOT affect a TimelineInterprenter attached; use its own reset for that.
    /// </summary>
    public void Reset(BulletTemplate template) {
        bulletTemplate = template;
        grazed = false;
        deactivated = false;
        updateCollisions = true;
        /*posx = 0; posy = 0; posz = 0; Set by GlobalHelper.Createbullet()*/ /*deltax = 0; deltay = 0; done every tick without saving data, doesn't need to be reset*/
    }

	void Update () {
        if (!GlobalHelper.paused) {
            //Move it
            posx += bulletTemplate.movement.x;
            posy += bulletTemplate.movement.y;
            thisTransform.position = new Vector3(posx, posy, posz);
            //Do collision checks only once every other frame because they are intensive.
            if (updateCollisions) {
                if (posx * posx + posy * posy > 64) { //AKA when it's so far out of the field it's irrelevant
                    Deactivate();
                }
                updateCollisions = false;
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

                        otherpos = PlayerPosGetter.playerPos;
                        deltax = otherpos.x - posx;
                        deltay = otherpos.y - posy;
                        if (!PlayerStats.noMovement && deltax * deltax + deltay * deltay < 0.5f * bulletTemplate.scale / 2f * bulletTemplate.scale / 2f + PlayerStats.hitboxRadius * PlayerStats.hitboxRadius * 0.33f) {
                            GlobalHelper.stats.TakeDamage();
                            Deactivate();
                        } else if (!grazed && deltax * deltax + deltay * deltay < PlayerStats.grazeRadius * PlayerStats.grazeRadius) {
                            PlayerStats.Graze();
                            grazed = true;
                        }
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
            } else {
                updateCollisions = true;
            }
        }
    }

    /// <summary>
    /// Sets the bullet to inactive and into GlobalHelper's bullet queue.
    /// </summary>
    public void Deactivate() {
        if (!deactivated) {
            GetComponent<SpriteRenderer>().sprite = null;
            deactivated = true;
            GlobalHelper.currentBullets--;
            GlobalHelper.backupBullets.Add(gameObject);
            gameObject.SetActive(false);
        }
    }
}
