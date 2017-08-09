using UnityEngine;
using System;

/// <summary>
/// A class made to get those huge character portraits in memory for both dialogue and spellcard portraits.
/// </summary>
public class CharacterPortraits : MonoBehaviour {

    private Sprite[] rachelSprites;
    private Sprite[] charnoSprites;

    void Start() {
        LoadSprites(new DialogueEntry.Character[]
            {DialogueEntry.Character.RACHEL,
             DialogueEntry.Character.CHARNO});
    }

    /// <summary>
    /// Gets the sprite associated with a character with an emotion. If it isn't there, it loads it, which is a laggy process which should be avoided here.
    /// </summary>
    public Sprite GetSprite(DialogueEntry.Character character, DialogueEntry.Emotion emotion) {
        switch (character) {
            case DialogueEntry.Character.RACHEL:
                if (rachelSprites == null || rachelSprites.Length != 8) {
                    LoadSprites(new DialogueEntry.Character[] { DialogueEntry.Character.RACHEL });
                }
                return rachelSprites[(int)emotion];
            case DialogueEntry.Character.CHARNO:
                if (charnoSprites == null || charnoSprites.Length != 8) {
                    LoadSprites(new DialogueEntry.Character[] { DialogueEntry.Character.CHARNO });
                }
                return charnoSprites[(int)emotion];
        }
        return null; //Should never happen
    }

    /// <summary>
    /// Loads all sprites of all characters into their respective arrays. Not doing 2D arrays because that looks less nice in code.
    /// </summary>
    public void LoadSprites(DialogueEntry.Character[] characters) {
        foreach (var character in characters) {
            switch (character) {
                case DialogueEntry.Character.RACHEL:
                    rachelSprites = SetSprites(DialogueEntry.Character.RACHEL.ToString());
                    break;
                case DialogueEntry.Character.CHARNO:
                    charnoSprites = SetSprites(DialogueEntry.Character.CHARNO.ToString());
                    break;
            }
        }
    }

    /// <summary>
    /// Sets all 8 emotions of a character as a sprite and returns the array containing those.
    /// </summary>
    private Sprite[] SetSprites(string character) {
        Sprite[] returnArray = new Sprite[8]; //There are ALWAYS at most 8 emotions.
        Texture2D texture;
        for (int i = 0; i < 8; i++) {
            texture = (Texture2D) Resources.Load("Graphics/Characters/" + character + "_" + ((DialogueEntry.Emotion)i).ToString());
            returnArray[i] = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), 0.5f * Vector2.one);
        }
        return returnArray;
    }

    /// <summary>
    /// Makes the arrays belonging to their characters empty.
    /// </summary>
    public void UnloadSprites(DialogueEntry.Character[] characters) {
        foreach (var character in characters) {
            switch (character) {
                case DialogueEntry.Character.RACHEL:
                    rachelSprites = null;
                    break;
                case DialogueEntry.Character.CHARNO:
                    charnoSprites = null;
                    break;
            }
        }
    }
}
