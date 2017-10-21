using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour {

    public static bool isReplay = false; //False during normal gameplay, true during replays.
    private static int timer = 0;
    private int[] keytimers = { 0, 0, 0, 0, 0, 0, 0, 0 };
    private int[] keyindices = { 0, 0, 0, 0, 0, 0, 0, 0 };

    public static ReplayData currentReplay = new ReplayData();

    private int currentInputDataIndex = 0; //For replays to keep track of how far along we are.
    private List<InputData> inputToCheck = new List<InputData>();
    private List<InputData> currentInput = new List<InputData>(); //During replays to keep track of what's currently happening.
    private PlayerMovement playerMovement;
    private bool[] all, prevAllKeys = new bool[] { false, false, false, false, false, false, false, false };

    private void Awake() {
        playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        timer = 0;
        if (!isReplay) {
            currentReplay.SetPlayerStats(
                (byte)(PlayerStats.lifepieces + PlayerStats.lives * PlayerStats.piecesToLife),
                (byte)(PlayerStats.bombpieces + PlayerStats.bombs * PlayerStats.piecesToBomb),
                (byte)(PlayerStats.power / 5),
                PlayerStats.value,
                PlayerStats.graze, GlobalHelper.level - 1);
            //Check any input down from before the level was started, and thus isn't registered by the Input.GetKeyDown part in Update().
            for (int i = 0; i < 8; i++) {
                if (Input.GetKey(KeyData.GetKeyCode(i))) {
                    keytimers[i] = 0;
                    currentReplay.AddInputData(new InputData(-1, -1, KeyData.keys[0]), GlobalHelper.level - 1); //placeholder for when the data should actually be recorded
                    keyindices[i] = currentReplay.inputData[GlobalHelper.level - 1].Count - 1;
                }
            }
        } else {
            inputToCheck = currentReplay.inputData[GlobalHelper.level-1];
        }
    }

    private void Update() {
        if (!GlobalHelper.paused) {
            timer++; //This way unpressing/pressing keys won't result in unknown behaviour, just zero-length stuff.
        }
        if (!isReplay) {
            for (int i = 0; i < 8; i++) {
                if (Input.GetKeyDown(KeyData.GetKeyCode(i))) {
                    keytimers[i] = 0;
                    currentReplay.AddInputData(new InputData(-1, -1, KeyData.keys[0]), GlobalHelper.level - 1); //placeholder for when the data should actually be recorded
                    keyindices[i] = currentReplay.inputData[GlobalHelper.level - 1].Count - 1;
                }
                if (!GlobalHelper.paused && Input.GetKey(KeyData.GetKeyCode(i))) { //Same comment as on the timer++; inside the non-pause check.
                    keytimers[i]++; //This triggers in the SAME tick as above! be wary. This causes everything (except menu stuff) to have at least length 1.
                }
                if (Input.GetKeyUp(KeyData.GetKeyCode(i))) {
                    currentReplay.inputData[GlobalHelper.level - 1][keyindices[i]] = new InputData(timer - keytimers[i], keytimers[i], KeyData.keys[i]);
                    //There can be zero-length shit in here but that doesn't really matter.
                }
            }
            if (Input.GetKeyDown(KeyCode.I)) { //TODO: Temp to remove!
                currentReplay.SetPlayerAndDifficulty(GlobalHelper.character, GlobalHelper.difficulty);
                currentReplay.SetReplayName("This is honestly a 32 char name!");
                currentReplay.SetSeed(GlobalHelper.randomSeed, 0);
                currentReplay.SetStartPos(PlayerStats.startPosition, GlobalHelper.level - 1);
                SaveLoad.SaveReplay(currentReplay, 0);
            }
        }
        if (Input.GetKeyDown(KeyCode.O)) { //TODO: Temp to remove!
            LoadReplayIntoGame(SaveLoad.LoadReplay(0), 0);
        }

        //When in a replay, check for all "inputs". This time, they're not from keyboards, but from the replay.
        if (isReplay && !GlobalHelper.paused) {
            //Check for new input
            while ((currentInputDataIndex < inputToCheck.Count) &&
                (inputToCheck[currentInputDataIndex].startingTick <= timer)) { //If it's new, it needs to be executed THIS tick.
                currentInput.Add(inputToCheck[currentInputDataIndex]);
                currentInputDataIndex++;
            }

            //Throw away old input
            List<int> indicesToRemove = new List<int>();
            for (int i = currentInput.Count-1; i >= 0; i--) { //Top down because the order of removal matters: removing 1,2,3 gives a different result from 3,2,1, the latter I want.
                if (timer >= currentInput[i].startingTick + currentInput[i].duration) { // >= because length 0 should be killed, but length 1 should have 1 tick of time. Inductively length n will have n ticks.
                    indicesToRemove.Add(i);
                }
            }
            foreach(int i in indicesToRemove) {
                currentInput.RemoveAt(i);
            }

            //Combine all keypresses into a single bool array to work with
            bool[] allKeys = new bool[] { false, false, false, false, false, false, false, false };
            foreach(InputData i in currentInput) {
                allKeys = KeyData.Combine(i.keys, allKeys);
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

    /// <summary>
    /// Restarts the game and lets it be played from a replay. Here level is [0-6].
    /// </summary>
    public static void LoadReplayIntoGame(ReplayData replay, int level) {
        currentReplay = replay;
        SceneSwitcher.LoadLevel(level+1, (GlobalHelper.Difficulty)(replay.playerAndDifficulty / 6), true);
    }
}
