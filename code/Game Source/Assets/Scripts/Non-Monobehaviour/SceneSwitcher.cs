using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSwitcher  {
    
    /// <summary>
    /// (Re)sets the scene to the level scene with level "level". This makes it a NEW GAME. So only use it for starting a playthrough, it resets scores and everything.
    /// </summary>
    public static void LoadLevel(int level, GlobalHelper.Difficulty difficulty, bool replay) {
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
        GameObject.DontDestroyOnLoad(GlobalHelper.player); //Keep the old player where he was
        ReplayManager.currentReplay.startpos[level] = PlayerPosGetter.playerPos;
        LoadLevelWithoutExtras(level, GlobalHelper.difficulty, ReplayManager.isReplay);
    }

    public static void LoadMenu() {
        Menu.previousSelectedMenuItems = new List<Transform>();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("menu");
    }
}
