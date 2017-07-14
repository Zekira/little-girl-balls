using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class SaveLoad {
    private static string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) +
        Path.DirectorySeparatorChar + "TouaoiiProject" + Path.DirectorySeparatorChar + "DisembodimentOfTheTealAngel" + Path.DirectorySeparatorChar;
    private static string spellcardHistoryPath = basePath + "SpellcardHistories.dat";
    private static string configPath = basePath + "Config.dat";
    private static string playerDataPath = basePath + "PlayerData.dat";

    /// <summary>
    /// Saves spellcard histories. 'histories' should be a multiple of four long because the safe format goes as following:
    /// 1) Numeric identifier of the spellcard; 2) Numeric character/shot ID 3) Successes 4) Attempts. So if spellcard 2 with Rachel A has a history of 13/37, it would be {2, 1, 13, 37}.
    /// </summary>
    public static void SaveSpellcardHistories(List<short> histories) {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist

        if ((histories.Count & 3) != 0) {
            Debug.LogError("Tried to save faulty spellcard history data!");
            return;
        }
        using (BinaryWriter writer = new BinaryWriter(File.Open(spellcardHistoryPath, FileMode.Create))) {
            for (int i = 0; i < histories.Count; i += 4) { //instead of i++ this to make clear they really should be in groups of four
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
            long length = new FileInfo(spellcardHistoryPath).Length;
            if ((length & 7) != 0) { //short is two bytes, and there are 4 shorts per entry, so the length (in bytes) needs to be divisable by 8
                Debug.LogError("Tried to read faulty spellcard history data!");
                return new List<short>();
            }
            for (int i = 0; i < length / 2; i++) {
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

    /* File format:
     * Global stuff: (Totalling 13 bytes)
     * Time played (uint) = 4 bytes
     * Bullets seen (ulong) = 8 bytes
     * Music heard (byte: 1 = stage 1 track, 2 = stage 1 track + boss, 3 is up and including to stage 2 track, etc. 12 = up to and including stage 6 boss, 13 = + credits, 14 = ex stage, 15 = ex boss) = 1 byte
     * For each player: (Totalling 77 bytes per player = 462)
     *      Highest unlocked stage (byte: 1 through 7) (Manages stage practice unlocks / extra stage unlock) = 1 byte
     *      Main game attempt count (short x4) = 4*2 bytes
     *      Main game clear count
     *      Extra attempt count
     *      Extra clear count
     *      Highscore (ulong) = 8 bytes
     *      Stage 1 through 7 highscore (ulong x7) = 7*8 bytes
     *      Time played (uint) = 4 bytes
     * //Total history
     * 
     */
    private const int playerDataPlayerSize = 77; //the size of the data in bytes a single player block uses
    public static void SavePlayerData(GlobalHelper.Character character) {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        using (BinaryWriter writer = new BinaryWriter(File.Open(playerDataPath, FileMode.OpenOrCreate))) {
            long length = new FileInfo(playerDataPath).Length;
            while (length < 475) { //Fill up with zero's if it's not full. The file should be 6*playerDataPlayerSize + global stuff size bytes
                writer.Seek((int)length, SeekOrigin.Begin);
                writer.Write((byte)0);
                length++;
            }
            writer.Seek(0, SeekOrigin.Begin);
            //Global stuff
            writer.Write(PlayerStats.totalTimePlayed);
            writer.Write(GlobalHelper.totalFiredBullets + GlobalHelper.previousFiredBullets);
            writer.Write(GlobalHelper.musicHeard);

            writer.Seek((int)character * playerDataPlayerSize, SeekOrigin.Current);
            //Per player stuff
            writer.Write(GlobalHelper.bestUnlockedStage);
            writer.Write(GlobalHelper.mainAttempts);
            writer.Write(GlobalHelper.mainFinishes);
            writer.Write(GlobalHelper.extraAttempts);
            writer.Write(GlobalHelper.extraFinishes);
            writer.Write(PlayerStats.highscore);
            foreach (ulong score in PlayerStats.stageHighScore) {
                writer.Write(score);
            }
            writer.Write(PlayerStats.timePlayed);
        }

    }

    public static void LoadPlayerData(GlobalHelper.Character character) {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (!File.Exists(playerDataPath)) {
            PlayerStats.totalTimePlayed = 0;
            GlobalHelper.totalFiredBullets = 0;
            GlobalHelper.previousFiredBullets = 0;
            GlobalHelper.musicHeard = 0;

            GlobalHelper.bestUnlockedStage = 0;
            GlobalHelper.mainAttempts = 0;
            GlobalHelper.mainFinishes = 0;
            GlobalHelper.extraAttempts = 0;
            GlobalHelper.extraFinishes = 0;
            PlayerStats.highscore = 0;
            for (int i = 0; i < PlayerStats.stageHighScore.Length; i++) {
                PlayerStats.stageHighScore[i] = 0;
            }
            PlayerStats.timePlayed = 0;
            return;
        }

        using (BinaryReader reader = new BinaryReader(File.OpenRead(playerDataPath))) {
            PlayerStats.totalTimePlayed = reader.ReadUInt32();
            GlobalHelper.previousFiredBullets = reader.ReadUInt64();
            GlobalHelper.totalFiredBullets = 0;
            GlobalHelper.musicHeard = reader.ReadByte();

            reader.BaseStream.Seek(playerDataPlayerSize * (int)character, SeekOrigin.Current); //stackoverflow says it's usually save. But when things bug, look here first.

            GlobalHelper.bestUnlockedStage = reader.ReadByte();
            GlobalHelper.mainAttempts = reader.ReadInt16();
            GlobalHelper.mainFinishes = reader.ReadInt16();
            GlobalHelper.extraAttempts = reader.ReadInt16();
            GlobalHelper.extraFinishes = reader.ReadInt16();
            PlayerStats.highscore = reader.ReadUInt64();
            for (int i = 0; i < PlayerStats.stageHighScore.Length; i++) {
                PlayerStats.stageHighScore[i] = reader.ReadUInt64();
            }
            PlayerStats.timePlayed = reader.ReadUInt32();

        }
    }
}
