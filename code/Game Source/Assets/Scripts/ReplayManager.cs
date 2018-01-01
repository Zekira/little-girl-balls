using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayManager : MonoBehaviour {

    public static bool isReplay = false; //False during normal gameplay, true during replays.
    private static int[] timer = { 0, 0, 0, 0, 0, 0, 0 }; //One timer for each level.
    private static int[] keylevels = { -1, -1, -1, -1, -1, -1, -1, -1 }; //-1 stands for "no data" and is used to check if there is data from a key that's down.
    private static int[] keytimers = { 0, 0, 0, 0, 0, 0, 0, 0 }; //The count for how long each key is being pressed
    private static int[] keyindices = { 0, 0, 0, 0, 0, 0, 0, 0 }; //The indices of the most recent of each key in currentReplay.inputData

    public static ReplayData currentReplay = new ReplayData();
    public static ReplayManager replayManager;

    private int currentInputDataIndex = 0; //For replays to keep track of how far along we are.
    private List<InputData> inputToCheck = new List<InputData>();
    private List<InputData> currentInput = new List<InputData>(); //During replays to keep track of what's currently happening.
    private PlayerMovement playerMovement;
    private bool[] prevAllKeys = new bool[] { false, false, false, false, false, false, false, false };

    public void Awake() {
        timer = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        keylevels = new int[] { -1, -1, -1, -1, -1 , -1, -1, -1 };
        keytimers = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        keyindices = new int[] {  0, 0, 0, 0, 0, 0, 0, 0 };
        replayManager = this;
        playerMovement = GlobalHelper.player.GetComponent<PlayerMovement>();
        timer[GlobalHelper.level] = 0;
        if (!isReplay) {
            currentReplay.SetPlayerStats(
                (byte)(PlayerStats.lifepieces + PlayerStats.lives * PlayerStats.piecesToLife),
                (byte)(PlayerStats.bombpieces + PlayerStats.bombs * PlayerStats.piecesToBomb),
                (byte)(PlayerStats.power / 5),
                PlayerStats.value,
                PlayerStats.graze, GlobalHelper.level);
            //Check any input down from before the level was started, and thus isn't registered by the Input.GetKeyDown part in Update().
            InterruptAllKeys();
        } else {
            //Playerstats are get in PlayerStat's start
            inputToCheck = currentReplay.inputData[GlobalHelper.level];

            PlayerStats.highscore = ulong.MaxValue; //doesn't get saved anyway
            GlobalHelper.uiVariable.Find("HighScore").GetComponent<Text>().text = StringFetcher.GetString("ACTIVEREPLAY");
            GlobalHelper.canvas.Find("Static Canvas").Find("Nonvariable").Find("Hi-Score").gameObject.SetActive(false);
        }
    }

    private void Update() {
        if (!GlobalHelper.paused) {
            timer[GlobalHelper.level]++; //This way unpressing/pressing keys won't result in unknown behaviour, just zero-length stuff.
        }
        if (!isReplay) {
            for (int i = 0; i < 8; i++) {
                if (Input.GetKeyDown(KeyData.GetKeyCode(i))) {
                    keylevels[i] = GlobalHelper.level;
                    keytimers[i] = 0;
                    currentReplay.AddInputData(new InputData(-1, -1, KeyData.keys[0]), keylevels[i]); //placeholder for when the data should actually be recorded
                    keyindices[i] = currentReplay.inputData[GlobalHelper.level].Count - 1;
                }
                if (!GlobalHelper.paused && Input.GetKey(KeyData.GetKeyCode(i))) { //Same comment as on the timer++; inside the non-pause check.
                    keytimers[i]++; //This triggers in the SAME tick as above! be wary. This causes everything (except menu stuff) to have at least length 1.
                }
                if (Input.GetKeyUp(KeyData.GetKeyCode(i))) {
                    if (keylevels[i] == -1) {
                        /* This should honestly never happen, but it does and I can't find the cause.
                         * If this happens, line 72's keylevels[i] goes wrong for obvious reasons.
                         * Like once every thousand or so keypresses while keyboard mashing this if statement evaluates true.
                         * It usually seems to be about keys that don't last long at all. (One or two ticks)
                         * Everything else about those inputs (keytimers[i], keyindices[i]) seems correct.
                         * So I just set the keylevels[i] to the currentlevel - what's the chance that things go wrong with
                         *  1) This bug triggers 2) at almost EXACTLY a stage transition 3) with gameplay the player wants to save a replay?
                         */
                        keylevels[i] = GlobalHelper.level;
                    }
                    currentReplay.inputData[keylevels[i]][keyindices[i]] = new InputData(timer[keylevels[i]] - keytimers[i], keytimers[i], KeyData.keys[i]);
                    keylevels[i] = -1;
                    //There can be zero-length shit in here but that doesn't really matter.
                }
            }
            if (Input.GetKeyDown(KeyCode.I)) { //TODO: Temp to remove!
                PrepareSaveReplay("This is honestly a 32 char name!", 0);
            }
        }
        if (Input.GetKeyDown(KeyCode.O)) { //TODO: Temp to remove!
            LoadReplayIntoGame(SaveLoad.LoadReplay(0), 0);
        }

        //When in a replay, check for all "inputs". This time, they're not from keyboards, but from the replay.
        if (isReplay && !GlobalHelper.paused) {
            //Check for new input
            while ((currentInputDataIndex < inputToCheck.Count) &&
                (inputToCheck[currentInputDataIndex].startingTick <= timer[GlobalHelper.level])) { //If it's new, it needs to be executed THIS tick.
                currentInput.Add(inputToCheck[currentInputDataIndex]);
                currentInputDataIndex++;
            }

            //Throw away old input
            List<int> indicesToRemove = new List<int>();
            for (int i = currentInput.Count-1; i >= 0; i--) { //Top down because the order of removal matters: removing 1,2,3 gives a different result from 3,2,1, the latter I want.
                if (timer[GlobalHelper.level] >= currentInput[i].startingTick + currentInput[i].duration) { // >= because length 0 should be killed, but length 1 should have 1 tick of time. Inductively length n will have n ticks.
                    indicesToRemove.Add(i);
                }
            }

            foreach(int i in indicesToRemove) {
                currentInput.RemoveAt(i);
            }

            //Combine all keypresses into a single bool array to work with
            bool[] allKeys = new bool[] { false, false, false, false, false, false, false, false };
            foreach(InputData i in currentInput) {
                allKeys = KeyData.Combine(i.keys, allKeys); //This leaves a -1,-1,1 in the replay data but that doesn't matter as it's simply ignored, and needed for further replays.
            }

            //Actually do stuff with the input
            playerMovement.CheckFocus((allKeys[6] && !prevAllKeys[6]), (prevAllKeys[6] && !allKeys[6]));
            if (!PlayerStats.noMovement) {
                playerMovement.CheckBomb(allKeys[5]);
                playerMovement.CheckMove(allKeys[0], allKeys[1], allKeys[2], allKeys[3]);
                playerMovement.CheckShootOrSkip(allKeys[4], allKeys[7], (allKeys[4] && !prevAllKeys[4]));
            }

            prevAllKeys = allKeys;
        }
    }

    public void MakeReplayNewstageCompatible() {
        if (isReplay) {
            inputToCheck = currentReplay.inputData[GlobalHelper.level];
            currentInputDataIndex = 0;
        } else {
            InterruptAllKeys();
        }
        
    }

    //Splits all keys if pressed.
    private static void InterruptAllKeys() {
        for (int i = 0; i < 8; i++) {
            InterruptKeyDown(i);
        }
    }

    /// <summary>
    /// Restarts the game and lets it be played from a replay. Here level is [0-6].
    /// </summary>
    public static void LoadReplayIntoGame(ReplayData replay, int level) {
        currentReplay = replay;
        SceneSwitcher.LoadLevel(level, (GlobalHelper.Difficulty)(replay.playerAndDifficulty / 6), true);
    }

    /// <summary>
    /// If a key is pressed, act as if it were to go up and down again in a single tick. Useful for splitting replays somewhere, e.g. between levels or at the start/end of the game.
    /// </summary>
    public static void InterruptKeyDown(int keyId) {
        //Check for old input, and if it is there, apply it.
        if (keylevels[keyId] != -1) {
            currentReplay.inputData[keylevels[keyId]][keyindices[keyId]] = new InputData(timer[keylevels[keyId]] - keytimers[keyId], keytimers[keyId], KeyData.keys[keyId]);
            keylevels[keyId] = -1;
        }
        //Check if there's still new input, and put it in the to-check list.
        if (Input.GetKey(KeyData.GetKeyCode(keyId))) {
            keylevels[keyId] = GlobalHelper.level;
            keytimers[keyId] = 0;
            currentReplay.AddInputData(new InputData(-1, -1, KeyData.keys[0]), keylevels[keyId]); //placeholder for when the data should actually be recorded
            keyindices[keyId] = currentReplay.inputData[GlobalHelper.level].Count - 1;
        }
    }

    public static void PrepareSaveReplay(string name, int index) {
        currentReplay.SetPlayerAndDifficulty(GlobalHelper.character, GlobalHelper.difficulty);
        currentReplay.SetReplayName(name);
        currentReplay.SetSeed(GlobalHelper.randomSeed, 0);
        currentReplay.SetStartPos(PlayerStats.respawnPosition, GlobalHelper.level);
        currentReplay.highScores[GlobalHelper.level] = PlayerStats.score;
        InterruptAllKeys();
        SaveLoad.SaveReplay(currentReplay, index);
    }
}
