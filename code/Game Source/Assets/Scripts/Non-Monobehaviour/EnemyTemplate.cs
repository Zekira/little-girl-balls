﻿using UnityEngine;
using System.Collections.Generic;

public struct EnemyTemplate {
    public float scale;
    public int enemyID;
    public bool isBoss;
    public int maxHealth;
    public int dropValueCount;
    public int dropPowerCount;
    public int dropPowerLargeCount;
    public int dropPowerFullCount;
    public int dropScoreCount;
    public Vector2 startpostion;
    public uint baseScore;
    public List<string> attackPath; //NECCESSARY; should be at least one long.
    public List<int> spellTimers;
    public DialogueEntry.character character; //For the bosses' spellcard portrait.


    public EnemyTemplate(bool basic) {
        scale = 1f;
        enemyID = 0;
        isBoss = false;
        maxHealth = 1;
        dropValueCount = 0;
        dropPowerCount = 0;
        dropPowerLargeCount = 0;
        dropPowerFullCount = 0;
        dropScoreCount = 0;
        startpostion = new Vector2(0f, 0f);
        baseScore = 0;
        attackPath = new List<string>(); //NECCESSARY; should be at least one long.
        spellTimers = new List<int>();
        character = DialogueEntry.character.RACHEL; //For the bosses' spellcard portrait.
}
}

/*/// <summary>
/// A class representing the data belonging to an enemy, whether it actually exists or not.
/// </summary>
public class EnemyTemplate {
    public float scale = 1f;
    public int enemyID = 0;
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
    public List<int> spellTimers = new List<int>();
    public DialogueEntry.character character = DialogueEntry.character.RACHEL; //For the bosses' spellcard portrait.


    public EnemyTemplate() {
    }

    public EnemyTemplate(EnemyTemplate template) {
        scale = template.scale;
        attackPath = template.attackPath;
        enemyID = template.enemyID;
        isBoss = template.isBoss;
        maxHealth = template.maxHealth;
        dropValueCount = template.dropValueCount;
        dropPowerCount = template.dropPowerCount;
        dropPowerLargeCount = template.dropPowerLargeCount;
        dropScoreCount = template.dropScoreCount;
        startpostion = template.startpostion;
        baseScore = template.baseScore;
        dropPowerFullCount = template.dropPowerFullCount;
        spellTimers = template.spellTimers;
        character = template.character;
    }
}*/
