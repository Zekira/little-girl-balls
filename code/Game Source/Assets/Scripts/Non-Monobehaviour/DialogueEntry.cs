using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class representing a piece of dialogue and the speaker.
/// Did things slightly wrong and now a lot of classes uses the character and some the emotion enums.
/// </summary>
public class DialogueEntry {
    public enum Emotion { ANNOYED, HAPPY, ALERT, THINKING, NEUTRAL, TIRED, SURPRISED, EMBARRASED };
    public enum Character { RACHEL, CHARNO};
    public Emotion currentEmotion;
    public Character currentCharacer;
    public string[] special;
    public string text;
    public bool leftSpeaking = true;
    public bool showOther = true;

    public DialogueEntry() { }
    
    public DialogueEntry(string t, Character c, Emotion e, bool leftSpeaks, bool displayOther) {
        text = t;
        currentCharacer = c;
        currentEmotion = e;
        leftSpeaking = leftSpeaks;
        showOther = displayOther;
    }

    public DialogueEntry(string toParse) {
        DialogueEntry newEntry = ParseLine(toParse);
        currentCharacer = newEntry.currentCharacer;
        currentEmotion = newEntry.currentEmotion;
        leftSpeaking = newEntry.leftSpeaking;
        showOther = newEntry.showOther;
        text = newEntry.text;
    }

    public DialogueEntry(DialogueEntry entry) {
        text = entry.text;
        currentCharacer = entry.currentCharacer;
        currentEmotion = entry.currentEmotion;
    }

    /// <summary>
    /// Parses text written in the following format:
    /// character|
    /// emotion|
    /// who is speaking: "left" or "right"|
    /// whether to show the other side: "true" or "false"|
    /// [all text] |
    /// other modifiers seperated by |, such as "timelinetick", 
    /// </summary>
    private static DialogueEntry ParseLine(string text) {
        DialogueEntry returnEntry = new DialogueEntry();
        string[] info = text.Split('|');
        returnEntry.currentCharacer = (Character)System.Enum.Parse(typeof(Character), info[0].ToUpperInvariant().Replace("\n","").Replace("\r",""));
        returnEntry.currentEmotion = (Emotion)System.Enum.Parse(typeof(Emotion), info[1].ToUpperInvariant().Replace("\n", "").Replace("\r", ""));
        returnEntry.leftSpeaking = info[2].ToUpperInvariant().Replace("\n", "").Replace("\r", "") == "LEFT" ? true : false;
        returnEntry.showOther = info[3].ToUpperInvariant().Replace("\n", "").Replace("\r", "") == "TRUE" ? true : false;
        returnEntry.text = info[4].Replace("NEWLINE", System.Environment.NewLine);
        returnEntry.special = new string[info.Length-5];
        for (int i = 0; i < info.Length-5; i++) {
            returnEntry.special[i] = info[i + 5];
        }

        return returnEntry;
    }

    /// <summary>
    /// Parses a list of lines of dialogue seperated by newlines
    /// </summary>
    public static List<DialogueEntry> ParseFile(string path) {
        string content = SaveLoad.TryReadTextFile(path);
        if (content == null) {
            content = (Resources.Load(path) as TextAsset).text;
        }
        List<DialogueEntry> returnList = new List<DialogueEntry>();
        foreach (string line in content.Split(new char[] {'\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries)) {
            returnList.Add(ParseLine(line));
        }
        return returnList;
    }
}
