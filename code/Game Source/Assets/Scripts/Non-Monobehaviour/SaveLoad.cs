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
     * Music Volume (byte x2)
     * Other Volume
     * Fullscreen (bool)
     * //Language (??) TODO, later
     */
    public static void SaveConfig() {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        using (BinaryWriter writer = new BinaryWriter(File.Open(configPath, FileMode.Create))) {
            writer.Write((short)Config.keyPause);
            writer.Write((short)Config.keyFocus);
            writer.Write((short)Config.keyShoot);
            writer.Write((short)Config.keyBomb);
            writer.Write((short)Config.keyLeft);
            writer.Write((short)Config.keyRight);
            writer.Write((short)Config.keyUp);
            writer.Write((short)Config.keyDown);
            writer.Write((short)Config.keySkip);
            writer.Write((short)Config.keyRestart);
            writer.Write(Config.musicVolume);
            writer.Write(Config.otherVolume);
            writer.Write(Config.defaultFullscreen);
        }
    }

    public static void LoadApplyConfig() {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (!File.Exists(configPath)) { //set to defaults if it doesn't exist
            Config.SetDefault();
            return;
        }

        using (BinaryReader reader = new BinaryReader(File.OpenRead(configPath))) {
            Config.SetKeyPause(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeyFocus(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeyShoot(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeyBomb(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeyLeft(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeyRight(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeyUp(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeyDown(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeySkip(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetKeyRestart(null, (KeyCode)reader.ReadInt16(), false);
            Config.SetMusicVolume(null, reader.ReadByte(), false);
            Config.SetOtherVolume(null, reader.ReadByte(), false);
            Config.SetFullscreen(null, reader.ReadBoolean(), false);
        }
        SaveConfig();
    }
}
