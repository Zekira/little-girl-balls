using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// A class made to make my life easier.
/// Communicates with almost literally everything.
/// [later edit:] how to spaghetti 101
/// </summary>
public class GlobalHelper : MonoBehaviour {

    //Things needed all the time that make (some) sense even when not in a level.
    public static System.Random random = new System.Random(); //NOTE: Handle ALL random events through this; if I want to be able to add replays, I should save the seeds and input them here.
    public static ulong totalFiredBullets; //Fun statistic to keep track of.
    public static ulong previousFiredBullets;
    public static int currentBullets;

    public static bool paused = false;

    public static int level = 0;
    public enum Difficulty { EASY, NORMAL, HARD, LUNATIC, EXTRA };
    public static Difficulty difficulty = Difficulty.LUNATIC;
    public enum Character { RACHEL_A, RACHEL_B, RACHEL_C, WHATEVER_A, WHATEVER_B, WHATEVER_C };
    public static Character character = Character.RACHEL_A;
    public static byte musicHeard = 0; //See SaveLoad.cs for more info
    public static byte[] bestUnlockedStage = new byte[] { 0, 0, 0, 0 };
    public static short mainAttempts = 0;
    public static short mainFinishes = 0;
    public static short extraAttempts = 0;
    public static short extraFinishes = 0;
    public static int randomSeed;

    //Things set via the inspector
    public Transform tspellcardBackground, tcanvas, tbossUI, tlevelManager, tuiVariable,t3d;

    //These things only make sense in a level, so they're defined, but initialised in Awake()
    public static Transform spellcardBackground,canvas,secondCounter,msecondCounter, uiVariable;
    public static GameObject bossUI, player, levelManager, parent3d;
    public static List<GameObject> backupBullets, backupItems;
    public static List<Sprite> itemSprites, bulletSprites, snakeSprites;
    public static List<Sprite[]> enemySprites;
    public static PlayerStats stats;
    public static BulletClear bulletClear;
    public static CharacterPortraits characterPortraits;
    public static AudioManager audioManager; //Set through the audiomanager class itself
    public static bool dialogue, autoCollectItems;
    public static int activeBosses;
    public static GlobalHelper thisHelper;

    public static void SetGameFramerate() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

   //Things that make finding objects in other classes easier, but only make sense when in a level: the only time GlobalHelper is a script attached to an object.
    //Also sets up things needed for the level and such as this only runs when loading the level.
    void Awake() {
        SetGameFramerate();
        if (!ReplayManager.isReplay) {
            randomSeed = random.Next();
            random = new System.Random(randomSeed);
        } else {
            random = new System.Random(ReplayManager.currentReplay.seed[level]);
        }
        thisHelper = this;
        SaveLoad.LoadPlayerData(character);
        spellcardBackground = tspellcardBackground;
        canvas = tcanvas;
        bossUI = tbossUI.gameObject;
        secondCounter = bossUI.transform.Find("TimerSeconds");
        msecondCounter = bossUI.transform.Find("TimerMilliseconds");
        player = GameObject.FindWithTag("Player");
        levelManager = tlevelManager.gameObject;
        uiVariable = tuiVariable;
        parent3d = t3d.gameObject;

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

        activeBosses = 0;

        if (difficulty != Difficulty.EXTRA) {
            bestUnlockedStage[(int)difficulty] = (byte)Mathf.Max(level-1, bestUnlockedStage[(int)difficulty]);
        }

        uiVariable.Find("Difficulty").GetComponent<RawImage>().texture = (Texture2D)Resources.Load("Graphics/Difficulty/" + (int)difficulty);

        dialogue = false;
        autoCollectItems = false;
        paused = false;
    
    }

    public static int GetCharacterType(Character character) {
        return (int)character <= 2 ? 0 : 1; //Returns 0 when playing as rachel, 1 when playing as <whatever>
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

    public static void SetPaused(bool paused) {
        Menu.previousSelectedMenuItems = new List<Transform>();
        GlobalHelper.paused = paused;
        player.GetComponent<PlayerMovement>().UpdateFocused(); //Updating focus is needed when unpausing, otherwise it wouldn't register releasing/holding the button during the pause
        canvas.Find("Pause Canvas").gameObject.SetActive(paused);
        canvas.Find("Dialogue Canvas").gameObject.SetActive(/*dialogue &&*/ !paused); //It's empty anyways if there's no dialogue going on, so no need to hide it then
        if (paused) {
            audioManager.PauseMusic();
        } else {
            audioManager.UnpauseMusic();
        }
    }

    public void CreateSnake (int length, BulletTemplate template, Vector3 position) {
        StartCoroutine(CoCreateSnake(length, template, position));
    }

    private static GameObject createdObject;

    private IEnumerator CoCreateSnake(int length, BulletTemplate template, Vector3 position) {
        //First one manually
        GameObject recentObject = ThingCreator.CreateBullet(template, position);
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
                createdObject = ThingCreator.CreateBullet(template, position);
                recentObject.GetComponent<Bullet>().relatedSnake.Add(createdObject.transform);
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
