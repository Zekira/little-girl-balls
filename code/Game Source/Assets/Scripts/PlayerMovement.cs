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
    private BulletTemplate mainShot = BulletTemplate.basic;
    private BulletTemplate subShot = BulletTemplate.basic;
    private SpriteAnimator animator;
    private Transform thisTransform;

    public Sprite[] moveLeftSprites, moveRightSprites,stationairySprites;    

    void Awake() {
        focused = false;
        thisTransform = transform;
        SaveLoad.LoadApplyConfig(); //Just in case

        animator = GetComponent<SpriteAnimator>();
        moveLeftSprites = SpriteAnimator.GetSprites(Resources.Load("Graphics/Sprites/Rachel_Left") as Texture2D);
        moveRightSprites = SpriteAnimator.GetSprites(Resources.Load("Graphics/Sprites/Rachel_Right") as Texture2D); //TODO: Not just have this as a flipped left.
        stationairySprites = SpriteAnimator.GetSprites(Resources.Load("Graphics/Sprites/Rachel_Stationairy") as Texture2D);

        dialogueManager = GlobalHelper.levelManager.GetComponent<DialogueManager>();

        mainShot.enemyShot = false;
        subShot.enemyShot = false;
        mainShot.clearImmune = true;
        subShot.clearImmune = true;
        switch (GlobalHelper.character) {
            case GlobalHelper.Character.RACHEL_A:
                mainShot.bulletDamage = 20;
                mainShot.innerColor = new Color(0.9f, 1f, 1f, 0.8f);
                mainShot.outerColor = new Color(0.2f, 0.7f, 0.7f, 0.8f);
                mainShot.bulletID = 3;
                mainShot.movement = new Vector2(0f, 0.2f);
                mainShot.scale = 0.4f;

                subShot.bulletDamage = 4;
                subShot.innerColor = new Color(0.9f, 1f, 1f, 0.6f);
                subShot.outerColor = new Color(0.1f, 0.6f, 0.6f, 0.6f);
                subShot.bulletID = 3;
                subShot.movement = new Vector2(0f, 0.2f);
                subShot.scale = 0.25f;
                break;
            case GlobalHelper.Character.RACHEL_B:
                mainShot.bulletDamage = 12;
                mainShot.innerColor = new Color(0.9f, 1f, 1f, 0.8f);
                mainShot.outerColor = new Color(0.2f, 0.7f, 0.7f, 0.6f);
                mainShot.bulletID = 3;
                mainShot.scale = 0.4f;

                subShot.bulletDamage = 2;
                subShot.innerColor = new Color(0.9f, 1f, 1f, 0.6f);
                subShot.outerColor = new Color(0.1f, 0.6f, 0.6f, 0.6f);
                subShot.bulletID = 3;
                subShot.scale = 0.25f;
                break;
        }
    }
	
	void Update () {
        //Check restart
        if (Input.GetKeyDown(Config.keyRestart)) {
            SceneSwitcher.LoadLevel(PlayerStats.firstStage, GlobalHelper.difficulty, ReplayManager.isReplay);
        }
        //Check if pause
        if (Input.GetKeyDown(Config.keyPause)) {
            GlobalHelper.SetPaused(!GlobalHelper.paused);
        }
        //Interact with game world
        if (!GlobalHelper.paused) {
            if (PlayerPosGetter.playerPos.y > 2) {
                GlobalHelper.autoCollectItems = true;
            } else {
                GlobalHelper.autoCollectItems = false;
            }

            //Things that should ONLY happen when not in replays: All game input handling.
            if (!ReplayManager.isReplay) {

                //Check for going focused/unfocused
                CheckFocus(Input.GetKeyDown(Config.keyFocus), Input.GetKeyUp(Config.keyFocus));

                //Things that shouldn't happen when in deathanimation: movement, shot, and bombs
                if (!PlayerStats.noMovement) {
                    //Bombs
                    CheckBomb(Input.GetKeyDown(Config.keyBomb));

                    //Movement
                    CheckMove(Input.GetKey(Config.keyLeft), Input.GetKey(Config.keyRight), Input.GetKey(Config.keyUp), Input.GetKey(Config.keyDown));

                    //Check whether the player is shooting or advancing dialogue.
                    CheckShootOrSkip(Input.GetKey(Config.keyShoot), Input.GetKey(Config.keySkip), Input.GetKeyDown(Config.keyShoot));
                }
            }
            if (shotCooldown > 0) {
                shotCooldown--;
            }

            //Debug stuff
            if (Input.GetKeyDown(KeyCode.Slash)) {
                Debug.Log(GlobalHelper.currentBullets + "/" + GlobalHelper.backupBullets.Count + "/" + GlobalHelper.totalFiredBullets);
                Debug.Log(PlayerStats.score);
            }
        }
    }

    /// <summary>
    /// Move the player in one of the eight directions depending on the four inputs.
    /// </summary>
    public void CheckMove(bool left, bool right, bool up, bool down) {
        //Check what movement should happen
        moveLeft = left ? 1 : 0;
        moveRight = right ? 1 : 0;
        moveUp = up ? 1 : 0;
        moveDown = down ? 1 : 0;

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
        thisTransform.position = new Vector3(
            Mathf.Clamp(PlayerPosGetter.playerPos.x + totalSpeedMultiplier * moveDirection.x, -4f, 4f),
            Mathf.Clamp(PlayerPosGetter.playerPos.y + totalSpeedMultiplier * moveDirection.y, -4.65f, 4.65f),
            PlayerPosGetter.playerPos.z);
    }

    /// <summary>
    /// When the player presses the bomb button. This checks whether the bomb is valid, and if so, goes off.
    /// </summary>
    public void CheckBomb(bool bomb) {
        if (bomb && !GlobalHelper.dialogue && PlayerStats.bombs > 0) { //todo: graphics
            //Set the spellcard bonus to failure. Does basically nothing if there's no spell active except eat like .01ms
            GlobalHelper.levelManager.GetComponent<SpellcardManager>().Fail();

            GlobalHelper.stats.invincibility = 300;
            PlayerStats.SetBombs((byte)(PlayerStats.bombs - 1), PlayerStats.bombpieces);
            GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(0.3f, BulletClear.BulletClearType.BOMB, 300);
        }
    }

    /// <summary>
    /// Checks 
    /// </summary>
    public void CheckShootOrSkip(bool shoot, bool skip, bool shootdown) {
        if (shoot && !GlobalHelper.dialogue && shotCooldown <= 0) {
            Shoot();
        } else if (skip && GlobalHelper.dialogue) {
            dialogueManager.AdvanceDialogue();
        } else if (shootdown && GlobalHelper.dialogue) {
            dialogueManager.AdvanceDialogue();
        }
    }

    public void CheckFocus(bool down, bool up) {
        if (down) {
            focused = true;
            thisTransform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
            thisTransform.GetChild(0).localScale = new Vector3(PlayerStats.hitboxRadius, PlayerStats.hitboxRadius, 1f);
        } else if (up) {
            focused = false;
            thisTransform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void UpdateFocused() {
        if (Input.GetKey(Config.keyFocus)) { 
            focused = true;
            thisTransform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
            thisTransform.GetChild(0).localScale = new Vector3(PlayerStats.hitboxRadius, PlayerStats.hitboxRadius, 1f);
        } else {
            focused = false;
            thisTransform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void Shoot() {
        switch (GlobalHelper.character) {
            case GlobalHelper.Character.RACHEL_A:
                ThingCreator.CreateBullet(mainShot, PlayerPosGetter.playerPos);
                if (PlayerStats.power >= 200) {
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos + new Vector3(-0.2f, -0.1f, 0f));
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos + new Vector3(0.2f, -0.1f, 0f));
                }
                if (PlayerStats.power == 400) {
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos + new Vector3(-0.4f, -0.2f, 0f));
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos + new Vector3(0.4f, -0.2f, 0f));
                } else if (PlayerStats.power >= 300) {
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos + new Vector3(0f, -0.3f, 0f));
                }
                if (PlayerStats.power >= 100 && PlayerStats.power < 200) {
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos + new Vector3(0f, -0.3f, 0f));
                }
                shotCooldown = 6;
                break;
            case GlobalHelper.Character.RACHEL_B:
                mainShot.movement = new Vector2(0f, 0.2f);
                mainShot.rotation = 0f;
                ThingCreator.CreateBullet(mainShot, PlayerPosGetter.playerPos);
                mainShot.movement = new Vector2(0.1414f, 0.1414f);
                mainShot.rotation = 0.7853f;
                ThingCreator.CreateBullet(mainShot, PlayerPosGetter.playerPos);
                mainShot.rotation = -0.7853f;
                mainShot.movement = new Vector2(-0.1414f, 0.1414f);
                ThingCreator.CreateBullet(mainShot, PlayerPosGetter.playerPos);
                if (PlayerStats.power == 400) {
                    subShot.movement = new Vector2(0.031f, 0.197f);
                    subShot.rotation = 0.157f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.031f, 0.197f);
                    subShot.rotation = -0.157f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);

                    subShot.movement = new Vector2(0.062f, 0.190f);
                    subShot.rotation = 0.314f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.062f, 0.190f);
                    subShot.rotation = -0.314f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);

                    subShot.movement = new Vector2(0.091f, 0.178f);
                    subShot.rotation = 0.471f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.091f, 0.178f);
                    subShot.rotation = -0.471f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);

                    subShot.movement = new Vector2(0.112f, 0.162f);
                    subShot.rotation = 0.628f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.112f, 0.162f);
                    subShot.rotation = -0.628f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                } else if (PlayerStats.power >= 300) {
                    subShot.movement = new Vector2(0.039f, 0.196f);
                    subShot.rotation = 0.196f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.039f, 0.196f);
                    subShot.rotation = -0.196f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);

                    subShot.movement = new Vector2(0.077f, 0.185f);
                    subShot.rotation = 0.392f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.077f, 0.185f);
                    subShot.rotation = -0.392f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);

                    subShot.movement = new Vector2(0.111f, 0.166f);
                    subShot.rotation = 0.589f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.111f, 0.166f);
                    subShot.rotation = -0.589f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                } else if (PlayerStats.power >= 200) {
                    subShot.movement = new Vector2(0.052f, 0.193f);
                    subShot.rotation = 0.261f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.052f, 0.193f);
                    subShot.rotation = -0.261f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);

                    subShot.movement = new Vector2(0.1f, 0.173f);
                    subShot.rotation = 0.523f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.1f, 0.173f);
                    subShot.rotation = -0.523f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                } else if (PlayerStats.power >= 100) {
                    subShot.movement = new Vector2(0.077f, 0.185f);
                    subShot.rotation = 0.392f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                    subShot.movement = new Vector2(-0.077f, 0.185f);
                    subShot.rotation = -0.392f;
                    ThingCreator.CreateBullet(subShot, PlayerPosGetter.playerPos);
                }
                shotCooldown = 8;
                break;
        }
    }
}
