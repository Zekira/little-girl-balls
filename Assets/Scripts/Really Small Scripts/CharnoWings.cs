using UnityEngine;
using System.Collections;
/// <summary>
/// A small class handling the slowly moving up and down of Charno's wings.
/// </summary>
public class CharnoWings : MonoBehaviour {

    public float time = 0;
    private int direction = 1;

	void Update () {
        Vector3 parentpos = transform.parent.position;
        time += direction / 200f;
        transform.position = new Vector3(parentpos.x, parentpos.y + Mathf.Lerp(0, 1, time)/8f, parentpos.z);
        if (time > 1) {
            direction = -1;
        } else if (time < 0) {
            direction = 1;
        }
	}
}
