﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// A class made to make my life easier.
/// </summary>
public static class GlobalHelper {

    //Things that make finding objects in other classes easier
    public static Transform enemyParent = GameObject.FindWithTag("EnemyParent").transform;
    public static Transform spellcardBackground = GameObject.FindWithTag("SpellcardBackground").transform;
    public static GameObject bossUI = GameObject.FindWithTag("BossUI");
    public static Transform secondCounter = bossUI.transform.Find("TimerSeconds");
    public static Transform msecondCounter = bossUI.transform.Find("TimerMilliseconds");
    public static GameObject player = GameObject.FindGameObjectWithTag("Player");
    public static GameObject levelManager = GameObject.FindWithTag("LevelManager");

    public static PlayerStats stats = player.GetComponent<PlayerStats>();
    public static System.Random random = new System.Random(); //NOTE: Handle ALL random events through this; if I want to be able to add replays, I should save the seeds and input them here.

    public const int enemyTextureCount = 1;
    public const int bulletTextureCount = 3;
    public static List<GameObject> backupBullets = new List<GameObject>();
    public static List<Sprite> bulletSprites = new List<Sprite>();
    public static List<Sprite> enemySprites = new List<Sprite>();

    public static int totalFiredBullets;
    public static bool paused = false;
    public static bool dialogue = false;
    public static int stageNumber = 1;
    public enum Difficulty {EASY, NORMAL, HARD, LUNATIC, EXTRA};
    public static Difficulty difficulty = Difficulty.EASY;
    public static bool autoCollectItems = false;
    public static float destroyBulletsHeight = 99f;

    //Things used in createbullet and createenemy that differ everytime but is a waste to keep creating and better to just keep access to all the time.
    private static GameObject createdObject;
    private static MaterialPropertyBlock bulletMatPropertyBlock = new MaterialPropertyBlock();
    private static Bullet bullet;
    private static Vector3 bulletpos;
    private static SpriteRenderer spriteRenderer;

    /// <summary>
    /// Get a reference to the Player GameObject.
    /// </summary>
    /// <returns>Returns the Player GameObject.</returns>
    public static GameObject GetPlayer() {
        return player;
    }

    /// <summary>
    /// Gets a reference to the PlayerStats component.
    /// </summary>
    /// <returns>Returns the PlayerStats component.</returns>
    public static PlayerStats GetStats() {
        return stats;
    }

    /// <summary>
    /// Get a reference to either the TimerSeconds or TimerMilliseconds Transform. True to get the TimerSeconds. False to get the TimerMilliseconds.
    /// </summary>
    /// <returns>Returns TimerSeconds or TimerMilliseconds</returns>
    public static Transform GetTimer(bool type) {
        if (type) {
            return secondCounter;
        } else {
            return msecondCounter;
        }
    }

    /// <summary>
    /// Update the reference to the Player GameObject, if needed.
    /// </summary>
    public static void UpdatePlayer() {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = player.GetComponent<PlayerStats>();
    }

    /// <summary>
    /// Adds commas to numbers. 1234567 -> 1,234,567
    /// </summary>
    /// <param name="number">The number to add commas to</param>
    /// <returns>The number as a string with commas</returns>
    public static string Commafy(long number) {
        string returnString = number.ToString();
        int length = returnString.Length;
        for (int i = 3; i < length; i += 3) {
            returnString = returnString.Insert(length - i, ",");
        }
        return returnString;
    }

    /// <summary>
    /// Creates an enemy from a template with appropriate settings, applies the EnemyTemplate to the created object's Enemy class, and returns the created object.
    /// </summary>
    /// <param name="enemyTemplate">The template to base the enemy on. Needs to have at least one attackpath or everything will error. (TODO: Fix)</param>
    /// <returns>Returns a reference to the created object.</returns>
    public static GameObject CreateEnemy(EnemyTemplate enemyTemplate) {
        //If the list of enemysprites attached to GlobalHelper is empty, initialise them because they're needed here.
        if (enemySprites.Count == 0) {
            Texture2D texture;
            for (int i = 0; i < enemyTextureCount; i++) {
                texture = (Texture2D)Resources.Load("Graphics/Enemies/Enemy" + i);
                enemySprites.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 256));
            }
        }
        //Create the object and set the settings.
        createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Enemy"));

        createdObject.transform.position += new Vector3(enemyTemplate.startpostion.x, enemyTemplate.startpostion.y, 0f);
        createdObject.transform.localScale = enemyTemplate.scale * Vector3.one;
        createdObject.transform.SetParent(GameObject.FindWithTag("EnemyParent").transform);

        createdObject.GetComponent<Enemy>().health = enemyTemplate.maxHealth;

        createdObject.transform.GetComponent<SpriteRenderer>().sprite = enemySprites[enemyTemplate.enemyID];
        if (enemyTemplate.colorise) {
            createdObject.transform.GetComponent<SpriteRenderer>().color = enemyTemplate.color;
        } else {
            createdObject.transform.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 1f);
        }
        
        createdObject.GetComponent<Enemy>().template = enemyTemplate;
        return createdObject;
    }

    /// <summary>
    /// Creates an empty bullet ready to be used by CreateBullet(*actual arguments*). This is done to prevent Instantiate() lagginess as flipping a bool is faster than that.
    /// </summary>
    public static void CreateEmptyBullet() {
        createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Bullet"));
        createdObject.SetActive(false);
    }

    /// <summary>
    /// Creates a bullet from a template with appropriate settings, applies the BulletTemplate to the created object's Bullet class, and returns the created object.
    /// </summary>
    /// <param name="bulletTemplate">The BulletTemplate to use.</param>
    /// <param name="bulletPosition">The position to spawn the bullet in.</param>
    /// <returns>Returns a reference to the created bullet.</returns>
    public static GameObject CreateBullet(BulletTemplate bulletTemplate, Vector2 bulletPosition) {
        //If the list of bulletsprites attached to GlobalHelper is empty, initialise them because they're needed here.
        if (bulletSprites.Count == 0) {
            Texture2D texture;
            for (int i = 0; i < bulletTextureCount; i++) {
                texture = (Texture2D)Resources.Load("Graphics/Bullets/Bullet" + i);
                bulletSprites.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 256));
            }
        }
        //The z-value of bullets is this value because this prevents z-fighting. Also, fun stats.
        totalFiredBullets++;
        //Take it either from the backup list, or instantiate a new bullet. The former is prefered because it's faster.
        if (backupBullets.Count == 0) {
            createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Bullet"));
        } else {
            createdObject = backupBullets[0];
            backupBullets.RemoveAt(0);
            createdObject.SetActive(true);
        }
        //Modifies the bullet based on whether it's harmful (eg shot by the enemy or player). Harmful bullets can be grazed and kill you, unharmful bullets have damage attached and don't harm you.
        if (bulletTemplate.isHarmful) {
            bulletpos = new Vector3(bulletPosition.x, bulletPosition.y, totalFiredBullets / 100000f);
        } else {
            bulletpos = new Vector3(bulletPosition.x, bulletPosition.y, 5 + totalFiredBullets / 100000f); //Player shot bullets should not cover actual harmful bullets.
        }
        if (!bulletTemplate.positionIsRelative) {
            bulletpos.x = bulletTemplate.position.x;
            bulletpos.y = bulletTemplate.position.y;
        } else {
            bulletpos.x += bulletTemplate.position.x;
            bulletpos.y += bulletTemplate.position.y;
        }
        //Set the bullet's internal position vars; reading transform.position is a laggy operation apparantly, so this solves that.
        bullet = createdObject.GetComponent<Bullet>();
        bullet.bulletTemplate = bulletTemplate;
        bullet.posx = bulletpos.x;
        bullet.posy = bulletpos.y;
        bullet.posz = bulletpos.z;
        //Set the actual position
        createdObject.transform.position = bulletpos;

        bullet.grazed = false;

        createdObject.transform.localScale = bulletTemplate.scale * Vector3.one;
        createdObject.transform.SetParent(GameObject.FindWithTag("BulletParent").transform);
        createdObject.transform.eulerAngles = new Vector3(0f, 0f, -bulletTemplate.rotation * Mathf.Rad2Deg);

        bullet.player = GetPlayer();

        spriteRenderer = createdObject.transform.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = bulletSprites[bulletTemplate.bulletID];
        //If the property block is empty, initalise it here because it's needed.
        spriteRenderer.GetPropertyBlock(bulletMatPropertyBlock);
        //Change the color the sprites should render
        bulletMatPropertyBlock.SetColor("_Color1", bulletTemplate.innerColor);
        bulletMatPropertyBlock.SetColor("_Color2", bulletTemplate.outerColor);
        spriteRenderer.SetPropertyBlock(bulletMatPropertyBlock);
        //Start the animation of the bullet spawning by disabling the Bullet thing and enabling the Materialisation.
        bullet.enabled = false;
        createdObject.GetComponent<BulletMaterialisation>().enabled = true;
        return createdObject;
    }
}
