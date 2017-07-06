using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class SaveLoad {
    private static string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) +
        Path.DirectorySeparatorChar + "TouaoiiProject" + Path.DirectorySeparatorChar + "DisembodimentOfTheTealAngel" + Path.DirectorySeparatorChar;
    private static string spellcardHistoryPath = basePath + "SpellcardHistories.dat";
    private static string configPath = basePath + "Config.dat";

    public static void test() {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
    }

    /// <summary>
    /// Saves spellcard histories. 'histories' should be a multiple of four long because the safe format goes as following:
    /// 1) Numeric identifier of the spellcard; 2) Numeric character/shot ID 3) Successes 4) Attempts. So if spellcard 2 with Rachel A has a history of 13/37, it would be {2, 1, 13, 37}.
    /// </summary>
    public static void SaveSpellcardHistories(List<short> histories) {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist

        if (histories.Count % 4 != 0) {
            Debug.LogError("Tried to save faulty spellcard history data!");
            return;
        }
        using (BinaryWriter writer = new BinaryWriter(File.Open(spellcardHistoryPath, FileMode.Create))) {
            for (int i = 0; i < histories.Count; i += 4) { //instead of i++ this to make clear they really should be in triplets
                writer.Write(histories[i]);
                writer.Write(histories[i + 1]);
                writer.Write(histories[i + 2]);
                writer.Write(histories[i + 3]);
            }
        }
    }

    /// <summary>
    ///  Loads spellcard histories as defined by SaveSpellcardHistories().
    /// </summary>
    public static List<short> LoadSpellcardHistories() {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (!File.Exists(spellcardHistoryPath)) {
            SaveSpellcardHistories(new List<short>());
        }

        using (BinaryReader reader = new BinaryReader(File.OpenRead(spellcardHistoryPath))) {
            List<short> returnShort = new List<short>();
            if (returnShort.Count % 4 != 0) {
                Debug.LogError("Tried to read faulty spellcard history data!");
                return new List<short>();
            }
            for (int i = 0; i < new FileInfo(spellcardHistoryPath).Length / 2; i++) {
                returnShort.Add(reader.ReadInt16());
            }
            return returnShort;
        }
    }

    /* File format:
     * KeyPause id (short x10, 1(incl) through 133(incl), others are not keyboard)
     * KeyFocus id
     * KeyShoot id
     * KeyBomb id
     * KeyLeft id
     * KeyRight id
     * KeyUp id
     * KeyDown id
     * KeySkip id
     * KeyRestart id
     * Music Volume (float x2)
     * Other Volume
     * Fullscreen (bool)
     * //Language (??) TODO, later
     */
    public static void SaveConfig(List<int> settings) {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        using (BinaryWriter writer = new BinaryWriter(File.Open(configPath, FileMode.Create))) {
            writer.Write((short)PlayerMovement.keyPause);
            writer.Write((short)PlayerMovement.keyFocus);
            writer.Write((short)PlayerMovement.keyShoot);
            writer.Write((short)PlayerMovement.keyBomb);
            writer.Write((short)PlayerMovement.keyLeft);
            writer.Write((short)PlayerMovement.keyRight);
            writer.Write((short)PlayerMovement.keyUp);
            writer.Write((short)PlayerMovement.keyDown);
            writer.Write((short)PlayerMovement.keySkip);
            writer.Write((short)PlayerMovement.keyRestart);
            writer.Write(GlobalHelper.MusicVolume);
            writer.Write(GlobalHelper.OtherVolume);
            writer.Write(GlobalHelper.defaultFullscreen);
        }
    }

    public static void LoadApplyConfig() {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (!File.Exists(configPath)) { //set to defaults if it doesn't exist
            PlayerMovement.keyPause = KeyCode.Escape;
            PlayerMovement.keyFocus = KeyCode.LeftShift;
            PlayerMovement.keyShoot = KeyCode.Z;
            PlayerMovement.keyBomb = KeyCode.X;
            PlayerMovement.keyLeft = KeyCode.LeftArrow;
            PlayerMovement.keyRight = KeyCode.RightArrow;
            PlayerMovement.keyUp = KeyCode.UpArrow;
            PlayerMovement.keyDown = KeyCode.DownArrow;
            PlayerMovement.keySkip = KeyCode.LeftControl;
            PlayerMovement.keyRestart = KeyCode.R;
            GlobalHelper.MusicVolume = 0.1f;
            GlobalHelper.OtherVolume = 0.1f;
            GlobalHelper.defaultFullscreen = false;
            return;
        }

        using (BinaryReader reader = new BinaryReader(File.OpenRead(configPath))) {
            PlayerMovement.keyPause = (KeyCode)reader.ReadInt16();
            PlayerMovement.keyFocus = (KeyCode)reader.ReadInt16();
            PlayerMovement.keyShoot = (KeyCode)reader.ReadInt16();
            PlayerMovement.keyBomb = (KeyCode)reader.ReadInt16();
            PlayerMovement.keyLeft = (KeyCode)reader.ReadInt16();
            PlayerMovement.keyRight = (KeyCode)reader.ReadInt16();
            PlayerMovement.keyUp = (KeyCode)reader.ReadInt16();
            PlayerMovement.keyDown = (KeyCode)reader.ReadInt16();
            PlayerMovement.keySkip = (KeyCode)reader.ReadInt16();
            PlayerMovement.keyRestart = (KeyCode)reader.ReadInt16();
            GlobalHelper.MusicVolume = reader.ReadSingle();
            GlobalHelper.OtherVolume = reader.ReadSingle();
            GlobalHelper.defaultFullscreen = reader.ReadBoolean();
        }
    }
}
