using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Made to handle spellcards and their score/history.
/// </summary>
public class SpellcardManager : MonoBehaviour {

    public uint startValue = 0;
    public uint currentValue = 0;
    public int timeLimit = 0;
    public bool failed = false;

    //private int ticksTaken = 0; //TODO: time taken
    //private float timeTaken = 0;
    private Enemy parentEnemy;
    private GameObject spellcardUI;
    private DialogueEntry.character portraitCharacter;

    void Start() {
        spellcardUI = GlobalHelper.bossUI.transform.FindChild("SpellcardUI").gameObject;
    }

    //void Update() {
    //    ticksTaken++;
    //    timeTaken += Time.deltaTime;
    //}

    /// <summary>
    /// If the attack's name is nothing, it does nothing. If it is something, it starts everything associated with spellcards.
    /// </summary>
    public void ActivateSpellcard(EnemyTemplate template, int attack, Enemy enemy) {
        portraitCharacter = template.character;
        StopCoroutine(DecreaseScore());
        StopCoroutine(MoveSpellUI());
        StopCoroutine(MoveCasterPortrait());
        //ticksTaken = 0;
        //timeTaken = 0;
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
            StartCoroutine(MoveCasterPortrait());
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

    private IEnumerator MoveCasterPortrait() { //From (-320,-80) to (230,520)
        Vector2 startPos = new Vector2(-320f, -200f);
        Vector2 endPos = new Vector2(130f, 100f);
        GameObject spellcardCaster = GlobalHelper.bossUI.transform.FindChild("SpellcardCaster").gameObject;
        spellcardCaster.SetActive(true);
        Image spellcardCasterImage = spellcardCaster.GetComponent<Image>();
        spellcardCasterImage.sprite = GlobalHelper.levelManager.GetComponent<CharacterPortraits>().GetSprite(portraitCharacter, DialogueEntry.emotion.HAPPY);
        float linearProgress = 0f;
        float progress = 0f;
        RectTransform spellcardCasterTransform = spellcardCaster.GetComponent<RectTransform>();
        spellcardCasterTransform.anchoredPosition = new Vector3(startPos.x, startPos.y);
        Color color = spellcardCasterImage.color;
        color.a = 0.75f;
        spellcardCasterImage.color = color;
        //how much it should move when linprogress = [0,1] is defined by f(x)=80(x-0.5)^4; the integral from 0 to 1 equals 1, so everytick add f(linprogress)'t part of the difference between start end end pos.
        //for some reason I have to divide the thing by 100 though. Guessing it's because this isn't actually integrating it.
        while (linearProgress < 1f) {
            progress = f(linearProgress);
            spellcardCasterTransform.anchoredPosition += progress * (endPos - startPos);
            if (linearProgress > 0.66f) {
                color = spellcardCasterImage.color;
                color.a = 0.75f - (linearProgress-0.66f) * 0.75f / 0.34f;
                spellcardCasterImage.color = color;
            }
            
            linearProgress += 0.01f;
            yield return null;
        }
        spellcardCaster.SetActive(false);
    }

    private float f(float f) {
        return 0.8f * (f - 0.5f) * (f - 0.5f) * (f - 0.5f) * (f - 0.5f);
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

    private IEnumerator DecreaseScore() {
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

    /// <summary>
    /// Coroutines fail when the object calling them is destroyed/inactive even though what they to is still avtive so I have to do it like this.
    /// This shows the "Got Spell Card Bonus! [x]" or "Bonus Failed...".
    /// </summary>
    public void StartShowBonus() {
        StartCoroutine(ShowBonus());
    }

    private IEnumerator ShowBonus() { //TODO: time taken + actual time taken.
        Transform spellcardBonus = GameObject.FindWithTag("UIVariable").transform.FindChild("SpellcardBonus");
        spellcardBonus.gameObject.SetActive(true);
        if (!failed) {
            spellcardBonus.FindChild("Title").GetComponent<Text>().text = "Got Spell Card Bonus!";
            spellcardBonus.FindChild("Score").GetComponent<Text>().text = GlobalHelper.Commafy(currentValue);
        } else {
            spellcardBonus.FindChild("Title").GetComponent<Text>().text = "Bonus Failed...";
            spellcardBonus.FindChild("Score").GetComponent<Text>().text = "";
        }
        int waitTime = 150;
        while (waitTime > 0) {
            waitTime--;
            yield return null;
        }
        spellcardBonus.gameObject.SetActive(false);
    }

    /// <summary>
    /// The spellcard is not captured if this is called.
    /// </summary>
    public void Fail() {
        failed = true;
        currentValue = 0;
        spellcardUI.transform.FindChild("Bonus").GetComponent<Text>().text = "FAILED";
    }
}
