using UnityEngine;
using System.Collections;

/// <summary>
/// Represents an item and all movement attached.
/// </summary>
public class Item : MonoBehaviour {

    public enum ItemType { POWER, LARGEPOWER, POINT, VALUE, LIFEPIECE, BOMBPIECE, FULLPOWER};
    public ItemType type = ItemType.POWER;
    private int cooldown = 1;
    private Vector3 pos;
    public bool autoCollected = false;

    void OnEnable() {
        cooldown = 1;
        pos = transform.position;
        autoCollected = false;
    }

	void Update () {
        if (!GlobalHelper.paused) {
            if (transform.position.y < -4.75) {
                Destroy(this.gameObject);
            }
            if (GlobalHelper.autoCollectItems) {
                autoCollected = true;
            }
            if (cooldown <= 0) {
                pos = transform.position;
                Vector3 playerpos = PlayerPosGetter.playerPos;
                float deltax = playerpos.x - pos.x;
                float deltay = playerpos.y - pos.y;
                float distance = deltax * deltax + deltay * deltay;
                if (!GlobalHelper.GetStats().noMovement && distance < 0.016f) { //Close enough to be picked up. And you can't pick up stuff if you're dead.
                    switch (type) {
                        case ItemType.POWER:
                            GlobalHelper.GetStats().AddPower(5);
                            break;
                        case ItemType.LARGEPOWER:
                            GlobalHelper.GetStats().AddPower(100);
                            break;
                        case ItemType.FULLPOWER:
                            GlobalHelper.GetStats().SetPower(400);
                            break;
                        case ItemType.POINT:
                            GlobalHelper.GetStats().AddScore(GlobalHelper.GetStats().value);
                            break;
                    }
                    GlobalHelper.backupItems.Add(this.gameObject);
                    gameObject.SetActive(false);
                } else if (!GlobalHelper.GetStats().noMovement && (distance < 2 || autoCollected)) { //Close enough to be attracted or autocollected. You also can't attract stuff if you're dead.
                    Vector2 travel = new Vector2(deltax, deltay).normalized / 10f;
                    transform.position += new Vector3(travel.x, travel.y, 0f);
                } else {
                    transform.position += new Vector3(0f, -1 / 40f, 0f);
                }
            }
            cooldown--;
        }
	}
}
