using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is to compress the wide range of possible keycodes into the 8 used ones in gameplay, independent from settings.
/// </summary>
public static class KeyData {

    public static readonly bool[][] keys =
        { new bool[] { true, false, false, false, false, false, false, false },
          new bool[] { false, true, false, false, false, false, false, false },
          new bool[] { false, false, true, false, false, false, false, false },
          new bool[] { false, false, false, true, false, false, false, false },
          new bool[] { false, false, false, false, true, false, false, false },
          new bool[] { false, false, false, false, false, true, false, false },
          new bool[] { false, false, false, false, false, false, true, false },
          new bool[] { false, false, false, false, false, false, false, true } };

    /// <summary>
    /// Converts 8 bools into a a bool array, about input.
    /// Order: left / right / up / down / shoot / bomb / focus / skip
    /// </summary>
    public static bool[] InputToByte(bool left, bool right, bool up, bool down, bool shoot, bool bomb, bool focus, bool skip) {
        return new bool[] { left, right, up, down, shoot, bomb, focus, skip};
    }

    /// <summary>
    /// Converts the current input into a byte about input.
    /// Order: left / right / up / down / shoot / bomb / focus / skip
    /// </summary>
    public static bool[] CurrentKeysDownToByte() {
        return InputToByte(
            Input.GetKeyDown(Config.keyLeft),
            Input.GetKeyDown(Config.keyRight),
            Input.GetKeyDown(Config.keyUp),
            Input.GetKeyDown(Config.keyDown),
            Input.GetKeyDown(Config.keyShoot),
            Input.GetKeyDown(Config.keyBomb),
            Input.GetKeyDown(Config.keyFocus),
            Input.GetKeyDown(Config.keySkip));
    }

    /// <summary>
    /// Returns the byte where all bits are 0 if both a and b are 0, and 1 otherwise.
    /// </summary>
    public static bool[] Combine(bool[] a, bool[] b) {
        return new bool[] { a[0] || b[0], a[1] || b[1], a[2] || b[2], a[3] || b[3], a[4] || b[4], a[5] || b[5], a[6] || b[6], a[7] || b[7] };
    }

    public static bool GetLeft(bool[] info) {
        return info[0];
    }

    public static bool GetRight(bool[] info) {
        return info[1];
    }

    public static bool GetUp(bool[] info) {
        return info[2];
    }

    public static bool GetDown(bool[] info) {
        return info[3];
    }

    public static bool GetShoot(bool[] info) {
        return info[4];
    }

    public static bool GetBomb(bool[] info) {
        return info[5];
    }

    public static bool GetFocus(bool[] info) {
        return info[6];
    }

    public static bool GetSkip(bool[] info) {
        return info[7];
    }

    public static int GetKeyIndex(KeyCode key) {
        if (key == Config.keyLeft) {
            return 0;
        }
        if (key == Config.keyRight) {
            return 1;
        }
        if (key == Config.keyUp) {
            return 2;
        }
        if (key == Config.keyDown) {
            return 3;
        }
        if (key == Config.keyShoot) {
            return 4;
        }
        if (key == Config.keyBomb) {
            return 5;
        }
        if (key == Config.keyFocus) {
            return 6;
        }
        if (key == Config.keySkip) {
            return 7;
        }

        return -1;
    }

    public static KeyCode GetKeyCode(int index) {
        return
            index == 0 ? Config.keyLeft :
            index == 1 ? Config.keyRight :
            index == 2 ? Config.keyUp :
            index == 3 ? Config.keyDown :
            index == 4 ? Config.keyShoot :
            index == 5 ? Config.keyBomb :
            index == 6 ? Config.keyFocus :
            index == 7 ? Config.keySkip :
                        KeyCode.None;
    }
}
