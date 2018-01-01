using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class SaveLoad
{
    private static string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) +
        Path.DirectorySeparatorChar + "TouaoiiProject" + Path.DirectorySeparatorChar + "DisembodimentOfTheTealAngel" + Path.DirectorySeparatorChar;
    private static string spellcardHistoryPath = basePath + "SpellcardHistories.dat";
    private static string configPath = basePath + "Config.dat";
    private static string playerDataPath = basePath + "PlayerData.dat";
    private static string replayBasePath = basePath + "Replays" + Path.DirectorySeparatorChar; //Expecting to put things like "replay1.touaoiireplay" after thie replayBasePath
    private static string resourcesBasePath = basePath + "Resources" + Path.DirectorySeparatorChar;

    /// <summary>
    /// Saves spellcard histories. 'histories' should be a multiple of four long because the safe format goes as following:
    /// 1) Numeric identifier of the spellcard; 2) Numeric character/shot ID 3) Successes 4) Attempts. So if spellcard 2 with Rachel A has a history of 13/37, it would be {2, 1, 13, 37}.
    /// </summary>
    public static void SaveSpellcardHistories(List<short> histories) {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (ReplayManager.isReplay) {
            return; //Should NOT be saving ANYTHING during replays!
        }
        if ((histories.Count & 3) != 0) {
            Debug.LogError("[Error] Tried to save faulty spellcard history data!");
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
                Debug.LogError("[Error] Tried to read faulty spellcard history data!");
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
        if (ReplayManager.isReplay) {
            return; //Should NOT be saving ANYTHING during replays!
        }
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
     * Music heard (byte: 1 = stage 1 track, 2 = stage 1 track + boss, 3 is up and including to stage 2 track, etc. 12 = up to and including stage 6 boss, 13 = ex stage, 14 = ex boss, 15 = credits, 16 = ending) = 1 byte
     * For each player: (Totalling 313 bytes per player = 1878)
     *      Highest unlocked stage (byte: 1 through 7) (Manages stage practice unlocks / extra stage unlock) = 1 byte for each difficulty
     *      Main game attempt count (short x4) = 4*2 bytes
     *      Main game clear count
     *      Extra attempt count
     *      Extra clear count
     *      Highscore (ulong) = 8 bytes
     *      Stage 1 through 7 highscore (ulong x7) easy = 7*8 bytes
     *      Stage 1 through 7 highscore on other difficulties (ulong x28) = 7*28 bytes = 224 bytes
     *      Time played (uint) = 4 bytes
     * //Total history
     * 
     */
    private const int playerDataPlayerSize = 313; //the size of the data in bytes a single player block uses
    private const int playerDataGlobalSize = 13;
    public static void SavePlayerData(GlobalHelper.Character character) {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (ReplayManager.isReplay) {
            return; //Should NOT be saving ANYTHING during replays!
        }
        using (BinaryWriter writer = new BinaryWriter(File.Open(playerDataPath, FileMode.OpenOrCreate))) {
            long length = new FileInfo(playerDataPath).Length;
            while (length < playerDataGlobalSize + (6 * playerDataPlayerSize)) { //Fill up with zero's if it's not full. The file should be 6*playerDataPlayerSize + global stuff size bytes
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
            writer.Write(GlobalHelper.bestUnlockedStage[0]);
            writer.Write(GlobalHelper.bestUnlockedStage[1]);
            writer.Write(GlobalHelper.bestUnlockedStage[2]);
            writer.Write(GlobalHelper.bestUnlockedStage[3]);
            writer.Write(GlobalHelper.mainAttempts);
            writer.Write(GlobalHelper.mainFinishes);
            writer.Write(GlobalHelper.extraAttempts);
            writer.Write(GlobalHelper.extraFinishes);
            writer.Write(PlayerStats.highscore);
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 7; j++) {
                    writer.Write(PlayerStats.stageHighScore[i, j]);
                }
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

            GlobalHelper.bestUnlockedStage = new byte[] { 0, 0, 0, 0 };
            GlobalHelper.mainAttempts = 0;
            GlobalHelper.mainFinishes = 0;
            GlobalHelper.extraAttempts = 0;
            GlobalHelper.extraFinishes = 0;
            PlayerStats.highscore = 0;
            for (int i = 0; i < 5; i++) {
                for (int ii = 0; ii < 7; ii++) {
                    PlayerStats.stageHighScore[i, ii] = 0;
                }
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

            GlobalHelper.bestUnlockedStage[0] = reader.ReadByte(); //One for each difficulty
            GlobalHelper.bestUnlockedStage[1] = reader.ReadByte();
            GlobalHelper.bestUnlockedStage[2] = reader.ReadByte();
            GlobalHelper.bestUnlockedStage[3] = reader.ReadByte();
            GlobalHelper.mainAttempts = reader.ReadInt16();
            GlobalHelper.mainFinishes = reader.ReadInt16();
            GlobalHelper.extraAttempts = reader.ReadInt16();
            GlobalHelper.extraFinishes = reader.ReadInt16();
            PlayerStats.highscore = reader.ReadUInt64();
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 7; j++) {
                    PlayerStats.stageHighScore[i, j] = reader.ReadUInt64();
                }
            }
            PlayerStats.timePlayed = reader.ReadUInt32();
        }
    }

    /// <summary>
    /// Returns whether extra is unlocked, which is when any player's bestUnlockedStage[1/2/3] == 7
    /// </summary>
    public static bool HasUnlockedExtra() {
        (new FileInfo(basePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (!File.Exists(playerDataPath)) {
            return false;
        }
        using (BinaryReader reader = new BinaryReader(File.OpenRead(playerDataPath))) {
            for (int i = 0; i < 6; i++) {
                reader.BaseStream.Seek(playerDataGlobalSize /*After the global data*/
                                       + 1 /*Ignore the first byte of this player's data because it's easy best*/
                                       + playerDataPlayerSize * i /*Offset determined by player number*/, SeekOrigin.Begin);
                if (reader.ReadByte() == 7) { //If best normal for this char = 7
                    return true;
                }
                if (reader.ReadByte() == 7) { //If best hard for this char = 7
                    return true;
                }
                if (reader.ReadByte() == 7) { //If best lunatic for this char = 7
                    return true;
                }
            }
        }
        return false;
    }

    /* File format:
     * Replay format (byte) = 1 byte (So different replay types won't clash if I ever were to make more)
     * Replay name (char[32]) = 64 bytes (32 chars and chars are 16-bit)
     * Playertype, difficulty = 1 byte (6 different players and 5 different difficulties = 30 possible things.)
     * Replay flags (byte) = 1 byte (1st bit: NoBomb, 2nd bit: NoMiss, 3rd bit: Pacifist; NN can be derived from the 1st and 2nd)
     * 4 empty ints (4 ints) = 16 bytes (For when new data is needed later, so it won't be incompatible with old replays)
     * Level 1 starting point in this file (int) = 4 bytes (-1 = undefined)
     * Level 2 starting point in this file (int) = 4 bytes (-1 = undefined)
     * ...
     * Level 7 starting point in this file (int) = 4 bytes (-1 = undefined)
     * 
       Leveldata:
     * Levelid (byte) = 1 byte
     * Seed (int) = 4 bytes (There aren't the same results if you skip levels)
     * Playerstartpos x (float) = 4 bytes
     * Playerstartpos y (float) = 4 bytes
     * Playerstart lifepieces (byte) = 1 byte
     * Playerstart bombpieces (byte) = 1 byte
     * Playerstart power (byte) = 1 byte (there are 81 different possible values so it fits within a byte)
     * Playerstart value (int) = 4 bytes
     * Playerstart graze (int) = 4 bytes
     * Score at the end of stage (ulong) = 8 bytes
         Inputs in timestamp order:
     * Timestamp (int) = 4 bytes
     * Key(s) (byte) = 1 byte
     * Duration (int) = 4 bytes
     */
    //Where the binarywriter should be when it should write "Level 1 starting point in this file (int)".
    private const int LevelStartStartIndex = 1/*format*/ + 64/*32 chars*/ + 1/*player/difficulty*/ + 1/*Replay flags*/ + 16/*4 empty ints*/;
    public static void SaveReplay(ReplayData replayData, int index) {
        (new FileInfo(replayBasePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (ReplayManager.isReplay) {
            return; //Should NOT be saving ANYTHING during replays!
        }
        using (BinaryWriter writer = new BinaryWriter(File.Open(replayBasePath + "replay" + index + ".touaoiirpy", FileMode.Create))) {
            writer.Write((byte)0); //Replay format
            for (int i = 0; i < 32; i++) { //Name
                writer.Write((short)replayData.replayName[i]);
            }
            writer.Write(replayData.playerAndDifficulty);
            writer.Write(NumberFunctions.SetBit(
                NumberFunctions.SetBit(
                NumberFunctions.SetBit(new byte(), 0, replayData.noBomb), 1, replayData.noMiss),
                2, replayData.pacifist)); //Set the flags of nobomb / nomiss / pacifist
            writer.Write((long)0); writer.Write((long)0); //The 16 empty bytes for maybe later
            writer.Seek(28, SeekOrigin.Current); //Skip the 7 ints describing where everything is because we don't know that yet.
            int currentLevelLocation = (int)writer.BaseStream.Position; //This starts at LevelStartStartIndex.
            /* What should happen here:
             * It skips the 28 bytes where the locations of level data should be going
             * If leveldata is empty the n'th 4 bytes of those 28 should be "-1"(int).
             * If not, it should be the binarywriter location of where the level data starts (int).*/
            for (int level = 0; level < 7; level++) { //Put in data for all levels.
                if (replayData.inputData[level] == null ||
                    replayData.inputData[level].Count == 0) {
                    //If there's no movement data for this level it obviously shouldn't be in the replay.
                    //So don't do anything with it.
                    currentLevelLocation = (int)writer.BaseStream.Position;
                    writer.Seek(LevelStartStartIndex + level*4 /*each level takes up 4 bytes*/, SeekOrigin.Begin);
                    writer.Write(-1);
                    //Go back to where we were.
                    writer.Seek(currentLevelLocation, SeekOrigin.Begin);
                } else {
                    //There is movement data for this level so it's important.
                    currentLevelLocation = (int)writer.BaseStream.Position;
                    writer.Seek(LevelStartStartIndex + level * 4, SeekOrigin.Begin);
                    //This thing is important, so its position is not -1.
                    writer.Write(currentLevelLocation);
                    //Go back to where we were.
                    writer.Seek(currentLevelLocation, SeekOrigin.Begin);
                    writer.Write((byte)level); //level id
                    writer.Write(replayData.seed[level]); //seed
                    writer.Write(replayData.startpos[level].x); //player start x
                    writer.Write(replayData.startpos[level].y); //player start y
                    writer.Write(replayData.lives[level]); //lives
                    writer.Write(replayData.bombs[level]); //bombs
                    writer.Write(replayData.power[level]); //power
                    writer.Write(replayData.value[level]); //value
                    writer.Write(replayData.graze[level]); //graze
                    writer.Write(replayData.highScores[level]); //score at the end
                    List<InputData> data = replayData.inputData[level];
                    for(int i = 0; i < data.Count; i++) {
                        //All input data (in order because the list is in order)
                        writer.Write(data[i].startingTick);
                        //Writing a SINGLE byte because writing 8 bools uses up 8 bytes of space. And that's just wasteful.
                        writer.Write(NumberFunctions.BoolsToByte(data[i].keys));
                        writer.Write(data[i].duration);
                    }
                }
                //End of the for loop. No need to mess with the writer because the beginning of either branch inside the for loop does that already.
            }
        }
    }

    /// <summary>
    /// Load the replay by name Replays/replay[index].touaoiirpy.
    /// </summary>
    public static ReplayData LoadReplay(int index) {
        (new FileInfo(replayBasePath)).Directory.Create(); //Create the basedirectory if it doesn't exist
        if (!File.Exists(replayBasePath + "replay" + index + ".touaoiirpy")) {
            Debug.LogError("[Error] That replay file doesn't exist; asked index: " + index);
            return new ReplayData();
        }

        ReplayData loadedReplay = new ReplayData();
        using (BinaryReader reader = new BinaryReader(File.OpenRead(replayBasePath + "replay" + index + ".touaoiirpy"))) {
            byte replayFormat = reader.ReadByte();
            if (replayFormat == 0) {
                char[] name = new char[32];
                for (int i = 0; i < 32; i++) {
                    name[i] = (char)reader.ReadInt16();
                }
                Debug.Log("[Replay] " + new string(name));
                loadedReplay.replayName = name;
                byte playerAndDifficulty = reader.ReadByte();
                Debug.Log("[Replay] Character: " + (GlobalHelper.Character)(playerAndDifficulty % 6));
                Debug.Log("[Replay] Difficulty: " + (GlobalHelper.Difficulty)(playerAndDifficulty / 6));
                loadedReplay.playerAndDifficulty = playerAndDifficulty;
                byte flags = reader.ReadByte();
                loadedReplay.noBomb = NumberFunctions.GetBit(flags, 0);
                Debug.Log("[Replay] NoBomb: " + loadedReplay.noBomb);
                loadedReplay.noMiss = NumberFunctions.GetBit(flags, 1);
                Debug.Log("[Replay] NoMiss: " + loadedReplay.noMiss);
                loadedReplay.pacifist = NumberFunctions.GetBit(flags, 2);
                Debug.Log("[Replay] Pacifist: " + loadedReplay.pacifist);
                //Skip the 16 empty bytes
                reader.BaseStream.Seek(16, SeekOrigin.Current);
                //For emphasis, skipped!
                int[] levelStarts = new int[7];
                for (int i = 0; i < 7; i++) {
                    levelStarts[i] = reader.ReadInt32(); //variable to know where to jump to for each level
                    Debug.Log("[Replay] Stage " + i + " byte index: " + levelStarts[i]);
                }
                for (int i = 0; i < 7; i++) {
                    if (levelStarts[i] == -1) {
                        Debug.Log("[Replay] Level " + i + " isn't a level.");
                        continue; //Not actually a level.
                    }
                    Debug.Log("[Replay] <b>Level " + i + " IS a level.</b>");
                    reader.BaseStream.Seek(levelStarts[i], SeekOrigin.Begin);
                    byte levelId = reader.ReadByte();
                    Debug.Log("[Replay] Numeric Level ID: " + levelId);
                    int seed = reader.ReadInt32();
                    Debug.Log("[Replay] Level seed: " + seed);
                    loadedReplay.seed[i] = seed;
                    float pposx = reader.ReadSingle();
                    float pposy = reader.ReadSingle();
                    Debug.Log("[Replay] Player start: " + pposx + "x, " + pposy + "y.");
                    loadedReplay.startpos[i] = new Vector2(pposx, pposy);
                    byte plives = reader.ReadByte();
                    loadedReplay.lives[i] = plives;
                    byte pbombs = reader.ReadByte();
                    loadedReplay.bombs[i] = pbombs;
                    byte ppower = reader.ReadByte();
                    loadedReplay.power[i] = ppower;
                    uint pvalue = reader.ReadUInt32();
                    loadedReplay.value[i] = pvalue;
                    int pgraze = reader.ReadInt32();
                    loadedReplay.graze[i] = pgraze;
                    Debug.Log("[Replay] Player start stats: L: " + plives + " B: " + pbombs + " P: " + ppower + " V: " + pvalue + " G: " + pgraze);
                    loadedReplay.highScores[i] = reader.ReadUInt64();
                    Debug.Log("[Replay] Score for this stage: " + loadedReplay.highScores[i]);
                    /* What should happen here:
                     * Keep looping so long as:
                     * 1) if there is a level after here with higher position, that position is reached
                     * 2) if there is not a level with higher position, until the end of the file
                     */
                    int[] nextLevelPositions = new int[7-i-1]; //If we're at level 2 (i=1), there are 5 more to check (i=2 through i=6)
                    for (int j = i+1; j < 7; j++) {
                        nextLevelPositions[j-i-1] = levelStarts[j]; //Need to scale the j=[i+1,6] to [0,something] because arrays don't start at i+1.
                    }
                    int readerGoal;
                    if (Mathf.Max(nextLevelPositions) != -1) { //The situation where all other levels aren't just -1.
                        readerGoal = Mathf.Max(nextLevelPositions);
                    } else {
                        readerGoal = (int)new FileInfo(replayBasePath + "replay" + index + ".touaoiirpy").Length;
                    }
                    while (reader.BaseStream.Position < readerGoal) {
                        int keyStartTick = reader.ReadInt32();
                        byte keyId = reader.ReadByte();
                        int keyDuration = reader.ReadInt32();
                        Debug.Log("[Replay] Key at time " + keyStartTick + " with duration " + keyDuration + " with id " + keyId);
                        loadedReplay.inputData[i].Add(new InputData(keyStartTick, keyDuration, NumberFunctions.ByteToBools(keyId)));
                    }
                }
            } else {
                Debug.LogError("[Error] Unsupported replay format " + replayFormat);
            }

            return loadedReplay;
        }
    }

    /// <summary>
    /// Returns the text contents of a file if it exists, or null if it doesn't. Relative to Resources.
    /// </summary>
    public static string TryReadTextFile(string path) {
        if (!File.Exists(resourcesBasePath + path + ".txt")) {
            return null;
        }
        Debug.Log(("[Info] Loaded external text from '" + resourcesBasePath + path + ".txt" + "'").Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
        return File.ReadAllText(resourcesBasePath + path + ".txt");
    }
}
