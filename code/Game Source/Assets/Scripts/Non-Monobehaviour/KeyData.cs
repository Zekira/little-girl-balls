using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Converts the eight different inputs bool on/off's (WASD bomb shoot focus skip) into a single byte, and back.
/// This is to compress the wide range of possible keycodes into the 8 used ones in gameplay, independent from settings.
/// </summary>
public static class KeyData {

    /// <summary>
    /// Converts 8 bools into a byte, specifically bools about input.
    /// Bit order: skip / focus / bomb / shoot / down / up / right / left
    /// </summary>
    public static byte InputToByte(bool left, bool right, bool up, bool down, bool shoot, bool bomb, bool focus, bool skip) {
        byte returnByte = 0;
        returnByte = NumberFunctions.SetBit(returnByte, 0, left);
        returnByte = NumberFunctions.SetBit(returnByte, 1, right);
        returnByte = NumberFunctions.SetBit(returnByte, 2, up);
        returnByte = NumberFunctions.SetBit(returnByte, 3, down);
        returnByte = NumberFunctions.SetBit(returnByte, 4, shoot);
        returnByte = NumberFunctions.SetBit(returnByte, 5, bomb);
        returnByte = NumberFunctions.SetBit(returnByte, 6, focus);
        returnByte = NumberFunctions.SetBit(returnByte, 7, skip);
        return returnByte;
    }

    /// <summary>
    /// The input has to be exactly 8 bools. I'm assuming this to be true because I'm not working with bool arrays anywhere else.
    /// </summary>
    public static byte InputToByte(bool[] input) {
        return InputToByte(input[0], input[1], input[2], input[3], input[4], input[5], input[6], input[7]);
    }

    /// <summary>
    /// Converts the current input into a byte about input.
    /// </summary>
    public static byte CurrentKeysDownToByte() {
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
    public static byte Combine(byte a, byte b) {
        bool[] contenta = GetAll(a);
        bool[] contentb = GetAll(b);
        bool[] returnBool = new bool[8];
        for (int i = 0; i < 8; i++) {
            returnBool[i] = contenta[i] || contentb[i];
        }
        return InputToByte(returnBool);
    }

    public static bool GetLeft(byte info) {
        return NumberFunctions.GetBit(info, 0);
    }

    public static bool GetRight(byte info) {
        return NumberFunctions.GetBit(info, 1);
    }

    public static bool GetUp(byte info) {
        return NumberFunctions.GetBit(info, 2);
    }

    public static bool GetDown(byte info) {
        return NumberFunctions.GetBit(info, 3);
    }

    public static bool GetShoot(byte info) {
        return NumberFunctions.GetBit(info, 4);
    }

    public static bool GetBomb(byte info) {
        return NumberFunctions.GetBit(info, 5);
    }

    public static bool GetFocus(byte info) {
        return NumberFunctions.GetBit(info, 6);
    }

    public static bool GetSkip(byte info) {
        return NumberFunctions.GetBit(info, 7);
    }

    public static bool[] GetAll(byte info) {
        bool[] returnBool = new bool[8];
        for (int i = 0; i < 8; i++) {
            returnBool[i] = NumberFunctions.GetBit(info, i);
        }
        return returnBool;
    }
}
