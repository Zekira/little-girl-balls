﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSwitcher  {
    
    /// <summary>
    /// (Re)sets the scene to the level scene with level "level". This makes it a NEW GAME. So only use it for starting a playthrough, it resets scores and everything.
    /// </summary>
    public static void LoadLevel(int level, GlobalHelper.Difficulty difficulty, bool replay) {
        //Kill all old players; this is a new game so they aren't needed
        if (!replay) { //Restart recording the replay in this new game
            ReplayManager.currentReplay = new ReplayData();
        }
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            GameObject.Destroy(player);
        }
        PlayerStats.newPlayer = true;
        LoadLevelWithoutExtras(level, difficulty, replay);
    }

    private static void LoadLevelWithoutExtras (int level, GlobalHelper.Difficulty difficulty, bool replay) {
        Menu.previousSelectedMenuItems = new List<Transform>();
        GlobalHelper.paused = true;
        GlobalHelper.difficulty = difficulty;
        GlobalHelper.level = level;
        ReplayManager.isReplay = replay;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("level");
    }

    /// <summary>
    /// Loads a level while keeping data from the current level.
    /// </summary>
    public static void ContinueToLevel(int level) {
        /* Data needed to be kept:
         * Everything from PlayerStats. Update the startpos.
         */
        ReplayManager.currentReplay.startpos[level] = PlayerPosGetter.playerPos;
        //LoadLevelWithoutExtras(level, GlobalHelper.difficulty, ReplayManager.isReplay); //NOT using this as loading a scene RESETS ALL INPUT until it's being pressed again (so it usually even needs a release!). So I can't use that because in bullet hells input is usually held throughout stage changes.
        GlobalHelper.level = level;
        //Start the level by changing and resetting LevelManager's timelineinterprenter
        GlobalHelper.levelManager.GetComponent<TimelineInterprenter>().Reset("Timelines/Stages/Stage" + (GlobalHelper.level + 1 /*The level files are 1-indexed, everything else 0-indexed*/) + "_" + ((int)GlobalHelper.difficulty));
    }

    public static void LoadMenu() {
        Menu.previousSelectedMenuItems = new List<Transform>();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("menu");
    }
}
