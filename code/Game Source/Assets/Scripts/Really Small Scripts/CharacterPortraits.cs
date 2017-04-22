using UnityEngine;
using System;

/// <summary>
/// A class made to get those huge character portraits in memory.
/// </summary>
public class CharacterPortraits : MonoBehaviour {

    private Sprite[] rachelSprites;
    private Sprite[] charnoSprites;

    void Start() {
        LoadSprites(new DialogueEntry.character[]
            {DialogueEntry.character.RACHEL,
             DialogueEntry.character.CHARNO});
    }

    public Sprite GetSprite(DialogueEntry.character character, DialogueEntry.emotion emotion) {
        switch (character) {
            case DialogueEntry.character.RACHEL:
                if (rachelSprites == null || rachelSprites.Length != 8) {
                    LoadSprites(new DialogueEntry.character[] { DialogueEntry.character.RACHEL });
                }
                return rachelSprites[(int)emotion];
            case DialogueEntry.character.CHARNO:
                if (charnoSprites == null || charnoSprites.Length != 8) {
                    LoadSprites(new DialogueEntry.character[] { DialogueEntry.character.CHARNO });
                }
                return charnoSprites[(int)emotion];
        }
        return null; //Should never happen
    }

    public void LoadSprites(DialogueEntry.character[] characters) {
        foreach (var character in characters) {
            switch (character) {
                case DialogueEntry.character.RACHEL:
                    rachelSprites = SetSprites(DialogueEntry.character.RACHEL.ToString());
                    break;
                case DialogueEntry.character.CHARNO:
                    charnoSprites = SetSprites(DialogueEntry.character.CHARNO.ToString());
                    break;
            }
        }
    }

    private Sprite[] SetSprites(string character) {
        Sprite[] returnArray = new Sprite[8];
        Texture2D texture;
        for (int i = 0; i < 8; i++) {
            texture = (Texture2D) Resources.Load("Graphics/Characters/" + character + "_" + ((DialogueEntry.emotion)i).ToString());
            returnArray[i] = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), 0.5f * Vector2.one);
        }
        return returnArray;
    }

    public void UnloadSprites(DialogueEntry.character[] characters) {
        foreach (var character in characters) {
            switch (character) {
                case DialogueEntry.character.RACHEL:
                    rachelSprites = null;
                    break;
                case DialogueEntry.character.CHARNO:
                    charnoSprites = null;
                    break;
            }
        }
    }
}
