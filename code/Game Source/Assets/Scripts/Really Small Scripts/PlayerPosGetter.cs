using UnityEngine;
using System.Collections;

/// <summary>
/// Needs to be attached to the player. This class is so that the player position doesn't need to be gotten 2000 times every frame.
/// </summary>
public class PlayerPosGetter : MonoBehaviour {

    public static Vector3 playerPos { get; private set; }

	void Update () {
        playerPos = transform.position;
	}
}
