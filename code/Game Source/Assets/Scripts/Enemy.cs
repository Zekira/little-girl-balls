using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// A class representing the physical instance of both enemies and bosses (stored in EnemyTemplate.isBoss), and for bosses some spellcard stuff.
/// </summary>
public class Enemy : MonoBehaviour {

    public EnemyTemplate template;
    public int health = 1000;
    public Transform healthbarTransform;
    public int timer = 9999;
    public int currentAttack = 0;
    public bool timeoutAttack = false; //This makes the boss immune until the end.

    private int bombTimer = 5;
    private bool midDelay = false;
    private int temptimer;

    void Start() {
        healthbarTransform = transform.Find("Healthbar");
        if (template.isBoss) {
            GlobalHelper.activeBosses++;
            healthbarTransform.gameObject.SetActive(true);
            GlobalHelper.bossUI.gameObject.SetActive(true);
            SetUIStarCount();
            currentAttack = -1;

            NextPhase(false, 0);
        }
    }

	void Update () {
        if (!GlobalHelper.paused) {
            //Checking collision with the player. This can happen inbetween phases, so outside the !midDelay part, but getting hit during dialogue is stupid, so not there.
            if (!GlobalHelper.dialogue && Vector2.Distance(transform.position, PlayerPosGetter.playerPos) < template.scale/2.5f) {
                GlobalHelper.stats.TakeDamage();
            }
            //Things that should not happen inbetween phases or during dialogue.
            if (!midDelay && !GlobalHelper.dialogue) {
                //Bomb damage. Going through phases by bombs is cheap so you can't do that.
                if (GlobalHelper.bulletClear.bulletClearType == BulletClear.BulletClearType.BOMB && GlobalHelper.bulletClear.destroyBulletsHeight < 5f) {
                    if (bombTimer <= 0 && health > 1) {
                        TakeDamage(1);
                        bombTimer = 5;
                    }
                    bombTimer--;
                }

                timer--;
                //Timeout'd attack.
                if (timer <= 0) {
                    if (timeoutAttack) {
                        NextPhase(true, 90);
                    } else {
                        NextPhase(false, 90);
                    }
                }
                //Kiled attack.
                if (health <= 0) {
                    NextPhase(true, 90);
                }
                //Updates the UI's timer.
                if (template.isBoss) {
                    GlobalHelper.GetTimer(true).GetComponent<Text>().text = (Mathf.Min(timer / 60, 99)).ToString();
                    temptimer = timer % 60 * 100 / 60;
                    if (timer < 6000) {
                        GlobalHelper.GetTimer(false).GetComponent<Text>().text = (temptimer / 10).ToString() + (temptimer % 10).ToString();
                    } else {
                        GlobalHelper.GetTimer(false).GetComponent<Text>().text = "99";
                    }
                }
            }
        }
	}

    /// <summary>
    /// Starts what should happen when this enemy takes damage: reducing the health, and if it's a boss, updating the healthbar.
    /// </summary>
    /// <param name="amount">The amount of damage dealt.</param>
    public void TakeDamage(int amount) {
        if (template.isBoss) {
            if (!timeoutAttack) {
                UpdateHealthbar(health - amount);
            }
        } else {
            health -= amount;
        }
    }

    /// <summary>
    /// Drops items, goes to the next attack / dies, optionally updates the UI (mainly if it's a boss), clears bullets.
    /// </summary>
    /// <param name="byDamage">Whether it happened by being damaged.</param>
    private void NextPhase(bool byDamage, int delay) {
        timeoutAttack = false;
        if (byDamage) {
            if (template.isBoss) {
                if (GetSpell(template.attackPath[currentAttack]) != "") {
                    GlobalHelper.stats.AddScore(GlobalHelper.levelManager.GetComponent<SpellcardManager>().currentValue);
                } else {
                    GlobalHelper.stats.AddScore(template.baseScore);
                }
            } else {
                GlobalHelper.stats.AddScore(template.baseScore);
            }
            DropItems();
        } else {
            GlobalHelper.levelManager.GetComponent<SpellcardManager>().Fail();
        }
        //If this boss' attack was a spellcard (and existed, that's why thereý a >= 0), a bonus needs to be shown
        if (template.isBoss && currentAttack >= 0 && GetSpell(template.attackPath[currentAttack]) != "") {
            GlobalHelper.levelManager.GetComponent<SpellcardManager>().EndSpellcard();
        }
        //Goes to the next attack, and if there is none, goes away.
        if (currentAttack + 1 < template.attackPath.Count) {
            currentAttack++;
        } else { //It's done and no more attacks are left
            //If it's a boss the bossUI should become inactive when it gets defeated, and only bosses should make the screen clear at death.
            if (template.isBoss) {
                GlobalHelper.activeBosses--;
                GlobalHelper.bossUI.SetActive(false);
                GlobalHelper.spellcardBackground.gameObject.SetActive(false);
                GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(10f, BulletClear.BulletClearType.FULLCLEAR, 30);
            }
            Destroy(this.gameObject);
            return;
        }
        //Set the time of the current attack (if it exists), 9999 if it isn't there, and set it to a timeout attack if it's negative.
        if (currentAttack < template.spellTimers.Count) {
            timer = template.spellTimers[currentAttack];
            if (timer < 0) {
                timeoutAttack = true;
                timer = 0 - timer;
            }
        } else {
            timer = 9999;
        }
        //Clears bullets if it's a boss, updates the UI, and starts the next spellcard
        if (template.isBoss) {
            GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(10f, BulletClear.BulletClearType.FULLCLEAR, 30);
            SetUIStarCount();
            GlobalHelper.levelManager.GetComponent<SpellcardManager>().ActivateSpellcard(template, currentAttack, this);
        }
        foreach (TimelineInterprenter i in GetComponents<TimelineInterprenter>()) {
            Destroy(i);
        }
        if (template.isBoss && !timeoutAttack) {
            StartCoroutine(FillHealthbar(delay));
        }
        StartCoroutine(DelayedStartNewAttack(currentAttack, delay));
    }

    /// <summary>
    /// Stops all current TimelineInterprenters, effectively killing all attacks, and starts a new one.
    /// </summary>
    /// <param name="index">The index of the template.attackPath to use.</param>
    private IEnumerator DelayedStartNewAttack(int index, int delay) {
        midDelay = true;
        foreach (TimelineInterprenter i in GetComponents<TimelineInterprenter>()) {
            Destroy(i);
        }
        while (delay > 0) {
            if (!GlobalHelper.paused) {
                delay--;
            }
            yield return null;
        }
        midDelay = false;
        TimelineInterprenter newInterprenter = gameObject.AddComponent<TimelineInterprenter>();
        newInterprenter.patternPath = template.attackPath[index];
    }

    /// <summary>
    /// Fills the healthbar from empty to full taking "time" ticks. It directly modifies the health variable.
    /// </summary>
    private IEnumerator FillHealthbar(int time) {
        transform.Find("Healthbar").gameObject.SetActive(true);
        int currentTime = 0;
        while (currentTime < time) {
            if (!GlobalHelper.paused) {
                UpdateHealthbar(1 + currentTime * template.maxHealth / time);
                currentTime++;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Drop the items specified in the EnemyTemplate.
    /// </summary>
    private void DropItems() {
        Vector3 position, basePosition;
        basePosition = transform.position;
        //Drop stuff depending on what's stated in the template, with a random offset, and z-value set to how important it is (+randomness to prevent z-fighting).
        for (int i = 0; i < template.dropPowerCount; i++) {
            position = basePosition + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble() - 0.01f);
            GlobalHelper.CreateItem(Item.ItemType.POWER, position);
        }
        for (int i = 0; i < template.dropPowerLargeCount; i++) {
            position = basePosition + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble() - 0.02f);
            GlobalHelper.CreateItem(Item.ItemType.LARGEPOWER, position);
        }
        if (template.dropPowerFullCount >= 1) {
            position = basePosition + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble() - 0.03f);
            GlobalHelper.CreateItem(Item.ItemType.FULLPOWER, position);
        }
        for (int i = 0; i < template.dropScoreCount; i++) {
            position = basePosition + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble());
            GlobalHelper.CreateItem(Item.ItemType.POINT, position);
        }
    }

    /// <summary>
    /// Updates the healthbar and sets the health to "to".
    /// </summary>
    /// <param name="to">What to set the health to.</param>
    public void UpdateHealthbar(int to) {
        health = Mathf.Min(template.maxHealth, to);
        UpdateHealthbar();
    }

    /// <summary>
    /// Updates the healthbar to the current health/maxhealth (through a shader).
    /// </summary>
    public void UpdateHealthbar() {
        if (healthbarTransform == null) {
            healthbarTransform = transform.Find("Healthbar");
        }
        healthbarTransform.GetComponent<SpriteRenderer>().material.SetFloat("_Progress", health / ((float)template.maxHealth));
    }

    /// <summary>
    /// Set how many stars should be displayed below the boss name.
    /// </summary>
    /// <param name="num">The number of stars</param>
    private void SetUIStarCount(int num) {
        num = num < 0 ? 0 : num;
        Transform uiAttacks = GlobalHelper.bossUI.transform.Find("Attacks");
        for (int i = 0; i < uiAttacks.childCount; i++) {
            if (i < num) {
                uiAttacks.GetChild(i).gameObject.SetActive(true);
            } else {
                uiAttacks.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Sets the amount of stars displayed below the boss name to the amount of spellcards left.
    /// </summary>
    private void SetUIStarCount() {
        int spellcardCount = 0;
        for (int i = currentAttack; i < template.attackPath.Count; i++) {
            if (GetSpell(template.attackPath[i]) != "") {
                spellcardCount++;
            }
        }
        SetUIStarCount(spellcardCount);
    }

    /// <summary>
    /// Gets the name of the spell-file if it's a spellcard, or returns an empty string if it isn't a spellcard.
    /// </summary>
    public static string GetSpell(string path) {
        string returnString = "";
        int idIndex = path.IndexOf("Spellcard");
        if (idIndex != -1) {
            int numericId = -1;
            int.TryParse(path.Substring(idIndex + 9), out numericId);
            SpellcardManager.currentSpellId = numericId;
            returnString = path.Substring(idIndex).ToUpperInvariant();
            returnString = StringFetcher.GetString(returnString);
        }
        return returnString;
    }
}
