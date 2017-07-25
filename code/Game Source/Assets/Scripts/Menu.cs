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

    private Color activeColor;
    private Color inactiveColor;

    void Awake() {
        activeColor = GetComponent<Text>().color;
        inactiveColor = new Vector4(activeColor.r / 2, activeColor.g / 2, activeColor.b / 2, 0.5f);
        SaveLoad.LoadApplyConfig(); //Just in case
        if (selectedObject == null || selectedObject.gameObject.activeInHierarchy == false) {
            GameObject.FindWithTag("MenuBase").GetComponent<Menu>().Select(false);
        }
    }

    void OnEnable() {
        if (!selectable) {
            GetComponent<Text>().color = inactiveColor;
        } else {
            GetComponent<Text>().color = activeColor;
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
                        WaitForKey(Config.SetKeyLeft, Config.keyLeft);
                        break;
                    case "KeyRight":
                        WaitForKey(Config.SetKeyRight, Config.keyRight);
                        break;
                    case "KeyUp":
                        WaitForKey(Config.SetKeyUp, Config.keyUp);
                        break;
                    case "KeyDown":
                        WaitForKey(Config.SetKeyDown, Config.keyDown);
                        break;
                    case "KeyShoot":
                        WaitForKey(Config.SetKeyShoot, Config.keyShoot);
                        break;
                    case "KeyBomb":
                        WaitForKey(Config.SetKeyBomb, Config.keyBomb);
                        break;
                    case "KeyFocus":
                        WaitForKey(Config.SetKeyFocus, Config.keyFocus);
                        break;
                    case "KeySkip":
                        WaitForKey(Config.SetKeySkip, Config.keySkip);
                        break;
                    case "KeyPause":
                        WaitForKey(Config.SetKeyPause, Config.keyPause);
                        break;
                    case "KeyRestart":
                        WaitForKey(Config.SetKeyRestart, Config.keyRestart);
                        break;
                    case "KeyDefault":
                        Config.SetDefaultKeys();
                        GoBack(true);
                        ToMenu("KeyConfig", true);
                        break;
                    //Difficulty items
                    case "PlayEasy":
                        GlobalHelper.difficulty = GlobalHelper.Difficulty.EASY;
                        ToMenu("Characters", false);
                        break;
                    case "PlayNormal":
                        GlobalHelper.difficulty = GlobalHelper.Difficulty.NORMAL;
                        ToMenu("Characters", false);
                        break;
                    case "PlayHard":
                        GlobalHelper.difficulty = GlobalHelper.Difficulty.HARD;
                        ToMenu("Characters", false);
                        break;
                    case "PlayLunatic":
                        GlobalHelper.difficulty = GlobalHelper.Difficulty.LUNATIC;
                        ToMenu("Characters", false);
                        break;
                    //Character items
                    case "Char1":
                        GlobalHelper.character = GlobalHelper.Character.RACHEL_A;
                        previousSelectedMenuItems = new List<Transform>();
                        GlobalHelper.LoadLevel(GlobalHelper.level, GlobalHelper.difficulty);
                        break;
                    case "Char2":
                        GlobalHelper.character = GlobalHelper.Character.RACHEL_B;
                        previousSelectedMenuItems = new List<Transform>();
                        GlobalHelper.LoadLevel(GlobalHelper.level, GlobalHelper.difficulty);
                        break;
                    case "Char3":

                        break;
                    case "Char4":

                        break;
                    case "Char5":

                        break;
                    case "Char6":

                        break;
                    //Game menu items
                    case "Resume":
                        previousSelectedMenuItems = new List<Transform>();
                        GlobalHelper.SetPaused(false);
                        break;
                    case "Restart":
                        previousSelectedMenuItems = new List<Transform>();
                        GlobalHelper.LoadLevel(GlobalHelper.level, GlobalHelper.difficulty);
                        break;
                    case "Title":
                        previousSelectedMenuItems = new List<Transform>();
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
        StartCoroutine(CoSelect(fromPrev));
    }

    /// <summary>
    /// Selects an object, and if it can't be selected, it goes to next one (which direction is specified by "fromPrev": true: to next, false: to prev
    /// </summary>
    private IEnumerator CoSelect(bool fromPrev) {
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
            if (parent.GetChild(i).GetComponent<Menu>() != null) {
                parent.GetChild(i).GetComponent<Menu>().Deselect();
            }
        }
        for (int i = 0; i < parent.childCount; i++) {
            if (parent.GetChild(i).tag == "MenuBase") {
                if (parent.GetChild(i).GetComponent<Menu>() != null) {
                    parent.GetChild(i).GetComponent<Menu>().Select(true);
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
    /// Call function methodToCall() when any keyboard key is pressed. AllowedKey is a specifically allowed key that skips the 'is this used by something else' check.
    /// </summary>
    public void WaitForKey(System.Action<Transform, KeyCode, bool> methodToCall, KeyCode allowedKey) {
        StartCoroutine(CoWaitForKey(methodToCall, allowedKey));
    }

    /// <summary>
    /// Call function methodToCall() when any keyboard key is pressed. AllowedKey is a specifically allowed key that skips the 'is this used by something else' check.
    /// </summary>
    private IEnumerator CoWaitForKey(System.Action<Transform, KeyCode, bool> methodToCall, KeyCode allowedKey) {
        connectedTransform.GetComponent<Text>().text = StringFetcher.GetString("WAITFORINPUT");
        checkingKeyInput = true;
        yield return null;
        while (checkingKeyInput) {
            for (int i = 0; i <= 320/*Amount of different things in the keycode enum*/; i++) {
                if (Input.GetKeyDown((KeyCode)i)) {
                    if (i == (int)Config.keyLeft || i == (int)Config.keyRight || i == (int)Config.keyUp || i == (int)Config.keyDown || i == (int)Config.keyShoot ||
                        i == (int)Config.keyBomb || i == (int)Config.keyPause || i == (int)Config.keySkip || i == (int)Config.keyRestart || i == (int)Config.keyFocus) {
                        connectedTransform.GetComponent<Text>().text = StringFetcher.GetString("KEYINUSE");
                        if (i == (int)allowedKey) {
                            methodToCall(connectedTransform, (KeyCode)i, true);
                            checkingKeyInput = false;
                            break;
                        }
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
