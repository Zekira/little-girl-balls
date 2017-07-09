using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Config {

    public static KeyCode keyPause { get; private set; }// = KeyCode.Escape;
    public static KeyCode keyFocus { get; private set; }// = KeyCode.LeftShift;
    public static KeyCode keyShoot { get; private set; }// = KeyCode.Z;
    public static KeyCode keyBomb { get; private set; }// = KeyCode.X;
    public static KeyCode keyLeft { get; private set; }// = KeyCode.LeftArrow;
    public static KeyCode keyRight { get; private set; }// = KeyCode.RightArrow;
    public static KeyCode keyUp { get; private set; }// = KeyCode.UpArrow;
    public static KeyCode keyDown { get; private set; }// = KeyCode.DownArrow;
    public static KeyCode keySkip { get; private set; }// = KeyCode.LeftControl;
    public static KeyCode keyRestart { get; private set; }// = KeyCode.R;

    public static byte musicVolume { get; private set; } //(Each stands for 5%)
    public static byte otherVolume { get; private set; } //(Each stands for 5%)
    public static bool defaultFullscreen { get; private set; }


    public static void SetDefault() {
        SetDefaultKeys();

        SetMusicVolume(null, 2, false);
        SetOtherVolume(null, 2, false);
        SetFullscreen(null, false, false);
        SaveLoad.SaveConfig();
    }

    public static void SetDefaultKeys() {
        SetKeyPause(null, KeyCode.Escape, false);
        SetKeyFocus(null, KeyCode.LeftShift, false);
        SetKeyShoot(null, KeyCode.Z, false);
        SetKeyBomb(null, KeyCode.X, false);
        SetKeyLeft(null, KeyCode.LeftArrow, false);
        SetKeyRight(null, KeyCode.RightArrow, false);
        SetKeyUp(null, KeyCode.UpArrow, false);
        SetKeyDown(null, KeyCode.DownArrow, false);
        SetKeySkip(null, KeyCode.LeftControl, false);
        SetKeyRestart(null, KeyCode.R, false);

        SaveLoad.SaveConfig();

    }

    public static void SetFullscreen(Transform textTransform, bool fullscreen, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = fullscreen ? StringFetcher.GetString("ON") : StringFetcher.GetString("OFF");
        }
        defaultFullscreen = fullscreen;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetOtherVolume(Transform textTransform, byte amount, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = "◀ " + (5 * amount) + "% ▶";
        }
        otherVolume = amount;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetMusicVolume(Transform textTransform, byte amount, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = "◀ " + (5 * amount) + "% ▶";
        }
        musicVolume = amount;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyPause(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyPause = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyFocus(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyFocus = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyShoot(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyShoot = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyBomb(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyBomb = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyLeft(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyLeft = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyRight(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyRight = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyUp(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyUp = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyDown(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyDown = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeySkip(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keySkip = key;
        if (save) {SaveLoad.SaveConfig();}
    }

    public static void SetKeyRestart(Transform textTransform, KeyCode key, bool save) {
        if (textTransform != null) {
            textTransform.GetComponent<Text>().text = key.ToString();
        }
        keyRestart = key;
        if (save) {SaveLoad.SaveConfig();}
    }
}
