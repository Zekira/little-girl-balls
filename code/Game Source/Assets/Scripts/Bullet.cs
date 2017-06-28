using UnityEngine;
using System.Collections;
/// <summary>
/// A class representing the physical instance of a BulletTemplate attached to a GameObject.
/// </summary>
public class Bullet : MonoBehaviour {

    public BulletTemplate bulletTemplate;
    public bool grazed;
    public GameObject player;
    public float posx, posy, posz, deltax, deltay;
    public int timeUntilUpdatedPosition = 1;
    Vector3 otherpos;

    /// <summary>
    /// Set all values to default. Does NOT affect a TimelineInterprenter attached; use its own reset for that.
    /// </summary>
    public void Reset() {
        bulletTemplate = new BulletTemplate();
        grazed = false;
        posx = 0; posy = 0; posz = 0; deltax = 0; deltay = 0;
        timeUntilUpdatedPosition = 1;
    }

	void Update () {
        if (!GlobalHelper.paused) {
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

                    otherpos = GlobalHelper.GetStats().playerPosition;
                    deltax = otherpos.x - posx;
                    deltay = otherpos.y - posy;
                    if (!GlobalHelper.GetStats().noMovement && deltax * deltax + deltay * deltay < 0.5f * bulletTemplate.scale / 2f * bulletTemplate.scale / 2f + GlobalHelper.GetStats().hitboxRadius * GlobalHelper.GetStats().hitboxRadius * 0.33f) {
                        GlobalHelper.GetStats().TakeDamage();
                        Deactivate();
                    } else if (!grazed && deltax * deltax + deltay * deltay < GlobalHelper.GetStats().grazeRadius * GlobalHelper.GetStats().grazeRadius) {
                        GlobalHelper.GetStats().Graze();
                        grazed = true;
                    }
                } else { //If the bullet is not harmful to the player, it should check enemies and damage them.
                    for (int i = 0; i < GlobalHelper.enemyParent.childCount; i++) {
                        Transform enemy = GlobalHelper.enemyParent.GetChild(i);
                        if (enemy.gameObject.activeSelf == true) {
                            otherpos = enemy.transform.position;
                            deltax = otherpos.x - posx;
                            deltay = otherpos.y - posy;
                            if (deltax * deltax + deltay * deltay < 1) {
                                enemy.GetComponent<Enemy>().TakeDamage(bulletTemplate.bulletDamage);
                                GlobalHelper.stats.SetScore(GlobalHelper.stats.score + 10);
                                Deactivate();
                            }
                        }
                    }
                }
            }
            if (posx * posx + posy * posy > 64) { //AKA when it's so far out of the field it's irrelevant
                Deactivate();
            }
            //Move it
            posx += bulletTemplate.movement.x;
            posy += bulletTemplate.movement.y;
            //Update its world position only once every other frame.
            if (timeUntilUpdatedPosition <= 0) {
                transform.position = new Vector3(posx, posy, posz);
            }
            timeUntilUpdatedPosition--;
        }
    }

    /// <summary>
    /// Sets the bullet to inactive and into GlobalHelper's bullet queue.
    /// </summary>
    public void Deactivate() {
        GlobalHelper.currentBullets--;
        GlobalHelper.backupBullets.Add(gameObject);
        gameObject.SetActive(false);
    }
}
