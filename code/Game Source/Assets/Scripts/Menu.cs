using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

    public static Transform selectedObject;
    public static int cooldown = 0;

    public Transform nextObject; //To be set in the inspector
    public Transform prevObject; //To be set in the inspector
    public bool selectable = true;

    void Awake() {
        if (selectedObject == null || selectedObject.gameObject.activeInHierarchy == false) {
            StartCoroutine(GameObject.FindWithTag("MenuBase").GetComponent<Menu>().select(false));
        }
    }
	
	void Update () {
        if (isSelected()) {
            if (cooldown > 0) {
                cooldown--;
            }
            //transform.position += new Vector3(Random.Range(-1.75f, 1.75f), Random.Range(-1.75f, 1.75f), 0f); //test debug to see if selected
            if (Input.GetKey(PlayerMovement.keyUp) && cooldown <= 0) {
                StartCoroutine(deselect());
                StartCoroutine(prevObject.GetComponent<Menu>().select(false));
                cooldown = 10;
            }
            if (Input.GetKey(PlayerMovement.keyDown) && cooldown <= 0) {
                StartCoroutine(deselect());
                StartCoroutine(nextObject.GetComponent<Menu>().select(true));
                cooldown = 10;
            }
            if (Input.GetKeyDown(PlayerMovement.keyShoot)) {
                switch (gameObject.name) {
                    //Mostly main menu items
                    case "Play":
                        transform.parent.parent.FindChild("Difficulty").gameObject.SetActive(true);
                        StartCoroutine(transform.parent.parent.FindChild("Difficulty").FindChild("PlayNormal").GetComponent<Menu>().select(true));
                        //GlobalHelper.LoadLevel(1, GlobalHelper.Difficulty.EASY);
                        break;

                    case "Quit":
                        Application.Quit();
                        break;
                    //Difficulty items
                    case "PlayEasy":
                        GlobalHelper.LoadLevel(1, GlobalHelper.Difficulty.EASY);
                        break;
                    case "PlayNormal":
                        GlobalHelper.LoadLevel(1, GlobalHelper.Difficulty.NORMAL);
                        break;
                    case "PlayHard":
                        GlobalHelper.LoadLevel(1, GlobalHelper.Difficulty.HARD);
                        break;
                    case "PlayLunatic":
                        GlobalHelper.LoadLevel(1, GlobalHelper.Difficulty.LUNATIC);
                        break;
                    //Game menu items
                    case "Resume":
                        GlobalHelper.paused = false;
                        GlobalHelper.GetPlayer().GetComponent<PlayerMovement>().UpdateFocused();
                        transform.parent.gameObject.SetActive(false); //The "Pause Camvas" canvas
                        break;
                    case "Restart":
                        GlobalHelper.LoadLevel(GlobalHelper.level, GlobalHelper.difficulty);
                        break;
                    case "Title":
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("menu");
                        break;
                    default:

                        break;
                }
            }
        }
    }

    private IEnumerator deselect() {
        //animation shit
        GetComponent<Outline>().effectColor = Color.black;
        yield return null;
    }

    /// <summary>
    /// Selects an object, and if it can't be selected, it goes to next one (which direction is specified by "fromPrev": true: to next, false: to prev
    /// </summary>
    public IEnumerator select(bool fromPrev) {
        if (selectable) {
            selectedObject = transform;
            //animation shit
            GetComponent<Outline>().effectColor = Color.white;
            yield return null;
        } else {
            if (fromPrev) {
                StartCoroutine(nextObject.GetComponent<Menu>().select(true));
                yield return null;
            } else {
                StartCoroutine(prevObject.GetComponent<Menu>().select(false));
                yield return null;
            }
        }
    }

    private bool isSelected() {
        return selectedObject == transform;
    }
}
