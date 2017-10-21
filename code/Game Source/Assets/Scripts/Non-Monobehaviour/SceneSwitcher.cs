using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSwitcher  {
    
    /// <summary>
    /// (Re)sets the scene to the level scene with level "level".
    /// </summary>
    public static void LoadLevel(int level, GlobalHelper.Difficulty difficulty, bool replay) {
        Menu.previousSelectedMenuItems = new List<Transform>();
        GlobalHelper.paused = true;
        GlobalHelper.difficulty = difficulty;
        GlobalHelper.level = level;
        ReplayManager.isReplay = replay;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("level");
    }

    public static void LoadMenu() {
        Menu.previousSelectedMenuItems = new List<Transform>();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("menu");
    }
}
