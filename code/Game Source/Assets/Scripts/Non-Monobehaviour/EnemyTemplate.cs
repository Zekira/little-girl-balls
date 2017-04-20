using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// A class representing the data belonging to an enemy, whether it actually exists or not.
/// </summary>
public class EnemyTemplate {
    public float scale = 1f;
    public int enemyID = 0;
    public bool colorise = false;
    public Color color = Color.white;
    public bool isBoss = false;
    public int maxHealth = 1;
    public int dropValueCount = 0;
    public int dropPowerCount = 0;
    public int dropPowerLargeCount = 0;
    public int dropPowerFullCount = 0;
    public int dropScoreCount = 0;
    public Vector2 startpostion = new Vector2(0f, 0f);
    public uint baseScore = 0;
    public List<string> attackPath = new List<string>(); //NECCESSARY; should be at least one long.
    public List<string> spellcardName = new List<string>();
    public List<int> spellTimers = new List<int>();


    public EnemyTemplate() {
    }

    public EnemyTemplate(EnemyTemplate template) {
        scale = template.scale;
        attackPath = template.attackPath;
        enemyID = template.enemyID;
        colorise = template.colorise;
        color = template.color;
        isBoss = template.isBoss;
        maxHealth = template.maxHealth;
        dropValueCount = template.dropValueCount;
        dropPowerCount = template.dropPowerCount;
        dropPowerLargeCount = template.dropPowerLargeCount;
        dropScoreCount = template.dropScoreCount;
        startpostion = template.startpostion;
        baseScore = template.baseScore;
        dropPowerFullCount = template.dropPowerFullCount;
        spellcardName = template.spellcardName;
        spellTimers = template.spellTimers;
    }
}
