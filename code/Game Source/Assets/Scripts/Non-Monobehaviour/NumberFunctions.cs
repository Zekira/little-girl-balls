using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for handling numbers and strings of numbers
/// </summary>
public static class NumberFunctions {

    /// <summary>
    /// Turns a string in the format "<x>[.<y>]" into a float
    /// </summary>
    public static float ParseFloat(string value) {
        bool negative = false;
        if (value[0] == '-') { //Remember the minus sign and remove it from the string.
            negative = true;
            value = value.Substring(1);
        }
        float returnValue = 0f;
        int stringLength = value.Length;
        int dotPosition = stringLength; //Defaults to a dot after the entire string.
        //Find the dot position if it's there
        int i;
        for (i = 0; i < stringLength; i++) {
            if (value[i] == '.') {
                dotPosition = i;
                break;
            }
        }
        //Evaluate before the dot. UTF-16 '0' = 48, '9' = 57, so '0' - 48 = 0, and '9' - 48 = 9.
        for (i = 0; i < dotPosition; i++) {
            returnValue += TenPower(value[i] - 48, dotPosition - i - 1);
        }
        //Evaluate behind the dot.
        for (i = dotPosition + 1; i < stringLength; i++) {
            returnValue += TenPower(value[i] - 48, dotPosition - i);
        }
        if (negative) {
            returnValue = 0 - returnValue;
        }
        return returnValue;
    }

    /// <summary>
    /// Returns f * 10^power.
    /// </summary>
    private static float TenPower(float f, int power) {
        if (power > 0) {
            for (int i = 0; i < power; i++) {
                f *= 10f;
            }
            return f;
        } else {
            for (int i = 0; i < -power; i++) {
                f *= 0.1f;
            }
            return f;
        }
    }

    /// <summary>
    /// Returns true if a string contains any of [a-zA-Z]
    /// </summary>
    public static bool ContainsLetters(string toEvaluate) {
        foreach (char c in toEvaluate) {
            if (c >= 65 && c <= 122) { //All letters in UTF-16 from A to Z to a to z
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Adds commas to numbers. 1234567 -> 1,234,567
    /// </summary>
    public static string Commafy(long number) {
        string returnString = number.ToString();
        int length = returnString.Length;
        for (int i = 3; i < length; i += 3) {
            returnString = returnString.Insert(length - i, ",");
        }
        return returnString;
    }

    /// <summary>
    /// Adds commas to numbers. 1234567 -> 1,234,567
    /// </summary>
    public static string Commafy(ulong number) {
        string returnString = number.ToString();
        int length = returnString.Length;
        for (int i = 3; i < length; i += 3) {
            returnString = returnString.Insert(length - i, ",");
        }
        return returnString;
    }

    /// <summary>
    /// Sets a bit in a byte to What. Index counts from lsb to msb.
    /// </summary>
    public static byte SetBit(byte b, int index, bool what) {
        if (index < 0 || index >= 8) {
            Debug.LogError("Can't modify a byte with index " + index + "; Not [0,8)");
            return b;
        }
        if (what) {
            return (byte)(b | (1 << index));
        } else {
            return (byte)(b & ~(1 << index));
        }
    }

    /// <summary>
    /// Gets a bit in a byte. Index counts from lsb to msb.
    /// </summary>
    public static bool GetBit(byte b, int index) {
        if (index < 0 || index >= 8) {
            Debug.LogError("Can't read a byte with index " + index + "; Not [0,8)");
            return false;
        }
        return ((b >> index) & 1) == 1;
    }

    /// <summary>
    /// Turn (up to) 8 different bits into a byte.
    /// </summary>
    public static byte BoolsToByte(bool[] bools) {
        byte returnByte = 0;
        for (int i = 0; i < 8; i++) {
            returnByte = SetBit(returnByte, i, bools[i]);
        }
        return returnByte;
    }

    public static bool[] ByteToBools(byte b) {
        bool[] returnBool = new bool[8];
        for (int i = 0; i < 8; i++) {
            returnBool[i] = GetBit(b, i);
        }
        return returnBool;
    }
}
