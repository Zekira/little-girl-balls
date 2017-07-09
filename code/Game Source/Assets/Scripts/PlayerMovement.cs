using UnityEngine;
using System.Collections;
/// <summary>
/// Processes all player input.
/// </summary>
public class PlayerMovement : MonoBehaviour {

    public float unfocusedSpeed = 0.1f;
    public float focusedSpeed = 0.04f;
    public bool focused = false;
    private const float oneOverSqrtOfTwo = 0.707106781f;
    private Vector2 moveDirection;
    private int moveLeft, moveRight, moveUp, moveDown;
    private float totalSpeedMultiplier;
    private int shotCooldown = 2;
    private DialogueManager dialogueManager;
    private BulletTemplate mainShot = new BulletTemplate();
    private SpriteAnimator animator;

    public Sprite[] moveLeftSprites, moveRightSprites,stationairySprites;    

    void Awake() {
        SaveLoad.LoadApplyConfig(); //Just in case

        animator = GetComponent<SpriteAnimator>();
        moveLeftSprites = SpriteAnimator.GetSprites(Resources.Load("Graphics/Sprites/Rachel_Left") as Texture2D);
        moveRightSprites = SpriteAnimator.GetSprites(Resources.Load("Graphics/Sprites/Rachel_Right") as Texture2D); //TODO: Not just have this as a flipped left.
        stationairySprites = SpriteAnimator.GetSprites(Resources.Load("Graphics/Sprites/Rachel_Stationairy") as Texture2D);

        dialogueManager = GameObject.FindWithTag("LevelManager").GetComponent<DialogueManager>();

        mainShot.bulletDamage = 2;
        mainShot.enemyShot = false;
        mainShot.innerColor = new Color(0.9f, 1f, 1f, 0.6f);
        mainShot.outerColor = new Color(0.2f, 0.7f, 0.7f, 0.6f);
        mainShot.bulletID = 3;
        mainShot.movement = new Vector2(0f, 0.2f);
        mainShot.scale = 0.4f;
        mainShot.clearImmune = true;
    }
	
	void Update () {
        //Check restart
        if (Input.GetKeyDown(Config.keyRestart)) {
            GlobalHelper.LoadLevel(GlobalHelper.level, GlobalHelper.difficulty);
        }
        //Check if pause
        if (Input.GetKeyDown(Config.keyPause)) {
            GlobalHelper.SetPaused(!GlobalHelper.paused);
        }
        //Interact with game world
        if (!GlobalHelper.paused) {
            if (transform.position.y > 2) {
                GlobalHelper.autoCollectItems = true;
            } else {
                GlobalHelper.autoCollectItems = false;
            }

            //Check for going focused/unfocused
            if (Input.GetKeyDown(Config.keyFocus)) {
                focused = true;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                transform.GetChild(0).localScale = new Vector3(GlobalHelper.stats.hitboxRadius, GlobalHelper.stats.hitboxRadius, 1f);
            } else if (Input.GetKeyUp(Config.keyFocus)) {
                focused = false;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            }

            //Things that shouldn't happen when in deathanimation: movement, shot, and bombs
            if (!GlobalHelper.stats.noMovement) {
                if (Input.GetKeyDown(Config.keyBomb) && !GlobalHelper.dialogue && GlobalHelper.stats.bombs > 0) { //todo: graphics
                    //Set the spellcard bonus to failure. Does basically nothing if there's no spell active except eat like .01ms
                    GlobalHelper.levelManager.GetComponent<SpellcardManager>().Fail();
                    GlobalHelper.stats.invincibility = 300;
                    GlobalHelper.stats.SetBombs((byte)(GlobalHelper.stats.bombs - 1), GlobalHelper.stats.bombpieces);
                    GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(0.3f, BulletClear.BulletClearType.BOMB,300);
                }
                //Check what movement should happen
                moveLeft = Input.GetKey(Config.keyLeft) ? 1 : 0;
                moveRight = Input.GetKey(Config.keyRight) ? 1 : 0;
                moveUp = Input.GetKey(Config.keyUp) ? 1 : 0;
                moveDown = Input.GetKey(Config.keyDown) ? 1 : 0;

                moveDirection = new Vector2(moveRight - moveLeft, moveUp - moveDown);

                //Set relevant sprites
                animator.SetSprites(stationairySprites);
                if (moveDirection.x < 0) {
                    animator.SetSprites(moveLeftSprites);
                } else if (moveDirection.x > 0) {
                    animator.SetSprites(moveRightSprites);
                }

                //Apply focused speed
                totalSpeedMultiplier = focused ? focusedSpeed : unfocusedSpeed;
                //Apply correct speed when going diagonally
                totalSpeedMultiplier *= (moveLeft + moveRight) * (moveUp + moveDown) > 0 ? oneOverSqrtOfTwo : 1f;
                //Change position; Domains: x: [-4,4], y: [-4.65,4.65]
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x + totalSpeedMultiplier * moveDirection.x, -4f, 4f),
                    Mathf.Clamp(transform.position.y + totalSpeedMultiplier * moveDirection.y, -4.65f, 4.65f),
                    transform.position.z);

                //Check whether the player is shooting or advancing dialogue.
                if (Input.GetKey(Config.keyShoot) && !GlobalHelper.dialogue && shotCooldown <= 0) {
                    GlobalHelper.CreateBullet(mainShot, transform.position);
                    shotCooldown = 6;
                } else if (GlobalHelper.dialogue && Input.GetKey(Config.keySkip)) {
                    dialogueManager.AdvanceDialogue();
                } else if (GlobalHelper.dialogue && Input.GetKeyDown(Config.keyShoot)) {
                    dialogueManager.AdvanceDialogue();
                }
            }
            if (shotCooldown > 0) {
                shotCooldown--;
            }

            //Debug stuff
            if (Input.GetKeyDown(KeyCode.Slash)) {
                Debug.Log(GlobalHelper.currentBullets + "/" + GlobalHelper.backupBullets.Count + "/" + GlobalHelper.totalFiredBullets);
            }
        }
    }

    public void UpdateFocused() {
        if (Input.GetKey(Config.keyFocus)) { 
            focused = true;
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
            transform.GetChild(0).localScale = new Vector3(GlobalHelper.stats.hitboxRadius, GlobalHelper.stats.hitboxRadius, 1f);
        } else {
            focused = false;
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
