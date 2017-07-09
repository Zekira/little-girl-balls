using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Menu : MonoBehaviour {

    public static Transform selectedObject;
    public static int cooldown = 0;
    public static int keyConfigCooldown = 0;
    public static bool checkingKeyInput = false;

    public Transform nextObject; //To be set in the inspector
    public Transform prevObject; //To be set in the inspector
    public bool selectable = true;
    public Transform connectedTransform; //To be set in the inspector. What transform is modified by this transform
    public static List<Transform> previousSelectedMenuItems = new List<Transform>();

    void Awake() {
        SaveLoad.LoadApplyConfig(); //Just in case
        if (selectedObject == null || selectedObject.gameObject.activeInHierarchy == false) {
            GameObject.FindWithTag("MenuBase").GetComponent<Menu>().Select(false);
        }
    }
	
	void Update () {
        if (IsSelected()) {
            if (cooldown > 0) {
                cooldown--;
            }
            if (keyConfigCooldown > 0) {
                keyConfigCooldown--;
                return; //Nothing below here should be called if this cooldown is active
            }
            if (checkingKeyInput) {
                return; //Nothing below here should be called if a key is being chosen
            }
            if (Input.GetKeyDown(Config.keyBomb)) {
                GoBack(true);
            }
            //transform.position += new Vector3(Random.Range(-1.75f, 1.75f), Random.Range(-1.75f, 1.75f), 0f); //test debug to see if selected
            if (Input.GetKey(Config.keyUp) && cooldown <= 0) {
                Deselect();
                prevObject.GetComponent<Menu>().Select(false);
                cooldown = 10;
            }
            if (Input.GetKey(Config.keyDown) && cooldown <= 0) {
                Deselect();
                nextObject.GetComponent<Menu>().Select(true);
                cooldown = 10;
            }
            if (Input.GetKey(Config.keyLeft) && cooldown <= 0) {
                switch (gameObject.name) {
                    case "MusicVolume":
                        Config.SetMusicVolume(connectedTransform, (byte)(Config.musicVolume == 0 ? 20 : Config.musicVolume - 1), true);
                        break;
                    case "OtherVolume":
                        Config.SetOtherVolume(connectedTransform, (byte)(Config.otherVolume == 0 ? 20 : Config.otherVolume - 1), true);
                        break;
                    case "Fullscreen":
                        Config.SetFullscreen(connectedTransform, !Config.defaultFullscreen, true);
                        break;
                }
                cooldown = 8;
            }
            if (Input.GetKey(Config.keyRight) && cooldown <= 0) {
                switch (gameObject.name) {
                    case "MusicVolume":
                        Config.SetMusicVolume(connectedTransform, (byte)(Config.musicVolume == 20 ? 0 : Config.musicVolume + 1), true);
                        break;
                    case "OtherVolume":
                        Config.SetOtherVolume(connectedTransform, (byte)(Config.otherVolume == 20 ? 0 : Config.otherVolume + 1), true);
                        break;
                    case "Fullscreen":
                        Config.SetFullscreen(connectedTransform, !Config.defaultFullscreen, true);
                        break;
                }
                cooldown = 8;
            }
            if (Input.GetKeyDown(Config.keyShoot)) {
                cooldown = 8;
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
                    //Options menu
                    case "Controls":
                        ToMenu("KeyConfig", true);
                        break;
                    case "MusicVolume":
                        Config.SetMusicVolume(connectedTransform, (byte)(Config.musicVolume == 20 ? 0 : Config.musicVolume + 1), true);
                        //also done with keyLeft and keyRight
                        break;
                    case "OtherVolume":
                        Config.SetOtherVolume(connectedTransform, (byte)(Config.otherVolume == 20 ? 0 : Config.otherVolume + 1), true);
                        //also with keyLeft and keyRight
                        break;
                    case "Fullscreen":
                        Config.SetFullscreen(connectedTransform, !Config.defaultFullscreen, true);
                        break;
                    //Key Config
                    case "KeyLeft":
                        WaitForKey(Config.SetKeyLeft);
                        break;
                    case "KeyRight":
                        WaitForKey(Config.SetKeyRight);
                        break;
                    case "KeyUp":
                        WaitForKey(Config.SetKeyUp);
                        break;
                    case "KeyDown":
                        WaitForKey(Config.SetKeyDown);
                        break;
                    case "KeyShoot":
                        WaitForKey(Config.SetKeyShoot);
                        break;
                    case "KeyBomb":
                        WaitForKey(Config.SetKeyBomb);
                        break;
                    case "KeyFocus":
                        WaitForKey(Config.SetKeyFocus);
                        break;
                    case "KeySkip":
                        WaitForKey(Config.SetKeySkip);
                        break;
                    case "KeyPause":
                        WaitForKey(Config.SetKeyPause);
                        break;
                    case "KeyRestart":
                        WaitForKey(Config.SetKeyRestart);
                        break;
                    case "KeyDefault":
                        Config.SetDefaultKeys();
                        GoBack(true);
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
                    case "Back":
                        GoBack(true);
                        break;
                    default:

                        break;
                }
            }
        }
    }

    private void Deselect() {
        StartCoroutine(CoDeselect());
    }

    private IEnumerator CoDeselect() {
        //animation shit
        GetComponent<Outline>().effectColor = Color.black;
        if (connectedTransform != null) {
            connectedTransform.GetComponent<Outline>().effectColor = Color.black;
        }
        yield return null;
    }

    public void Select(bool fromPrev) {
        StartCoroutine(Coselect(fromPrev));
    }

    /// <summary>
    /// Selects an object, and if it can't be selected, it goes to next one (which direction is specified by "fromPrev": true: to next, false: to prev
    /// </summary>
    private IEnumerator Coselect(bool fromPrev) {
        if (selectable) {
            selectedObject = transform;
            //animation shit
            GetComponent<Outline>().effectColor = Color.white;
            if (connectedTransform != null) {
                connectedTransform.GetComponent<Outline>().effectColor = Color.white;
            }
            yield return null;
        } else {
            if (fromPrev) {
                nextObject.GetComponent<Menu>().Select(true);
                yield return null;
            } else {
                prevObject.GetComponent<Menu>().Select(false);
                yield return null;
            }
        }
    }

    private bool IsSelected() {
        return selectedObject == transform;
    }

    public void ToMenu(string name, bool hidePrevious) {
        ToMenu(selectedObject.parent.parent.Find(name), true);
    }

    public void ToMenu(Transform parent, bool hidePrevious) {
        previousSelectedMenuItems.Add(selectedObject);
        selectedObject.parent.gameObject.SetActive(!hidePrevious);
        parent.gameObject.SetActive(true);
        for (int i = 0; i < parent.childCount; i++) {
            if (parent.GetChild(i).tag == "MenuBase") {
                if (parent.GetChild(i).GetComponent<Menu>() != null) {
                    parent.GetChild(i).GetComponent<Menu>().Select(true);
                }
            } else {
                if (parent.GetChild(i).GetComponent<Menu>() != null) {
                    parent.GetChild(i).GetComponent<Menu>().Deselect();
                }
            }
        }
    }

    public void GoBack(bool hidePrevious) {
        if (previousSelectedMenuItems.Count > 0) {
            selectedObject.parent.gameObject.SetActive(!hidePrevious);
            previousSelectedMenuItems[previousSelectedMenuItems.Count - 1].parent.gameObject.SetActive(true);
            previousSelectedMenuItems[previousSelectedMenuItems.Count - 1].GetComponent<Menu>().Select(true);
            previousSelectedMenuItems.RemoveAt(previousSelectedMenuItems.Count - 1);
        }
    }

    /// <summary>
    /// Call function methodToCall() when any keyboard key is pressed.
    /// </summary>
    public void WaitForKey(System.Action<Transform, KeyCode, bool> methodToCall) {
        StartCoroutine(CoWaitForKey(methodToCall));
    }

    /// <summary>
    /// Call function methodToCall() when any keyboard key is pressed.
    /// </summary>
    private IEnumerator CoWaitForKey(System.Action<Transform, KeyCode, bool> methodToCall) {
        connectedTransform.GetComponent<Text>().text = StringFetcher.GetString("WAITFORINPUT");
        checkingKeyInput = true;
        yield return null;
        while (checkingKeyInput) {
            for (int i = 0; i <= 320/*Amount of different things in the keycode enum*/; i++) {
                if (Input.GetKeyDown((KeyCode)i)) {
                    if (i == (int)Config.keyLeft || i == (int)Config.keyRight || i == (int)Config.keyUp || i == (int)Config.keyDown || i == (int)Config.keyShoot ||
                        i == (int)Config.keyBomb || i == (int)Config.keyPause || i == (int)Config.keySkip || i == (int)Config.keyRestart || i == (int)Config.keyFocus) {
                        connectedTransform.GetComponent<Text>().text = StringFetcher.GetString("KEYINUSE");
                        continue;
                    }
                    methodToCall(connectedTransform, (KeyCode)i, true);
                    checkingKeyInput = false;
                    break;
                }
            }
            yield return null;
        }
        keyConfigCooldown = 2;
    }
}
