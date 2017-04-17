using UnityEngine;
using System.Collections;
/// <summary>
/// A bunch of GlobalHelper.CreateEmptyBullet();'s. Not the best code I've ever written. Only does it if last tick had time left because that's toottaally relevant.
/// </summary>
public class BulletqueueFiller : MonoBehaviour {

    private const float minFrameTime = 1 / 59f;

	void Update () {
        if (Time.deltaTime < minFrameTime) {
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
            GlobalHelper.CreateEmptyBullet();
        }
	}
}
