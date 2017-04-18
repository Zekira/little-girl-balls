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

	void Update () {
        if (!GlobalHelper.paused) {
            //Check whether colliding with the player is lethal, and if so, either be grazed or be lethal.
            //A bullet is 1 unit long if its scale is 1.
            if (bulletTemplate.isHarmful) {
                if (posy > GlobalHelper.destroyBulletsHeight) { //Destroy it if the clear "animation" is happening and it's above the height. TODO: Add a var to make some bullets immune to clearing.
                    Deactivate();
                }

                otherpos = GlobalHelper.GetStats().playerPosition;
                deltax = otherpos.x - posx;
                deltay = otherpos.y - posy;
                if (!GlobalHelper.GetStats().noMovement && deltax * deltax + deltay * deltay < 0.5f * bulletTemplate.scale / 2f * bulletTemplate.scale / 2f + GlobalHelper.GetStats().hitboxRadius * GlobalHelper.GetStats().hitboxRadius) {
                    GlobalHelper.GetStats().TakeDamage();
                    Deactivate();
                } else if (!GlobalHelper.GetStats().noMovement && !grazed && deltax * deltax + deltay * deltay < GlobalHelper.GetStats().grazeRadius * GlobalHelper.GetStats().grazeRadius) {
                    GlobalHelper.GetStats().IncrementGraze();
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
            if (posx * posx + posy * posy > 36) { //AKA when it's so far out of the field it's irrelevant
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
    private void Deactivate() {
        GlobalHelper.backupBullets.Add(gameObject);
        gameObject.SetActive(false);
    }
}
