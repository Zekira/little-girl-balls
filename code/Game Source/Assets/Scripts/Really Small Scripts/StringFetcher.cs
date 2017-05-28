using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A class to import the proper text for UI elements.
/// </summary>
public class StringFetcher : MonoBehaviour {

    private static Dictionary<string, string> strings = GetFromFile();

    void Start() {
        if (GetComponent<Text>() != null) {
            string text = GetComponent<Text>().text;
            text = GetString(text);
            GetComponent<Text>().text = text;
        }
    }

    /// <summary>
    /// Puts all strings in Resources/Text/Strings into a dictionary.
    /// </summary>
	private static Dictionary<string, string> GetFromFile () {
        //TODO: From outside of unity.
        Dictionary<string, string> returnDict = new Dictionary<string, string>();
        foreach (string s in ((TextAsset)Resources.Load("Text/Strings")).text.Split(new char[] { '\n', '\r' })) {
            string[] entry = s.Split(new char[] { '=' }, 2);
            if (entry.Length == 2) {
                returnDict.Add(entry[0], entry[1]);
            }
        }
        return returnDict;
	}

    /// <summary>
    /// Returns the string with key "s" if it exists, or itself if it doesn't exist.
    /// </summary>
    public static string GetString(string s) {
        strings.TryGetValue(s, out s);
        return s;
    }
}
