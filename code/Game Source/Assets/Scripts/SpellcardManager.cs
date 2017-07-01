using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Made to handle spellcards and their score/history.
/// </summary>
public class SpellcardManager : MonoBehaviour {

    public uint startValue = 0;
    public uint currentValue = 0;
    public int timeLimit = 0;
    public static bool failed = false;
    public static int currentSpellId; //Set in Enemy.GetSpell();

    private int ticksTaken = 0;
    private float timeTaken = 0;
    private Enemy parentEnemy;
    private GameObject spellcardUI;
    private DialogueEntry.character portraitCharacter;

    private List<short> histories = new List<short>();

    void Start() {
        spellcardUI = GlobalHelper.bossUI.transform.FindChild("SpellcardUI").gameObject;
    }

    void Update() {
        ticksTaken++;
        timeTaken += Time.deltaTime;
    }

    /// <summary>
    /// If the attack's name is nothing, it does nothing. If it is something, it starts everything associated with spellcards.
    /// </summary>
    public void ActivateSpellcard(EnemyTemplate template, int attack, Enemy enemy) {
        portraitCharacter = template.character;
        StopCoroutine(DecreaseScore());
        StopCoroutine(MoveSpellUI());
        StopCoroutine(MoveCasterPortrait());
        ticksTaken = 0;
        timeTaken = 0;
        failed = false;
        parentEnemy = enemy;
        startValue = (uint)(GlobalHelper.difficulty + GlobalHelper.level) * 1000000;
        currentValue = startValue;
        timeLimit = template.spellTimers[attack];
        string name = Enemy.GetSpell(template.attackPath[attack]);
        if (name != "") {
            spellcardUI.SetActive(true);
            GlobalHelper.spellcardBackground.gameObject.SetActive(true);
            SetSpellcardName(name);
            int historyvalues = GetHistory(currentSpellId, 0); //TODO: Characters & shottypes
            spellcardUI.transform.FindChild("History").GetComponent<Text>().text = (historyvalues >> 16) + "/" + ((historyvalues & 0xffff) + 1);
            SetHistory(currentSpellId, 0, historyvalues >> 16, (historyvalues & 0xffff) + 1);
            spellcardUI.transform.FindChild("Bonus").GetComponent<Text>().text = currentValue.ToString();
            StartCoroutine(MoveSpellUI());
            StartCoroutine(MoveCasterPortrait());
            if (!enemy.timeoutAttack) { //Score should only decrease when it's a survival card
                StartCoroutine(DecreaseScore());
            }
        } else {
            spellcardUI.SetActive(false);
            GlobalHelper.spellcardBackground.gameObject.SetActive(false);
            currentValue = template.baseScore;
        }
        if (enemy.timeoutAttack) { //Healthbar shouldn't show when it's a survival attack.
            enemy.transform.FindChild("Healthbar").gameObject.SetActive(false);
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
            if (!GlobalHelper.paused) {
                progress = f(linearProgress);
                spellcardCasterTransform.anchoredPosition += progress * (endPos - startPos);
                if (linearProgress > 0.66f) {
                    color = spellcardCasterImage.color;
                    color.a = 0.75f - (linearProgress - 0.66f) * 0.75f / 0.34f;
                    spellcardCasterImage.color = color;
                }

                linearProgress += 0.01f;
            }
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
            if (!GlobalHelper.paused) {
                position.x = Mathf.Lerp(-640, 33, progress);
                progress += 0.05f;
                uiTransform.anchoredPosition = position;
            }
            yield return null;
        }
        position = new Vector3(33, -340, 0);
        uiTransform.anchoredPosition = position;
        progress = 0f;
        //Wait a bit
        while (progress <= 1f) {
            if (!GlobalHelper.paused) {
                progress += 0.2f;
            }
        }
        progress = 0f;
        while (progress <= 1f) { //The second bit: move up
            if (!GlobalHelper.paused) {
                position.y = Mathf.Lerp(-340, 320, progress);
                progress += 0.015f;
                uiTransform.anchoredPosition = position;
            }
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
    /// This shows the "Got Spell Card Bonus! [x]" or "Bonus Failed...", and updates the history
    /// </summary>
    public void EndSpellcard() {
        StartCoroutine(ShowBonus());
        int historyvalues = GetHistory(currentSpellId, 0); //TODO: Characters & shottypes
        SetHistory(currentSpellId, 0, (historyvalues >> 16) + (failed ? 0 : 1), historyvalues & 0xffff); //The second argument is already set at the beginning of the spell
    }

    private IEnumerator ShowBonus() {
        Transform spellcardBonus = GameObject.FindWithTag("UI").transform.FindChild("Boss Canvas").FindChild("SpellcardBonus");
        spellcardBonus.gameObject.SetActive(true);
        if (!failed) {
            spellcardBonus.FindChild("Title").GetComponent<Text>().text = StringFetcher.GetString("SPELLBONUS");
            spellcardBonus.FindChild("Score").GetComponent<Text>().text = GlobalHelper.Commafy(currentValue);
        } else {
            spellcardBonus.FindChild("Title").GetComponent<Text>().text = StringFetcher.GetString("BONUSFAILED");
            spellcardBonus.FindChild("Score").GetComponent<Text>().text = "";
        }
        spellcardBonus.FindChild("Time Taken").GetComponent<Text>().text = ticksTaken / 600 + "" + (ticksTaken / 60) % 10 + ":" + (ticksTaken % 60) / 6 + "" + (ticksTaken % 60) % 10;
        spellcardBonus.FindChild("Actual Time").GetComponent<Text>().text = ((int)timeTaken/10)+ "" + ((int)timeTaken%10) + ":" + ((int)(10*timeTaken%10)) + "" + ((int)(100*timeTaken%10));
        float slowdown;
        if (timeTaken == 0) { //Prevent division by zero if it happens somehow
            slowdown = 6.66f;
        } else { //What usually happens ie not killing the spellcard the tick it starts.
            slowdown = Mathf.Max((60 * timeTaken - ticksTaken) / (60 * timeTaken), 0f);
        }
        spellcardBonus.FindChild("Slowdown").GetComponent<Text>().text = ((int)(100*slowdown)) + "%";
        int waitTime = 150;
        while (waitTime > 0) {
            if (!GlobalHelper.paused) {
                waitTime--;
            }
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

    /// <summary>
    /// The int is two shorts: the first is the succeed count, the second is the attempts.
    /// To go back to two shorts: first = x>>16, second = 0xffff
    /// </summary>
    public int GetHistory(int spellcardId, int character) {
        histories = SaveLoad.LoadSpellcardHistories();
        for (int i = 0; i < histories.Count; i += 4) {
            if (histories[i] == spellcardId && histories[i + 1] == character) {
                return (histories[i + 2] << 16) + histories[i + 3];
            }
        }
        return 0; //Zero attempts and zero successes if the player hasn't faced it yet
    }

    public void SetHistory(int spellcardId, int character, int successes, int attempts) {
        bool foundInList = false;
        for (int i = 0; i < histories.Count; i += 4) {
            if (histories[i] == spellcardId && histories[i + 1] == character) {
                foundInList = true;
                histories[i + 2] = (short)successes;
                histories[i + 3] = (short)attempts;
            }
        }
        if (!foundInList) {
            histories.Add((short)spellcardId);
            histories.Add((short)character);
            histories.Add((short)successes);
            histories.Add((short)attempts);
        }
        SaveLoad.SaveSpellcardHistories(histories);
    }
}
