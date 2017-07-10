using UnityEngine;
using System.Collections;

public class BulletListFiller : MonoBehaviour {

	void Update () {
        if (GlobalHelper.backupBullets.Count < 1500) { //There'll be probably never more than 1500 bullets on screen at any time.
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
        } else {
            Destroy(this);
        }
	}
}