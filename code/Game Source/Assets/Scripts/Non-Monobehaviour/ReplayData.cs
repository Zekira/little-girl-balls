using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO: Multi-level support!
/// <summary>
/// For use in ReplayManager
/// </summary>
public class ReplayData {

    public char[] replayName = new char[32];
    public byte playerAndDifficulty = 0;
    //Can always assume this is in order because new things get added to it after older things. That's how time works.
    public List<InputData>[] inputData = { new List<InputData>(), new List<InputData>(), new List<InputData>(), new List<InputData>(), new List<InputData>(), new List<InputData>(), new List<InputData>() };
    public int[] seed = new int[7];
    public Vector2[] startpos = new Vector2[7];
    public byte[] lives = new byte[7];
    public byte[] bombs = new byte[7];
    public byte[] power = new byte[7];
    public uint[] value = new uint[7];
    public int[] graze = new int[7];

    public void SetPlayerAndDifficulty(GlobalHelper.Character character, GlobalHelper.Difficulty difficulty) {
        playerAndDifficulty = (byte)(((int)character) + 6 * ((int)difficulty));
        //This gives unique ids to every combination. difficulty is multiplied by the amount of characters.
    }
    /// <summary>
    /// Sets the replay's name to the first 32 characters of "name", and fills it up with spaces if "name" is shorter than 32 chars.
    /// </summary>
    public void SetReplayName(string name) {
        for (int i = 0; i < 32; i++) {
            if (i < name.Length) {
                replayName[i] = name[i];
            } else {
                replayName[i] = ' ';
            }
        }
    }

    /// <summary>
    /// Set the player's lives/bombs/power/value/graze on level (0-6).
    /// </summary>
    public void SetPlayerStats(byte lives, byte bombs, byte power, uint value, int graze, int level) {
        this.lives[level] = lives;
        this.bombs[level] = bombs;
        this.power[level] = power;
        this.value[level] = value;
        this.graze[level] = graze;
    }

    /// <summary>
    /// Sets the player's start pos at level (0-6) to be pos.
    /// </summary>
    public void SetStartPos(Vector2 pos, int level) {
        startpos[level] = pos;
    }

    /// <summary>
    /// Puts the RNGesus seed inside the level's (0-6) data.
    /// </summary>
    public void SetSeed(int seed, int level) {
        this.seed[level] = seed;
    }

    /// <summary>
    /// Puts all key data inside the level's (0-6) data.
    /// </summary>
    public void AddInputData(InputData data, int level) {
        inputData[level].Add(data);
    }
}
