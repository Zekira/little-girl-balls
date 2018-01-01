using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Menu : MonoBehaviour {
    /* How to handle this horrible class:
     * Adding something to do when a menu is loaded? In OnEnable()'s last switch
     * Adding something to do when it is selected? In CoSelect()'s switch
     * Adding something to do when it is deselected? In CoDeselect()'s switch
     * Adding something to do when it is pressed? In Update()'s last switch
     * connectedTransform does different things every time and is inconsistent
     */

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
    private bool playSelect = false;

    void Awake() {
        GlobalHelper.SetGameFramerate(false);
        activeColor = GetComponent<Text>().color;
        inactiveColor = new Vector4(activeColor.r / 2, activeColor.g / 2, activeColor.b / 2, 0.5f);
        SaveLoad.LoadApplyConfig(); //Just in case
        if (selectedObject == null || selectedObject.gameObject.activeInHierarchy == false) {
            GameObject.FindWithTag("MenuBase").GetComponent<Menu>().Select(false);
        }
    }

    void OnEnable() {
        if (transform.parent.name == "ExtraCharacters") {
            switch (gameObject.name) {
                case "Char1":
                    SaveLoad.LoadPlayerData(GlobalHelper.Character.RACHEL_A);
                    selectable = Mathf.Max(GlobalHelper.bestUnlockedStage[1], GlobalHelper.bestUnlockedStage[2], GlobalHelper.bestUnlockedStage[3]) == 7;
                    break;
                case "Char2":
                    SaveLoad.LoadPlayerData(GlobalHelper.Character.RACHEL_B);
                    selectable = Mathf.Max(GlobalHelper.bestUnlockedStage[1], GlobalHelper.bestUnlockedStage[2], GlobalHelper.bestUnlockedStage[3]) == 7;
                    break;
                case "Char3":
                    SaveLoad.LoadPlayerData(GlobalHelper.Character.RACHEL_C);
                    selectable = Mathf.Max(GlobalHelper.bestUnlockedStage[1], GlobalHelper.bestUnlockedStage[2], GlobalHelper.bestUnlockedStage[3]) == 7;
                    break;
                case "Char4":
                    SaveLoad.LoadPlayerData(GlobalHelper.Character.WHATEVER_A);
                    selectable = Mathf.Max(GlobalHelper.bestUnlockedStage[1], GlobalHelper.bestUnlockedStage[2], GlobalHelper.bestUnlockedStage[3]) == 7;
                    break;
                case "Char5":
                    SaveLoad.LoadPlayerData(GlobalHelper.Character.WHATEVER_B);
                    selectable = Mathf.Max(GlobalHelper.bestUnlockedStage[1], GlobalHelper.bestUnlockedStage[2], GlobalHelper.bestUnlockedStage[3]) == 7;
                    break;
                case "Char6":
                    SaveLoad.LoadPlayerData(GlobalHelper.Character.WHATEVER_C);
                    selectable = Mathf.Max(GlobalHelper.bestUnlockedStage[1], GlobalHelper.bestUnlockedStage[2], GlobalHelper.bestUnlockedStage[3]) == 7;
                    break;
            }
        }
        switch(gameObject.name) {
            case "Extra":
                selectable = SaveLoad.HasUnlockedExtra();
                break;
            case "PlayStage1":
                SaveLoad.LoadPlayerData(GlobalHelper.character);
                selectable = (PlayerStats.stageHighScore[(int)GlobalHelper.difficulty,0] > 0);
                GetComponent<Text>().text = PlayerStats.stageHighScore[(int)GlobalHelper.difficulty,0] + "   " + StringFetcher.GetString("PLAYSTAGE1");
                break;
            case "PlayStage2":
                SaveLoad.LoadPlayerData(GlobalHelper.character);
                selectable = (PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 1] > 0);
                GetComponent<Text>().text = PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 1] + "   " + StringFetcher.GetString("PLAYSTAGE2");
                break;
            case "PlayStage3":
                SaveLoad.LoadPlayerData(GlobalHelper.character);
                selectable = (PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 2] > 0);
                GetComponent<Text>().text = PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 2] + "   " + StringFetcher.GetString("PLAYSTAGE3");
                break;
            case "PlayStage4":
                SaveLoad.LoadPlayerData(GlobalHelper.character);
                selectable = (PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 3] > 0);
                GetComponent<Text>().text = PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 3] + "   " + StringFetcher.GetString("PLAYSTAGE4");
                break;
            case "PlayStage5":
                SaveLoad.LoadPlayerData(GlobalHelper.character);
                selectable = (PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 4] > 0);
                GetComponent<Text>().text = PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 4] + "   " + StringFetcher.GetString("PLAYSTAGE5");
                break;
            case "PlayStage6":
                SaveLoad.LoadPlayerData(GlobalHelper.character);
                selectable = (PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 5] > 0);
                GetComponent<Text>().text = PlayerStats.stageHighScore[(int)GlobalHelper.difficulty, 5] + "   " + StringFetcher.GetString("PLAYSTAGE6");
                break;
        }
        if (!selectable) {
            GetComponent<Text>().color = inactiveColor;
        } else {
            GetComponent<Text>().color = activeColor;
        }
    }

    void Update () {
        playSelect = true;
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
                AudioManager.QueueSound(AudioManager.SFX.MENU_SELECT);
                cooldown = 8;
                switch (gameObject.name) {
                    //Mostly main menu items
                    case "Play":
                    case "Practice":
                        ToMenu("Difficulty", true);
                        break;
                    case "Extra":
                        ToMenu("ExtraCharacters", true);
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
                    //Level select items
                    case "PlayStage1":
                        SceneSwitcher.LoadLevel(0, GlobalHelper.difficulty, false);
                        break;
                    case "PlayStage2":
                        SceneSwitcher.LoadLevel(1, GlobalHelper.difficulty, false);
                        break;
                    case "PlayStage3":
                        SceneSwitcher.LoadLevel(2, GlobalHelper.difficulty, false);
                        break;
                    case "PlayStage4":
                        SceneSwitcher.LoadLevel(3, GlobalHelper.difficulty, false);
                        break;
                    case "PlayStage5":
                        SceneSwitcher.LoadLevel(4, GlobalHelper.difficulty, false);
                        break;
                    case "PlayStage6":
                        SceneSwitcher.LoadLevel(5, GlobalHelper.difficulty, false);
                        break;
                    //Character items TODOs
                    case "Char1":
                        GlobalHelper.character = GlobalHelper.Character.RACHEL_A;
                        AfterPlayerSelect();
                        break;
                    case "Char2":
                        GlobalHelper.character = GlobalHelper.Character.RACHEL_B;
                        AfterPlayerSelect();
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
                        GlobalHelper.SetPaused(false);
                        break;
                    case "Restart":
                        previousSelectedMenuItems = new List<Transform>();
                        SceneSwitcher.LoadLevel(GlobalHelper.level, GlobalHelper.difficulty, ReplayManager.isReplay);
                        break;
                    case "Title":
                        SceneSwitcher.LoadMenu();
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
            switch(connectedTransform.name) { //When a connected transform needs more than just a color change
                case "Description":
                    connectedTransform.GetComponent<Text>().text = "";
                    break;
            }
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
            if (playSelect) {
                AudioManager.QueueSound(AudioManager.SFX.MENU_CHANGE_SELECTION);
            }
            selectedObject = transform;
            //animation shit
            GetComponent<Outline>().effectColor = Color.white;
            if (connectedTransform != null) {
                connectedTransform.GetComponent<Outline>().effectColor = Color.white;
                switch(connectedTransform.name) { //things needed to be done when just selecting, e.g. displaying information
                    case "Description":
                        connectedTransform.GetComponent<Text>().text = StringFetcher.GetString("DESCRIPTION_" + selectedObject.name.ToUpperInvariant());
                        break;
                    default:

                        break;
                }
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
            AudioManager.QueueSound(AudioManager.SFX.MENU_CANCEL);
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

    private void AfterPlayerSelect() {
        if (previousSelectedMenuItems[previousSelectedMenuItems.Count - 1].name == "Extra") {
            SceneSwitcher.LoadLevel(6, GlobalHelper.Difficulty.EXTRA, false);
        } else if (previousSelectedMenuItems[previousSelectedMenuItems.Count - 2].name == "Play") {
            SceneSwitcher.LoadLevel(GlobalHelper.level, GlobalHelper.difficulty, false);
        } else if (previousSelectedMenuItems[previousSelectedMenuItems.Count - 2].name == "Practice") {
            ToMenu("StageSelect", true);
        }
    }
}
