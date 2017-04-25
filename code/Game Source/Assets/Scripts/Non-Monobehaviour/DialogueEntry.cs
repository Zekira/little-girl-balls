using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class representing a piece of dialogue and the speaker.
/// Did things slightly wrong and now a lot of classes uses the character and some the emotion enums.
/// </summary>
public class DialogueEntry {
    public enum emotion { ANNOYED, HAPPY, ALERT, THINKING, NEUTRAL, TIRED, SURPRISED, EMBARRASED };
    public enum character { RACHEL, CHARNO};
    public emotion currentEmotion;
    public character currentCharacer;
    public string text;
    public bool leftSpeaking = true;
    public bool showOther = true;

    public DialogueEntry() { }
    
    public DialogueEntry(string t, character c, emotion e, bool leftSpeaks, bool displayOther) {
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
    /// [all text]
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static DialogueEntry ParseLine(string text) {
        DialogueEntry returnEntry = new DialogueEntry();
        string[] info = text.Split('|');
        returnEntry.currentCharacer = (character)System.Enum.Parse(typeof(character), info[0].ToUpperInvariant().Replace("\n","").Replace("\r",""));
        returnEntry.currentEmotion = (emotion)System.Enum.Parse(typeof(emotion), info[1].ToUpperInvariant().Replace("\n", "").Replace("\r", ""));
        returnEntry.leftSpeaking = info[2].ToUpperInvariant().Replace("\n", "").Replace("\r", "") == "LEFT" ? true : false;
        returnEntry.showOther = info[3].ToUpperInvariant().Replace("\n", "").Replace("\r", "") == "TRUE" ? true : false;
        returnEntry.text = info[4];

        return returnEntry;
    }

    /// <summary>
    /// Parses a list of lines of dialogue seperated by >
    /// </summary>
    public static List<DialogueEntry> ParseFile(string path) { //todo: eventually allow from outside unity
        string content = (Resources.Load(path) as TextAsset).text;
        List<DialogueEntry> returnList = new List<DialogueEntry>();
        foreach (string line in content.Split('>')) {
            returnList.Add(ParseLine(line));
        }
        return returnList;
    }
}
