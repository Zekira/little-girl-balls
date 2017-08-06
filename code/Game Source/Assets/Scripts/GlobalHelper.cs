using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
/// <summary>
/// A class made to make my life easier.
/// Communicates with almost literally everything.
/// </summary>
public class GlobalHelper : MonoBehaviour {

    //Things needed all the time that make (some) sense even when not in a level.
    public static System.Random random = new System.Random(); //NOTE: Handle ALL random events through this; if I want to be able to add replays, I should save the seeds and input them here.
    public static ulong totalFiredBullets; //Fun statistic to keep track of.
    public static ulong previousFiredBullets;
    public static int currentBullets;

    public static bool paused = false;

    public static int level = 1;
    public enum Difficulty { EASY, NORMAL, HARD, LUNATIC, EXTRA };
    public static Difficulty difficulty = Difficulty.LUNATIC;
    public enum Character { RACHEL_A, RACHEL_B, RACHEL_C };
    public static Character character = Character.RACHEL_A;
    public static byte musicHeard = 0; //See SaveLoad.cs for more info
    public static byte bestUnlockedStage = 0;
    public static short mainAttempts = 0;
    public static short mainFinishes = 0;
    public static short extraAttempts = 0;
    public static short extraFinishes = 0;

    //These things only make sense in a level, so they're defined, but initialised in Awake()
    public static Transform spellcardBackground, secondCounter, msecondCounter,canvas;
    public static GameObject bossUI, player, levelManager;
    public static List<GameObject> backupBullets, backupItems;
    public static List<Sprite> itemSprites, bulletSprites, snakeSprites;
    public static List<Sprite[]> enemySprites;
    public static PlayerStats stats;
    public static BulletClear bulletClear;
    public static CharacterPortraits characterPortraits;
    public static bool dialogue, autoCollectItems;
    public static int activeBosses;
    public static GameObject thisObject;

    //Things used in createbullet and createenemy that differ everytime but is a waste to keep creating and destroying and better to just keep access to all the time.
    private static GameObject createdObject;
    private static MaterialPropertyBlock bulletMatPropertyBlock;
    private static Bullet bullet;
    private static SpriteRenderer spriteRenderer;
    private static Transform bulletTransform;

    //Things that make finding objects in other classes easier, but only make sense when in a level: the only time GlobalHelper is a script attached to an object.
    //Also sets up things needed for the level and such as this only runs when loading the level.
    void OnEnable() {
        thisObject = gameObject;
        SaveLoad.LoadPlayerData(character);
        spellcardBackground = GameObject.FindWithTag("SpellcardBackground").transform;
        canvas = GameObject.FindWithTag("UI").transform;
        bossUI = GameObject.FindWithTag("BossUI");
        secondCounter = bossUI.transform.Find("TimerSeconds");
        msecondCounter = bossUI.transform.Find("TimerMilliseconds");
        player = GameObject.FindGameObjectWithTag("Player");
        levelManager = GameObject.FindWithTag("LevelManager");

        stats = player.GetComponent<PlayerStats>();
        bulletClear = levelManager.GetComponent<BulletClear>();
        characterPortraits = levelManager.GetComponent<CharacterPortraits>();

        backupBullets = new List<GameObject>(); //Bullet objects that are deactivated but can be used
        backupItems = new List<GameObject>(); //Item objects that are deactivated but can be used

        LoadEnemySprites();
        LoadBulletSprites();
        LoadItemSprites();
        LoadSnakeSprites();
        Texture2D bulletMaterialiseTexture = (Texture2D) Resources.Load("Graphics/MaterialiseSprite");
        BulletMaterialisation.materialiseSprite = Sprite.Create(bulletMaterialiseTexture, new Rect(0, 0, bulletMaterialiseTexture.width, bulletMaterialiseTexture.height), Vector2.one * 0.5f);

        bulletMatPropertyBlock = new MaterialPropertyBlock();

        activeBosses = 0;

        GameObject.FindWithTag("UIVariable").transform.Find("Difficulty").GetComponent<RawImage>().texture = (Texture2D)Resources.Load("Graphics/Difficulty/" + (int)difficulty);

        AudioSource audio = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
        audio.clip = (AudioClip) Resources.Load("Audio/Music/Stage" + level);
        audio.Play();

        dialogue = false;
        autoCollectItems = false;
        paused = false;
    
    }

    //Event to tick all timelineinterprenters
    public delegate void TickTimelineInterprenters();
    public static event TickTimelineInterprenters Tick;
    public static void TickInterprenters() {
        if (Tick != null) {
            Tick();
        }
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
    /// Adds commas to numbers. 1234567 -> 1,234,567
    /// </summary>
    /// <param name="number">The number to add commas to</param>
    /// <returns>The number as a string with commas</returns>
    public static string Commafy(ulong number) {
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
    /// <param name="enemyTemplate">The template to base the enemy on. Needs to have at least one attackpath or everything will error.</param>
    /// <returns>Returns a reference to the created object.</returns>
    public static GameObject CreateEnemy(EnemyTemplate enemyTemplate) {
        //No regular enemies are allowed to be created when there is a boss on screen.
        if (activeBosses > 0 && !enemyTemplate.isBoss) {
            return null;
        }
        //Create the object and set the settings.
        createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Enemy"));

        createdObject.transform.position += new Vector3(enemyTemplate.startpostion.x, enemyTemplate.startpostion.y, 0f);
        createdObject.transform.localScale = enemyTemplate.scale * Vector3.one;

        createdObject.GetComponent<Enemy>().health = enemyTemplate.maxHealth;

        createdObject.transform.GetComponent<SpriteAnimator>().SetSprites(enemySprites[enemyTemplate.enemyID]);
        
        createdObject.GetComponent<Enemy>().template = enemyTemplate;
        return createdObject;
    }

    /// <summary>
    /// Creates an empty bullet ready to be used by CreateBullet(*actual arguments*). Spam this when not lagging. This is done to prevent Instantiate() lagginess as flipping a bool is faster than that.
    /// </summary>
    public static void CreateEmptyBullet() {
        currentBullets++;
        createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Bullet"));
        createdObject.GetComponent<Bullet>().Deactivate();
    }

    /// <summary>
    /// Creates a bullet from a template with appropriate settings, applies the BulletTemplate to the created object's Bullet class, and returns the created object.
    /// </summary>
    /// <param name="bulletTemplate">The BulletTemplate to use.</param>
    /// <param name="bulletPosition">The position to spawn the bullet in.</param>
    /// <returns>Returns a reference to the created bullet.</returns>
    public static GameObject CreateBullet(BulletTemplate bulletTemplate, Vector2 bulletPosition) {
        currentBullets++;
        //The z-value of bullets is this value because this prevents z-fighting. Also, fun stats.
        totalFiredBullets++;
        //Take it either from the backup list, or instantiate a new bullet. The former is prefered because it's faster.
        if (backupBullets.Count == 0) {
            createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Bullet"));
        } else {
            int index = backupBullets.Count - 1;
            createdObject = backupBullets[index];
            backupBullets.RemoveAt(index);
            createdObject.SetActive(true);
        }
        bullet = createdObject.GetComponent<Bullet>();
        bullet.Reset(bulletTemplate);
        createdObject.GetComponent<TimelineInterprenter>().enabled = false;
        //Sets the position
        if (!bulletTemplate.positionIsRelative) {
            bullet.posx = bulletTemplate.position.x;
            bullet.posy = bulletTemplate.position.y;
        } else {
            bullet.posx = bulletPosition.x + bulletTemplate.position.x;
            bullet.posy = bulletPosition.y + bulletTemplate.position.y;
        }
        bullet.posz = totalFiredBullets * 1e-6f;
        if (!bulletTemplate.enemyShot) {
            bullet.posz += 5f; //Player shot bullets should not cover actual harmful bullets.
        }
        //Set the actual position
        bulletTransform = createdObject.transform;
        bulletTransform.position = new Vector3(bullet.posx, bullet.posy, bullet.posz);

        bulletTransform.localScale = bulletTemplate.scale * Vector3.one;
        bulletTransform.eulerAngles = new Vector3(0f, 0f, -bulletTemplate.rotation * Mathf.Rad2Deg);

        spriteRenderer = createdObject.GetComponent<SpriteRenderer>();
        bullet.SetSpriteDirectly(bulletSprites[bulletTemplate.bulletID]);
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

    private static Vector3 smallItemSize = new Vector3(0.45f, 0.45f, 1f);

    public static GameObject CreateItem(Item.ItemType type, Vector3 position) {
        if (backupItems.Count == 0) {
            createdObject = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Item"));
        } else {
            createdObject = backupItems[0];
            backupItems.RemoveAt(0);
            createdObject.SetActive(true);
        }
        Item item = createdObject.GetComponent<Item>();
        item.type = type;
        switch (type) {
            case Item.ItemType.POINT:
            case Item.ItemType.POWER:
                createdObject.transform.localScale = smallItemSize;
                break;
            default: //Fullpower, largepower, 
                createdObject.transform.localScale = Vector3.one;
                break;
        }
        //TODO: Update this whenever a new texture is added to Resources/Graphics/Items
        switch (type) {
            case Item.ItemType.FULLPOWER:
                createdObject.GetComponent<SpriteRenderer>().sprite = itemSprites[0];
                break;
            case Item.ItemType.POINT:
                createdObject.GetComponent<SpriteRenderer>().sprite = itemSprites[1];
                break;
            case Item.ItemType.POWER:
            case Item.ItemType.LARGEPOWER:
                createdObject.GetComponent<SpriteRenderer>().sprite = itemSprites[2];
                break;
        }
        createdObject.transform.position = position;
        return createdObject;

    }

    public static GameObject CreateLaser(LaserTemplate template, Vector2 position) {
        createdObject = GameObject.Instantiate(Resources.Load("Prefabs/Laser") as GameObject);
        if (template.positionIsRelative) {
            createdObject.transform.position = new Vector3(position.x + template.position.x, position.y + template.position.y, 1);
        } else {
            createdObject.transform.position = new Vector3(template.position.x, template.position.y, 1f);
        }
        createdObject.transform.localScale = new Vector3(0.06f, 99f, 1f);
        createdObject.GetComponent<Laser>().template = template;

        spriteRenderer = createdObject.transform.GetComponent<SpriteRenderer>();
        //If the property block is empty, initalise it here because it's needed.
        spriteRenderer.GetPropertyBlock(bulletMatPropertyBlock);
        //Change the color the sprites should render
        bulletMatPropertyBlock.SetColor("_Color1", template.innerColor);
        bulletMatPropertyBlock.SetColor("_Color2", template.outerColor);
        spriteRenderer.SetPropertyBlock(bulletMatPropertyBlock);
        
        return createdObject;
    }

    public static void LoadBulletSprites() {
        bulletSprites = new List<Sprite>();
        foreach (Texture2D texture in Resources.LoadAll<Texture2D>("Graphics/Bullets")) {
            bulletSprites.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 128));
        }
    }

    public static void LoadItemSprites() {
        itemSprites = new List<Sprite>();
        foreach (Texture2D texture in Resources.LoadAll<Texture2D>("Graphics/Items")) {
            itemSprites.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100));
        }
    }

    public static void LoadEnemySprites() {
        enemySprites = new List<Sprite[]>();
        foreach (Texture2D texture in Resources.LoadAll<Texture2D>("Graphics/Enemies")) {
            enemySprites.Add(SpriteAnimator.GetSprites(texture));
        }
    }

    public static void LoadSnakeSprites() {
        snakeSprites = new List<Sprite>();
        foreach (Texture2D texture in Resources.LoadAll<Texture2D>("Graphics/Snake")) {
            snakeSprites.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 128));
        }
    }

    /// <summary>
    /// (Re)sets the scene to the level scene with level "level".
    /// </summary>
    public static void LoadLevel(int level, Difficulty difficulty) {
        paused = true;
        GlobalHelper.difficulty = difficulty;
        GlobalHelper.level = level;
        SceneManager.LoadSceneAsync("level");
    }

    public static void SetPaused(bool paused) {
        Menu.previousSelectedMenuItems = new List<Transform>();
        GlobalHelper.paused = paused;
        player.GetComponent<PlayerMovement>().UpdateFocused(); //Updating focus is needed when unpausing, otherwise it wouldn't register releasing/holding the button during the pause
        canvas.Find("Pause Canvas").gameObject.SetActive(paused);
        canvas.Find("Dialogue Canvas").gameObject.SetActive(/*dialogue &&*/ !paused); //It's empty anyways if there's no dialogue going on, so no need to hide it then
        if (paused) {
            GameObject.FindWithTag("BGM").GetComponent<AudioSource>().Pause();
        } else {
            GameObject.FindWithTag("BGM").GetComponent<AudioSource>().UnPause();
        }
    }

    /*public static Transform[] RemoveInactive(Transform[] transforms) {
        List<int> inactiveIndices = new List<int>();
        for (int i = 0; i < transforms.Length; i++) {
            if (!transforms[i].gameObject.activeInHierarchy) {
                inactiveIndices.Add(i);
            }
        }
        if (inactiveIndices.Count == 0) {
            return transforms;
        }
        Transform[] returnTransform = new Transform[transforms.Length - inactiveIndices.Count];
        int j = 0;
        for (int i = 0; i < transforms.Length; i++) {
            if (!inactiveIndices.Contains(i)) {
                returnTransform[j] = transforms[i];
                j++;
            }
        }
        return returnTransform;
    }*/

    public void CreateSnake (int length, BulletTemplate template, Vector3 position) {
        StartCoroutine(CoCreateSnake(length, template, position));
    }

    private IEnumerator CoCreateSnake(int length, BulletTemplate template, Vector3 position) {
        //First one manually
        GameObject recentObject = CreateBullet(template, position);
        new Snake(new Transform[] { recentObject.transform });
        yield return null;
        yield return null;
        yield return null;
        int i = 1;
        while (i < length) {
            if (!paused && !dialogue) {
                if(bulletClear.destroyBulletsHeight < position.y && bulletClear.bulletClearType == BulletClear.BulletClearType.FULLCLEAR) {
                    break;
                }
                createdObject = CreateBullet(template, position);
                recentObject.GetComponent<Bullet>().relatedSnake.Add(new Transform[] { createdObject.transform }); //TODO: Optimise
                recentObject = createdObject;
                i++;
                yield return null;
                yield return null;
            }
            yield return null;
        }
        recentObject.GetComponent<Bullet>().relatedSnake.Add(new Transform[] { });
    }
}
