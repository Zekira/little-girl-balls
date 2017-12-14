using System.Collections;
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
        GlobalHelper.level = level;
        //Make the replay think we're in stage 2 and split input so it's registered at the start of the stage
        ReplayManager.replayManager.MakeReplayNewstageCompatible();
        //Start the level by changing and resetting LevelManager's timelineinterprenter
        GlobalHelper.levelManager.GetComponent<TimelineInterprenter>().Reset("Timelines/Stages/Stage" + (GlobalHelper.level + 1 /*The level files are 1-indexed, everything else 0-indexed*/) + "_" + ((int)GlobalHelper.difficulty));
    }

    public static void LoadMenu() {
        Menu.previousSelectedMenuItems = new List<Transform>();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("menu");
    }
}
