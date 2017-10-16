using UnityEngine;
using System.Collections;

public class BulletListFiller : MonoBehaviour {

	void Update () {
        if (GlobalHelper.backupBullets.Count < 3000) { //There'll be probably never more than 3000 bullets on screen at any time.
            ThingCreator.CreateEmptyBullet();
            ThingCreator.CreateEmptyBullet();
            ThingCreator.CreateEmptyBullet();
            ThingCreator.CreateEmptyBullet();
        } else {
            Debug.Log("Spawned 3k bullets. bye");
            Destroy(this);
        }
	}
}