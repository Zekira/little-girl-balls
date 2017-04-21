using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Made to handle spellcards, its score/history, and its animations. TODO: the latter. 
/// </summary>
public class SpellcardManager : MonoBehaviour {

    public uint startValue = 0;
    public uint currentValue = 0;
    public int timeLimit = 0;
    public bool failed = false;

    private Enemy parentEnemy;
    private GameObject spellcardUI;

    void Start() {
        spellcardUI = GlobalHelper.bossUI.transform.FindChild("SpellcardUI").gameObject;
    }

    /// <summary>
    /// If the attack's name is nothing, it does nothing. If it is something, it starts everything associated with spellcards.
    /// </summary>
    public void ActivateSpellcard(EnemyTemplate template, int attack, Enemy enemy) {
        StopCoroutine(DecreaseScore());
        StopCoroutine(MoveSpellUI());
        failed = false;
        parentEnemy = enemy;
        startValue = (uint)(GlobalHelper.difficulty + GlobalHelper.stageNumber) * 1000000;
        currentValue = startValue;
        timeLimit = template.spellTimers[attack];
        string name = template.spellcardName[attack];
        if (name != "") {
            spellcardUI.SetActive(true);
            GlobalHelper.spellcardBackground.gameObject.SetActive(true);
            SetSpellcardName(name);
            StartCoroutine(MoveSpellUI());
            StartCoroutine(DecreaseScore());
        } else {
            spellcardUI.SetActive(false);
            GlobalHelper.spellcardBackground.gameObject.SetActive(false);
            currentValue = template.baseScore;
        }
    }

    private void SetSpellcardName(string name) { //Todo: add support for storing histories
        spellcardUI.SetActive(true);
        spellcardUI.transform.FindChild("SpellcardName").GetComponent<Text>().text = name;
    }

    /// <summary>
    /// Moves the name, bonus, and history from bottom left to top right. Topright = (33,320), bottomleft = (-640,-340)
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveSpellUI() {
        RectTransform uiTransform = spellcardUI.GetComponent<RectTransform>();
        float progress = 0f;
        Vector3 position = new Vector3(-640, -340, 0);
        while (progress <= 1f) { //The first bit: move right
            position.x = Mathf.Lerp(-640, 33, progress);
            progress += 0.05f;
            uiTransform.anchoredPosition = position;
            yield return null;
        }
        position = new Vector3(33, -340, 0);
        uiTransform.anchoredPosition = position;
        progress = 0f;
        //Wait a bit
        while (progress <= 1f) {
            progress += 0.2f;
        }
        progress = 0f;
        while (progress <= 1f) { //The second bit: move up
            position.y = Mathf.Lerp(-340, 320, progress);
            progress += 0.015f;
            uiTransform.anchoredPosition = position;
            yield return null;
        }
        position = new Vector3(33, 320, 0);
        uiTransform.anchoredPosition = position;
    }

    public IEnumerator DecreaseScore() {
        uint lessPerTick = (uint)(startValue / (timeLimit * 10)) * 10;
        while (timeLimit > 0) {
            if (!GlobalHelper.paused) {
                if (!failed) {
                    currentValue = startValue - lessPerTick * (uint)(timeLimit - parentEnemy.timer);
                    if ((timeLimit & 2) == 0) { //every fourth tick
                        spellcardUI.transform.FindChild("Bonus").GetComponent<Text>().text = currentValue.ToString();
                    }
                }
            }
            yield return null;
        }
    }

    public IEnumerator ShowBonus() { //TODO: time taken + actual time taken. Also doesn't end the last time
        Transform spellcardBonus = GlobalHelper.bossUI.transform.FindChild("SpellcardBonus");
        spellcardBonus.gameObject.SetActive(true);
        if (!failed) {
            spellcardBonus.FindChild("Title").GetComponent<Text>().text = "Got Spell Card Bonus!";
            spellcardBonus.FindChild("Score").GetComponent<Text>().text = GlobalHelper.Commafy(currentValue);
        } else {
            spellcardBonus.FindChild("Title").GetComponent<Text>().text = "Bonus Failed...";
            spellcardBonus.FindChild("Score").GetComponent<Text>().text = "";
        }
        int waitTime = 90;
        while (waitTime > 0) {
            waitTime--;
            yield return null;
        }
        spellcardBonus.gameObject.SetActive(false);
    }

    public void Fail() {
        failed = true;
        currentValue = 0;
        spellcardUI.transform.FindChild("Bonus").GetComponent<Text>().text = "FAILED";
    }
}
