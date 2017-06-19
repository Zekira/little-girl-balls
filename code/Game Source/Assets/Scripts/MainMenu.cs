using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour {

    int count = 0;

	// Use this for initialization
	void Awake () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (count > 0) {
            Debug.Log(++count);
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            count = 1;
            SceneManager.LoadSceneAsync("level");
        }
    }
}
