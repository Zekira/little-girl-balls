using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour {

    public static bool isReplay = false; //False during normal gameplay, true during replays.
    private static int timer = 0;
    private int[] keytimers = { 0, 0, 0, 0, 0, 0, 0, 0 };
    private int[] keyindices = { 0, 0, 0, 0, 0, 0, 0, 0 };

    public static ReplayData currentReplay = new ReplayData(); //don't forget to initialise

    private void Awake() {
        currentReplay.SetPlayerStats(
            (byte)(PlayerStats.lifepieces + PlayerStats.lives * PlayerStats.piecesToLife),
            (byte)(PlayerStats.bombpieces + PlayerStats.bombs * PlayerStats.piecesToBomb),
            (byte)(PlayerStats.power/5),
            PlayerStats.value,
            PlayerStats.graze, GlobalHelper.level-1);
        currentReplay.SetStartPos(PlayerStats.startPosition, GlobalHelper.level-1); //TODO
    }

    private void Update() {
        if (GlobalHelper.paused) {
            return;
        }
        timer++;
        for (int i = 0; i < 8; i++) {
            if (Input.GetKeyDown(KeyData.GetKeyCode(i))) {
                keytimers[i] = 0;
                currentReplay.AddInputData(new InputData(-1, -1, KeyData.keys[0]), GlobalHelper.level-1); //placeholder for when the data should actually be recorded
                keyindices[i] = currentReplay.inputData[GlobalHelper.level-1].Count - 1;
            }
            if (Input.GetKey(KeyData.GetKeyCode(i))) {
                keytimers[i]++; //This triggers in the SAME tick as above! be wary.
            }
            if (Input.GetKeyUp(KeyData.GetKeyCode(i))) {
                currentReplay.inputData[GlobalHelper.level-1][keyindices[i]] = new InputData(timer - keytimers[i], keytimers[i], KeyData.keys[i]);
            }
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            currentReplay.SetPlayerAndDifficulty(GlobalHelper.character, GlobalHelper.difficulty);
            currentReplay.SetReplayName("This is honestly a 32 char name!");
            currentReplay.SetSeed(230, 0);
            SaveLoad.SaveReplay(currentReplay, 0);
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            SaveLoad.LoadReplay(0);
        }
    }
}
