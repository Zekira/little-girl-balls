using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For subphases within an attack
/// </summary>
public struct Phase {

    public int goalLine; //Where to jump to when the health or timer is low enough
    public int healthTrigger; //At what health to go to tag
    public int timeTrigger; //At what time to go to tag
    public bool clear; //Whether to clear all bullets when going to tag
    public Enemy enemyReference; //Where to pull the health data from
    public GameObject lockReference; //What object the healthbar lock is

    public Phase(int goalLine, int healthTrigger, int timeTrigger, bool clear, Enemy enemyReference, GameObject lockReference) {
        this.goalLine = goalLine;
        this.healthTrigger = healthTrigger;
        this.timeTrigger = timeTrigger;
        this.clear = clear;
        this.enemyReference = enemyReference;
        this.lockReference = lockReference;
    }

    public bool CheckTrigger() {
        if (enemyReference.timer <= timeTrigger) {
            return true;
        }
        if (enemyReference.health <= healthTrigger) {
            return true;
        }
        return false;
    }
}
