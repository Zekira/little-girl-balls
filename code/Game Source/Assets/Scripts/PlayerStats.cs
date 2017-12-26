using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// A class made to keep track of different scores. In hindsight, this should've either been static and not monobehaviour, or a monobehaviour to some kind of master object.
/// </summary>
public class PlayerStats : MonoBehaviour
{

    //Things that go on the right
    public static ulong highscore = 0;
    public static ulong[] stageHighScore = { 0, 0, 0, 0, 0, 0, 0 }; //TODO: do stuff with this
    public static ulong score = 0;
    public static byte lives = 3;
    public const byte piecesToLife = 3;
    public static byte lifepieces = 0;
    public static byte bombs = 2;
    public const byte piecesToBomb = 4;
    public static byte bombpieces = 0;
    public static int power = 0;
    public static uint value = 10000;
    public static int graze = 0;
    public static int grazeInATick = 0;

    //Other stuff
    public int invincibility = 0;
    public static bool noMovement = false;
    public static int firstStage;
    public static Vector3 respawnPosition;
    public static float hitboxRadius = 0.15f;
    public static float grazeRadius = 1f;
    public static uint totalTimePlayed = 0;
    public static uint timePlayed = 0;

    public static Sprite[] bombSprites = { null, null, null, null, null }; //Both set in the inspector
    public static Sprite[] lifeSprites = { null, null, null, null };

    private static GameObject UIVariable;

    void OnEnable() {
        noMovement = false; //This otherwise never gets reset
    }

    void Start() {
        UIVariable = GlobalHelper.uiVariable.gameObject; //This needs to update no matter what.
        //Initialising the bomb piece textures
        Texture2D texture;
        for (int i = 0; i <= piecesToBomb; i++) {
            texture = (Texture2D)Resources.Load("Graphics/Pieces/Bomb" + i);
            bombSprites[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        //..and the life piece textures
        for (int i = 0; i <= piecesToLife; i++) {
            texture = (Texture2D)Resources.Load("Graphics/Pieces/Life" + i);
            lifeSprites[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        //Set the scores to their values
        if (!ReplayManager.isReplay) {
            SetLives(3, 0);
            SetBombs(2, 0);
            SetHighscore(highscore);
            SetScore(0);
            SetPower(0, false);
            SetGraze(0);
            SetValue(10000);
            firstStage = GlobalHelper.level;
        } else { //it's a replay.
            SetLives((byte)(ReplayManager.currentReplay.lives[GlobalHelper.level] / piecesToLife),
                (byte)(ReplayManager.currentReplay.lives[GlobalHelper.level] % piecesToLife));
            SetBombs((byte)(ReplayManager.currentReplay.bombs[GlobalHelper.level] / piecesToBomb),
                (byte)(ReplayManager.currentReplay.bombs[GlobalHelper.level] % piecesToBomb));
            SetPower(ReplayManager.currentReplay.power[GlobalHelper.level] * 5);
            SetGraze(ReplayManager.currentReplay.graze[GlobalHelper.level]);
            SetValue(ReplayManager.currentReplay.value[GlobalHelper.level]);
        }
        //Setting the startposition
        respawnPosition = transform.position;
    }

    void Update() {
        if (!GlobalHelper.paused) {
            if ((totalTimePlayed & 7) == 0) { //periodically save
                SaveLoad.SavePlayerData(GlobalHelper.character);
            }
            totalTimePlayed++;
            timePlayed++;
            if (grazeInATick > 0) {
                AudioManager.QueueSound(AudioManager.SFX.GRAZE);
                SetGraze(graze + grazeInATick);
                grazeInATick = 0;
            }
            //Invincibility cooldown.
            if (invincibility > 0) {
                invincibility--;
                if (noMovement || invincibility % 8 > 4) {
                    transform.GetComponent<SpriteRenderer>().enabled = false;
                } else {
                    transform.GetComponent<SpriteRenderer>().enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Starts what should happen when you take damage: losing hp and starting the DeathAnimation.
    /// </summary>
    public void TakeDamage() {
        //Stuff should only happen when not invincible
        if (invincibility <= 0) {
            //Set the spellcard bonus to failure. Does basically nothing if there's no spell active except eat like .01ms
            GlobalHelper.levelManager.GetComponent<SpellcardManager>().Fail();
            StartCoroutine(GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(0.3f, BulletClear.BulletClearType.DEATH));
            noMovement = true;
            transform.Find("DeathAnimation").gameObject.SetActive(true);
            invincibility = 210;
            AudioManager.QueueSound(AudioManager.SFX.HIT);
            if (lives == 0) { //Getting hit with zero lives in stock is a bad idea.
                Debug.Log("[Info] <b>Game over lul git good skrub</b>");
            } else {
                SetLives(--lives, lifepieces);
                SetBombs(2, bombpieces);
                Vector3 position;
                for (int i = 0; i < 25 && i < power; i += 5) {
                    position = PlayerPosGetter.playerPos + new Vector3(-0.25f + (float)GlobalHelper.random.NextDouble() / 2f, -0.25f + (float)GlobalHelper.random.NextDouble() / 2f, 2f + 0.01f * (float)GlobalHelper.random.NextDouble() - 0.01f);
                    ThingCreator.CreateItem(Item.ItemType.POWER, position);
                }
                SetPower(power > 50 ? power - 50 : 0);
            }
        }
    }

    /// <summary>
    /// Sets the bomb count and updates the UI.
    /// </summary>
    public static void SetBombs(byte total, byte part) {
        if (bombs == 6) { //You can't have bombpieces when you're already full.
            part = 0;
        }
        bombs = total > 6 ? (byte)6 : total;
        bombpieces = part > (piecesToBomb - 1) ? (byte)(piecesToBomb - 1) : part;
        for (int i = 0; i < 6; i++) { //Beware the off-by-1-errors left there forever. (Could either make the code more readable, or make a joke. Obvious which one is better.)
            if (i < total) {
                GameObject.FindWithTag("UIBombs").transform.Find("Bomb" + i).GetComponent<Image>().sprite = bombSprites[4];
            } else if (i == total && part != 0) {
                GameObject.FindWithTag("UIBombs").transform.Find("Bomb" + i).GetComponent<Image>().sprite = bombSprites[part];
            } else {
                GameObject.FindWithTag("UIBombs").transform.Find("Bomb" + i).GetComponent<Image>().sprite = bombSprites[0];
            }
        }
        UIVariable.transform.Find("Spellpieces").GetComponent<Text>().text = part + "/" + piecesToBomb;
    }

    /// <summary>
    /// Sets the life count and updates the UI.
    /// Everything works the same as SetBombs();
    /// </summary>
    private static void SetLives(byte total, byte part) {
        if (lives == 6) {
            part = 0;
        }
        lives = total > 6 ? (byte)6 : total;
        lifepieces = part > (piecesToLife - 1) ? (byte)(piecesToLife - 1) : part;
        for (int i = 0; i < 6; i++) {
            if (i < total) {
                GameObject.FindWithTag("UILives").transform.Find("Life" + i).GetComponent<Image>().sprite = lifeSprites[3];
            } else if (i == total && part != 0) {
                GameObject.FindWithTag("UILives").transform.Find("Life" + i).GetComponent<Image>().sprite = lifeSprites[part];
            } else {
                GameObject.FindWithTag("UILives").transform.Find("Life" + i).GetComponent<Image>().sprite = lifeSprites[0];
            }
        }
        UIVariable.transform.Find("Lifepieces").GetComponent<Text>().text = part + "/" + piecesToLife;
    }
    public static void AddLife() {
        SetLives((byte)(lives + 1), lifepieces);
        AudioManager.QueueSound(AudioManager.SFX.EXTEND);
    }
    public static void AddLifePiece() {
        SetLives(lives, (byte)(lifepieces + 1));
        if (lifepieces + 1 == piecesToLife) {
            AudioManager.QueueSound(AudioManager.SFX.EXTEND);
        }
    }

    /// <summary>
    /// Sets the graze and updates the UI.
    /// </summary
    private static void SetGraze(int amount) {
        graze = amount;
        UIVariable.transform.Find("Graze").GetComponent<Text>().text = NumberFunctions.Commafy(graze);
    }

    /// <summary>
    /// Adds 1 to graze and updates the UI.
    /// </summary>
    private static void IncrementGraze() {
        graze++;
        UIVariable.transform.Find("Graze").GetComponent<Text>().text = NumberFunctions.Commafy(graze);
    }

    /// <summary>
    /// Grazes if alive. Use this instead of incrementgraze if you want to wait with registering it until the end of the tick.
    /// </summary>
    public static void Graze() {
        if (!noMovement) {
            grazeInATick++;
        }
    }

    /// <summary>
    /// Sets the score to amount, updating the highscore if neccessary. Also updates the UI.
    /// </summary>
    public static void SetScore(ulong amount) {
        score = amount;
        UIVariable.transform.Find("Score").GetComponent<Text>().text = NumberFunctions.Commafy(score);
        if (score > highscore) {
            SetHighscore(score);
        }
    }

    /// <summary>
    /// Adds amount to score, updating the highscore if neccessary. Also updates the UI.
    /// </summary>
    public static void AddScore(uint amount) {
        SetScore(score += amount);
    }

    /// <summary>
    /// Sets the power to up to 400, and gives a score bonus.
    /// </summary>
    public static void SetPower(int amount) {
        SetPower(amount, true);
    }

    /// <summary>
    /// Sets the power up to 400, and optionally gives a score bonus.
    /// </summary>
    public static void SetPower(int amount, bool worthScore) {
        if (amount <= 400) {
            power = amount;
            UIVariable.transform.Find("PowerLarge").GetComponent<Text>().text = (power / 100).ToString();
            UIVariable.transform.Find("PowerSmall").GetComponent<Text>().text = "." + ((power % 100) / 10) + ((power % 100) % 10);
            if (worthScore) {
                AddScore(100);
            }
        } else {
            power = 400;
            if (worthScore) {
                AddScore(10000);
            }
        }
    }

    /// <summary>
    ///  Adds amount to the power, up to 400, or gives a bonus if it's already 400. Also updates the UI.
    /// </summary>
    public static void AddPower(int amount) {
        int powerMod100 = power % 100;
        if (powerMod100 + amount >= 100) { //If transitioning across a 1.00-power boundary
            AudioManager.QueueSound(AudioManager.SFX.POWERUP);
        }
        SetPower(power + amount);
    }

    /// <summary>
    /// Sets the highscore to amount; usually unneccessary as this is also done by SetScore() and AddScore().
    /// </summary>
    public static void SetHighscore(ulong amount) {
        highscore = amount;
        UIVariable.transform.Find("HighScore").GetComponent<Text>().text = NumberFunctions.Commafy(highscore);
    }

    public static void SetValue(uint amount) {
        value = (amount / 10) * 10; //End in a nice round number. Can't have scores ending in not-0, right?
        UIVariable.transform.Find("Value").GetComponent<Text>().text = NumberFunctions.Commafy(value);
    }
}
