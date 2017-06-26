using UnityEngine;
using System.Collections;

public class BulletListFiller : MonoBehaviour {

	void Update () {
        if (GlobalHelper.backupBullets.Count < 4000) {
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
        } else {
            Destroy(this.GetComponent<BulletListFiller>());
        }
	}
}