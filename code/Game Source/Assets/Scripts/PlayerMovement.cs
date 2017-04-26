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

    //TODO: Make these customisable
    public KeyCode keyPause = KeyCode.Escape;
    public KeyCode keyFocus = KeyCode.LeftShift;
    public KeyCode keyShoot = KeyCode.Z;
    public KeyCode keyBomb = KeyCode.X;
    public KeyCode keyLeft = KeyCode.LeftArrow;
    public KeyCode keyRight = KeyCode.RightArrow;
    public KeyCode keyUp = KeyCode.UpArrow;
    public KeyCode keyDown = KeyCode.DownArrow;
    public KeyCode keySkip = KeyCode.LeftControl;

    void Awake() {
        dialogueManager = GameObject.FindWithTag("LevelManager").GetComponent<DialogueManager>();
    }
	
	void Update () {
        if (Input.GetKeyDown(keyPause)) {
            GlobalHelper.paused = !GlobalHelper.paused;
            if (Input.GetKey(keyFocus)) { //Updating focus is needed when unpausing
                focused = true;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                transform.GetChild(0).localScale = new Vector3(GlobalHelper.GetStats().hitboxRadius, GlobalHelper.GetStats().hitboxRadius, 1f);
            } else {
                focused = false;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        if (!GlobalHelper.paused) {
            if (transform.position.y > 2) {
                GlobalHelper.autoCollectItems = true;
            } else {
                GlobalHelper.autoCollectItems = false;
            }
            //Check for going focused/unfocused
            if (Input.GetKeyDown(keyFocus)) {
                focused = true;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                transform.GetChild(0).localScale = new Vector3(GlobalHelper.GetStats().hitboxRadius, GlobalHelper.GetStats().hitboxRadius, 1f);
            } else if (Input.GetKeyUp(keyFocus)) {
                focused = false;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            }

            //Things that shouldn't happen when in deathanimation: movement, shot, and bombs
            if (!GlobalHelper.GetStats().noMovement) {
                if (Input.GetKeyDown(keyBomb) && !GlobalHelper.dialogue && GlobalHelper.GetStats().bombs > 0) { //todo: graphics + damage enemies
                    //Set the spellcard bonus to failure. Does basically nothing if there's no spell active except eat like .01ms
                    GlobalHelper.levelManager.GetComponent<SpellcardManager>().Fail();
                    GlobalHelper.GetStats().invincibility = 300;
                    GlobalHelper.GetStats().SetBombs((byte)(GlobalHelper.GetStats().bombs - 1), GlobalHelper.GetStats().bombpieces);
                    StartCoroutine(GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(0.3f, BulletClear.BulletClearType.BOMB,300));
                }
                //Check what movement should happen
                moveLeft = Input.GetKey(keyLeft) ? 1 : 0;
                moveRight = Input.GetKey(keyRight) ? 1 : 0;
                moveUp = Input.GetKey(keyUp) ? 1 : 0;
                moveDown = Input.GetKey(keyDown) ? 1 : 0;

                moveDirection = new Vector2(moveRight - moveLeft, moveUp - moveDown);

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
                if (Input.GetKey(keyShoot) && !GlobalHelper.dialogue && shotCooldown <= 0) {
                    BulletTemplate shot = new BulletTemplate();
                    shot.bulletDamage = 3;
                    shot.isHarmful = false;
                    shot.innerColor = new Color(0.2f, 0.5f, 1f);
                    shot.outerColor = new Color(0.2f, 0.5f, 1f);
                    shot.bulletID = 1;
                    shot.movement = new Vector2(Random.Range(-0.1f, 0.1f), 1f).normalized / 5f;
                    shot.scale = 0.3f;
                    shot.rotation = Random.Range(0f, 90f);
                    shot.rotationSpeed = Random.Range(-0.05f, 0.05f);
                    shot.clearImmune = true;
                    GlobalHelper.CreateBullet(shot, transform.position);
                    shotCooldown = 10;
                } else if (GlobalHelper.dialogue && Input.GetKey(keySkip)) {
                    dialogueManager.advanceDialogue();
                } else if (GlobalHelper.dialogue && Input.GetKeyDown(keyShoot)) {
                    dialogueManager.advanceDialogue();
                }
            }
            if (shotCooldown > 0) {
                shotCooldown--;
            }

            //Debug stuff
            if (Input.GetKeyDown(KeyCode.Slash)) {
                Debug.Log(GameObject.FindWithTag("BulletParent").transform.childCount + "/" + GlobalHelper.totalFiredBullets);
            }
        }
    }
}
