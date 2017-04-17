using UnityEngine;
using System.Collections;

/// <summary>
/// Things I'm too lazy to instantiate that I leave in the scene even though they're not neccessary yet (although needed with some object's instantiation).
/// </summary>
public class HideObjectsOnStart : MonoBehaviour {

    public Transform[] toHide;

	void Start () {
        foreach (Transform t in toHide) {
            t.gameObject.SetActive(false);
        }
	}
}
