using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Menu : MonoBehaviour {

    public static Transform selectedObject;
    public static int cooldown = 0;

    public Transform nextObject; //To be set in the inspector
    public Transform prevObject; //To be set in the inspector
    public bool selectable = true;
    public static List<Transform> previousSelectedMenuItems = new List<Transform>();

    void Awake() {
        if (selectedObject == null || selectedObject.gameObject.activeInHierarchy == false) {
            GameObject.FindWithTag("MenuBase").GetComponent<Menu>().select(false);
        }
    }
	
	void Update () {
        if (isSelected()) {
            if (cooldown > 0) {
                cooldown--;
            }
            if (Input.GetKey(PlayerMovement.keyBomb)) {
                GoBack(true);
            }
            //transform.position += new Vector3(Random.Range(-1.75f, 1.75f), Random.Range(-1.75f, 1.75f), 0f); //test debug to see if selected
            if (Input.GetKey(PlayerMovement.keyUp) && cooldown <= 0) {
                deselect();
                prevObject.GetComponent<Menu>().select(false);
                cooldown = 10;
            }
            if (Input.GetKey(PlayerMovement.keyDown) && cooldown <= 0) {
                deselect();
                nextObject.GetComponent<Menu>().select(true);
                cooldown = 10;
            }
            if (Input.GetKeyDown(PlayerMovement.keyShoot)) {
                switch (gameObject.name) {
                    //Mostly main menu items
                    case "Play":
                        ToMenu("Difficulty", true);
                        break;
                    case "Options":
                        ToMenu("Settings", true);
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
                        GlobalHelper.player.GetComponent<PlayerMovement>().UpdateFocused();
                        transform.parent.gameObject.SetActive(false); //The "Pause Camvas" canvas
                        break;
                    case "Restart":
                        GlobalHelper.LoadLevel(GlobalHelper.level, GlobalHelper.difficulty);
                        break;
                    case "Title":
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("menu");
                        break;
                    //General Cancel
                    case "Cancel":
                        GoBack(true);
                        break;
                    default:

                        break;
                }
            }
        }
    }

    private void deselect() {
        StartCoroutine(coDeselect());
    }

    private IEnumerator coDeselect() {
        //animation shit
        GetComponent<Outline>().effectColor = Color.black;
        yield return null;
    }

    public void select(bool fromPrev) {
        StartCoroutine(coselect(fromPrev));
    }

    /// <summary>
    /// Selects an object, and if it can't be selected, it goes to next one (which direction is specified by "fromPrev": true: to next, false: to prev
    /// </summary>
    private IEnumerator coselect(bool fromPrev) {
        if (selectable) {
            selectedObject = transform;
            //animation shit
            GetComponent<Outline>().effectColor = Color.white;
            yield return null;
        } else {
            if (fromPrev) {
                nextObject.GetComponent<Menu>().select(true);
                yield return null;
            } else {
                prevObject.GetComponent<Menu>().select(false);
                yield return null;
            }
        }
    }

    private bool isSelected() {
        return selectedObject == transform;
    }

    public void ToMenu(string name, bool hidePrevious) {
        ToMenu(selectedObject.parent.parent.FindChild(name), true);
    }

    public void ToMenu(Transform parent, bool hidePrevious) {
        previousSelectedMenuItems.Add(selectedObject);
        selectedObject.parent.gameObject.SetActive(!hidePrevious);
        parent.gameObject.SetActive(true);
        for (int i = 0; i < parent.childCount; i++) {
            if (parent.GetChild(i).tag == "MenuBase") {
                parent.GetChild(i).GetComponent<Menu>().select(true);
            } else {
                parent.GetChild(i).GetComponent<Menu>().deselect();
            }
        }
    }

    public void GoBack(bool hidePrevious) {
        if (previousSelectedMenuItems.Count > 0) {
            selectedObject.parent.gameObject.SetActive(!hidePrevious);
            previousSelectedMenuItems[previousSelectedMenuItems.Count - 1].parent.gameObject.SetActive(true);
            previousSelectedMenuItems[previousSelectedMenuItems.Count - 1].GetComponent<Menu>().select(true);
            previousSelectedMenuItems.RemoveAt(previousSelectedMenuItems.Count - 1);
        }
    }

    //public IEnumerator WaitForKey(Func<>)
}
