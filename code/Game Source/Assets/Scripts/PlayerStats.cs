using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// A class made to keep track of different scores. In hindsight, this should've either been static and not monobehaviour, or a monobehaviour to some kind of master object.
/// </summary>
public class PlayerStats : MonoBehaviour {

    //Things that go on the right
    public enum Difficulty { EASY, NORMAL, HARD, LUNATIC, EXTRA};
    public Difficulty difficulty = Difficulty.EASY;
    public uint highscore = 0;
    public uint score = 0;
    [Range(0,6)]
    public byte lives = 3;
    [Range(0,2)]
    public byte lifepieces = 0;
    [Range(0,6)]
    public byte bombs = 2;
    [Range(0,3)]
    public byte bombpieces = 0;
    [Range(0,400)]
    public int power = 0;
    public uint value = 10000;
    public ushort graze = 0;

    public int invincibility = 0;
    public bool noMovement = false;
    public Vector3 startPosition;
    public float hitboxRadius = 0.15f;
    public float grazeRadius = 1f;

    public Sprite[] bombSprites = { null, null, null, null, null};
    public Sprite[] lifeSprites = { null, null, null, null };
    public Vector3 playerPosition;

    private GameObject UIVariable;

    void Awake() {
        UIVariable = GameObject.FindWithTag("UIVariable");
        //Setting the startposition
        startPosition = transform.position;
        //Initialising the bomb piece textures
        Texture2D texture;
        for (int i = 0; i <= 4; i++) {
            texture = (Texture2D)Resources.Load("Graphics/Pieces/Bomb" + i);
            bombSprites[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        //..and the life piece textures
        for (int i = 0; i <= 3; i++) {
            texture = (Texture2D)Resources.Load("Graphics/Pieces/Life" + i);
            lifeSprites[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        //Set the scores to their values
        SetLives(lives, lifepieces);
        SetBombs(bombs, bombpieces);
        SetHighscore(highscore);
        SetScore(score);
        SetPower(power, false);
    }

    void Update() {
        if (!GlobalHelper.paused) {
            playerPosition = transform.position;
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
            if (lives == 0) { //Getting hit with zero lives in stock is a bad idea.
                //Debug.Log("<b>Game over lul git good skrub</b>");
            } else {
                StartCoroutine(GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(0.3f, BulletClear.BulletClearType.SOME));
                SetLives(--lives, lifepieces);
                SetBombs(2, bombpieces);
                noMovement = true;
                transform.Find("DeathAnimation").gameObject.SetActive(true);
                invincibility = 210;
            }
        }
    }

    /// <summary>
    /// Sets the bomb count and updates the UI.
    /// </summary>
    /// <param name="total">Amount of full bombs</param>
    /// <param name="part">Amount of parts in the next bomb</param>
    public void SetBombs(byte total, byte part) {
        if (bombs == 6) { //You can't have bombpieces when you're already full.
            part = 0;
        }
        bombs = total > 6 ? (byte) 6 : total;
        bombpieces = part > 3 ? (byte) 3 : part;
        for (int i = 0; i < 6; i++) { //Beware the off-by-1-errors left there forever. (Could either make the code more readable, or make a joke. Obvious which one is better.)
            if (i < total) {
                GameObject.FindWithTag("UIBombs").transform.Find("Bomb" + i).GetComponent<Image>().sprite = bombSprites[4];
            } else if (i == total && part != 0) {
                GameObject.FindWithTag("UIBombs").transform.Find("Bomb" + i).GetComponent<Image>().sprite = bombSprites[part];
            } else {
                GameObject.FindWithTag("UIBombs").transform.Find("Bomb" + i).GetComponent<Image>().sprite = bombSprites[0];
            }
        }
        UIVariable.transform.Find("Spellpieces").GetComponent<Text>().text = part + "/4";
    }

    /// <summary>
    /// Sets the life count and updates the UI.
    /// Everything works the same as SetBombs();
    /// </summary>
    /// <param name="total">Amount of full lives</param>
    /// <param name="part">Amount of parts in the next life</param>
    public void SetLives(byte total, byte part) {
        if (lives == 6) {
            part = 0;
        }
        lives = total > 6 ? (byte)6 : total;
        lifepieces = part > 2 ? (byte)2 : part;
        for (int i = 0; i < 6; i++) { 
            if (i < total) {
                GameObject.FindWithTag("UILives").transform.Find("Life" + i).GetComponent<Image>().sprite = lifeSprites[3];
            } else if (i == total && part != 0) {
                GameObject.FindWithTag("UILives").transform.Find("Life" + i).GetComponent<Image>().sprite = lifeSprites[part];
            } else {
                GameObject.FindWithTag("UILives").transform.Find("Life" + i).GetComponent<Image>().sprite = lifeSprites[0];
            }
        }
        UIVariable.transform.Find("Lifepieces").GetComponent<Text>().text = part + "/3";
    }

    /// <summary>
    /// Sets the graze and updates the UI.
    /// </summary
    public void SetGraze(ushort amount) {
        graze = amount;
        UIVariable.transform.Find("Graze").GetComponent<Text>().text = GlobalHelper.Commafy(graze);
    }

    /// <summary>
    /// Adds 1 to graze and updates the UI.
    /// </summary>
    public void IncrementGraze() {
        graze++;
        UIVariable.transform.Find("Graze").GetComponent<Text>().text = GlobalHelper.Commafy(graze);
    }

    /// <summary>
    /// Sets the score to amount, updating the highscore if neccessary. Also updates the UI.
    /// </summary>
    public void SetScore(uint amount) {
        score = amount;
        UIVariable.transform.Find("Score").GetComponent<Text>().text = GlobalHelper.Commafy(score);
        if (score > highscore) {
            SetHighscore(score);
        }
    }

    /// <summary>
    /// Adds amount to score, updating the highscore if neccessary. Also updates the UI.
    /// </summary>
    public void AddScore(uint amount) {
        SetScore(score += amount);
    }

    /// <summary>
    /// Sets the power to up to 400, and gives a score bonus.
    /// </summary>
    public void SetPower(int amount) {
        SetPower(amount, true);
    }

    /// <summary>
    /// Sets the power up to 400, and optionally gives a score bonus.
    /// </summary>
    public void SetPower(int amount, bool worthScore) {
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
    public void AddPower(int amount) {
        SetPower(power + amount);
    }

    /// <summary>
    /// Sets the highscore to amount; usually unneccessary as this is also done by SetScore() and AddScore().
    /// </summary>
    public void SetHighscore(uint amount) {
        highscore = amount;
        UIVariable.transform.Find("HighScore").GetComponent<Text>().text = GlobalHelper.Commafy(highscore);
    }
}
