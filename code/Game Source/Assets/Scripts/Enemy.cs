using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// A class representing the physical instance of both enemies and bosses (stored in EnemyTemplate.isBoss).
/// </summary>
public class Enemy : MonoBehaviour {

    public EnemyTemplate template = new EnemyTemplate();
    public int health = 1000;
    public Transform healthbarTransform;
    public Transform itemParentTransform;
    public int timer = 9999;
    public int currentAttack = 0;

    private bool midDelay = false;
    private int temptimer;

    void Start() {
        //Initialising stuff to both prevent null errors and update the UI if this is a boss.
        healthbarTransform = transform.Find("Healthbar");
        itemParentTransform = GameObject.FindWithTag("ItemParent").transform;
        if (template.isBoss) {
            healthbarTransform.gameObject.SetActive(true);
            GlobalHelper.bossUI.gameObject.SetActive(true);
            SetUIStarCount();
            currentAttack = -1;

            NextPhase(false, 0);
        }
    }

	void Update () {
        if (!GlobalHelper.paused && !midDelay) {
            timer--;
            //Timeout'd attack.
            if (timer <= 0) {
                NextPhase(false, 90);
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

    /// <summary>
    /// Starts what should happen when this enemy takes damage: reducing the health, and if it's a boss, updating the healthbar.
    /// </summary>
    /// <param name="amount">The amount of damage dealt.</param>
    public void TakeDamage(int amount) {
        if (template.isBoss) {
            UpdateHealthbar(health-amount);
        } else {
            health -= amount;
        }
    }

    /// <summary>
    /// Drops items, goes to the next attack / dies, optionally updates the UI (mainly if it's a boss).
    /// </summary>
    /// <param name="byDamage">Whether it happened by being damaged.</param>
    private void NextPhase(bool byDamage, int delay) {
        if (byDamage) {
            if (template.isBoss) {
                GlobalHelper.stats.AddScore(GlobalHelper.levelManager.GetComponent<SpellcardManager>().currentValue);
            } else {
                GlobalHelper.stats.AddScore(template.baseScore);
            }
            DropItems();
        } else {
            GlobalHelper.levelManager.GetComponent<SpellcardManager>().Fail();
        }
        //If this boss' attack was a spellcard (and existed, that's why thereý a >= 0), a bonus needs to be shown
        if (template.isBoss && currentAttack >= 0 && template.spellcardName[currentAttack] != "") {
            GlobalHelper.levelManager.GetComponent<SpellcardManager>().StartShowBonus();
        }
        //Goes to the next attack, and if there is none, goes away.
        if (currentAttack + 1 < template.attackPath.Count) {
            currentAttack++;
        } else { //It's done and no more attacks are left
            //If it's a boss the bossUI should become inactive when it gets defeated, and only bosses should make the screen clear.
            if (template.isBoss) {
                GlobalHelper.bossUI.SetActive(false);
                GlobalHelper.spellcardBackground.gameObject.SetActive(false);
                StartCoroutine(GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(10f, 30));
            }
            Destroy(this.gameObject);
            return;
        }
        //Clears bullets if it's a boss
        if (template.isBoss) {
            StartCoroutine(GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(10f, 30));
            SetUIStarCount();
            GlobalHelper.levelManager.GetComponent<SpellcardManager>().ActivateSpellcard(template, currentAttack, this);
        }

        //Set the time of the current attack (if it exists), or 9999 if it isn't there.
        if (currentAttack < template.spellTimers.Count) {
            timer = template.spellTimers[currentAttack];
        } else {
            timer = 9999;
        }
        foreach (TimelineInterprenter i in GetComponents<TimelineInterprenter>()) {
            Destroy(i);
        }
        StartCoroutine(FillHealthbar(delay));
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
            delay--;
            yield return null;
        }
        midDelay = false;

        foreach (TimelineInterprenter i in GetComponents<TimelineInterprenter>()) {
            Destroy(i);
        }
        TimelineInterprenter newInterprenter = gameObject.AddComponent<TimelineInterprenter>();
        newInterprenter.patternPath = template.attackPath[index];
    }

    /// <summary>
    /// Fills the healthbar from empty to full taking "time" ticks. It directly modifies the health variable.
    /// </summary>
    private IEnumerator FillHealthbar(int time) {
        int currentTime = 0;
        while (currentTime < time) {
            UpdateHealthbar(1+currentTime * template.maxHealth / time);
            currentTime++;
            yield return null;
        }
    }

    private void DropItems() {
        GameObject createdObject;
        Vector3 position;
        position = transform.position;
        //Drop stuff depending on what's stated in the template, with a random offset, and z-value set to how important it is (+randomness to prevent z-fighting).
        for (int i = 0; i < template.dropPowerCount; i++) {
            createdObject = Instantiate((GameObject)Resources.Load("Prefabs/PowerItem"));
            createdObject.transform.SetParent(itemParentTransform);
            createdObject.transform.position = position + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble());
        }
        for (int i = 0; i < template.dropPowerLargeCount; i++) {
            createdObject = Instantiate((GameObject)Resources.Load("Prefabs/PowerItemLarge"));
            createdObject.transform.SetParent(itemParentTransform);
            createdObject.transform.position = position + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble() - 0.02f);
        }
        if (template.dropPowerFullCount >= 1) {
            createdObject = Instantiate((GameObject)Resources.Load("Prefabs/PowerItemFull"));
            createdObject.transform.SetParent(itemParentTransform);
            createdObject.transform.position = position + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble() - 0.03f);
        }
        for (int i = 0; i < template.dropScoreCount; i++) {
            createdObject = Instantiate((GameObject)Resources.Load("Prefabs/PointItem"));
            createdObject.transform.SetParent(itemParentTransform);
            createdObject.transform.position = position + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble());
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
            if (template.spellcardName[i] != "") {
                spellcardCount++;
            }
        }
        SetUIStarCount(spellcardCount);
    }
}
