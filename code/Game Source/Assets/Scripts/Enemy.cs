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

    void Start() {
        //Initialising stuff to both prevent null errors and update the UI if this is a boss.
        healthbarTransform = transform.Find("Healthbar");
        itemParentTransform = GameObject.FindWithTag("ItemParent").transform;
        if (template.isBoss) {
            healthbarTransform.gameObject.SetActive(true);
            GlobalHelper.bossUI.gameObject.SetActive(true);
            SetUIStarCount(template.attackPath.Count);
            ActivateSpellcard();
        }
    }

	void Update () {
        if (!GlobalHelper.paused) {
            timer--;
            //Timeout'd attack.
            if (timer <= 0) {
                NextPhase(false);
            }
            //Kiled attack.
            if (health <= 0) {
                NextPhase(true);
            }
            //Updates the UI's timer.
            if (template.isBoss) {
                GlobalHelper.GetTimer(true).GetComponent<Text>().text = (Mathf.Min(timer / 60, 99)).ToString();
                int temptimer = timer % 60 * 100 / 60;
                GlobalHelper.GetTimer(false).GetComponent<Text>().text = (temptimer / 10).ToString() + (temptimer % 10).ToString();
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
    private void NextPhase(bool byDamage) {
        if (byDamage) {
            GlobalHelper.stats.AddScore(template.baseScore);
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
        //Clears bullets if it's a boss
        if (template.isBoss) { 
            StartCoroutine(GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(10f));
        }

        //Goes to the next attack, and if there is none, goes away.
        if (currentAttack + 1 < template.attackPath.Count) {
            currentAttack++;
            StartNewAttack(currentAttack);
        } else {
            //If it's a boss the bossUI should become inactive when it gets defeated
            if (template.isBoss) {
                GlobalHelper.bossUI.gameObject.SetActive(false);
                GlobalHelper.spellcardBackground.gameObject.SetActive(false);
            }
            Destroy(this.gameObject);
            return;
        }

        if (template.isBoss) {
            SetUIStarCount(template.attackPath.Count - currentAttack - 1);
            ActivateSpellcard();
        }
    }

    /// <summary>
    /// If the attack's name is nothing, it does nothing. If it is something, it starts everything associated with spellcards.
    /// </summary>
    /// <param name="name"></param>
    public void ActivateSpellcard() {
        string name = template.spellcardName[currentAttack];
        if (name != "") {
            GlobalHelper.bossUI.FindChild("SpellcardUI").gameObject.SetActive(true);
            GlobalHelper.spellcardBackground.gameObject.SetActive(true);
            setSpellcardName(name);
        } else {
            GlobalHelper.bossUI.FindChild("SpellcardUI").gameObject.SetActive(false);
            GlobalHelper.spellcardBackground.gameObject.SetActive(false);
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
    /// Stops all current TimelineInterprenters, effectively killing all attacks, and starts a new one.
    /// </summary>
    /// <param name="index">The index of the template.attackPath to use.</param>
    private void StartNewAttack(int index) {
        foreach (TimelineInterprenter i in GetComponents<TimelineInterprenter>()) {
            Destroy(i);
        }
        TimelineInterprenter newInterprenter = gameObject.AddComponent<TimelineInterprenter>();
        newInterprenter.patternPath = template.attackPath[index];
    }

    /// <summary>
    /// Set how many stars should be displayed below the boss name.
    /// </summary>
    /// <param name="num">The number of stars</param>
    private void SetUIStarCount(int num) {
        num = num < 0 ? 0 : num;
        Transform uiAttacks = GlobalHelper.bossUI.Find("Attacks");
        for (int i = 0; i < uiAttacks.childCount; i++) {
            if (i < num) {
                uiAttacks.GetChild(i).gameObject.SetActive(true);
            } else {
                uiAttacks.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    private void setSpellcardName(string name) { //Todo: add support for storing histories
        GlobalHelper.bossUI.FindChild("SpellcardUI").gameObject.SetActive(true);
        GlobalHelper.bossUI.FindChild("SpellcardUI").FindChild("SpellcardName").GetComponent<Text>().text = name;
    }
}
