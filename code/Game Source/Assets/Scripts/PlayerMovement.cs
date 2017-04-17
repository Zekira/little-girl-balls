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

    void Awake() {
        dialogueManager = GameObject.FindWithTag("LevelManager").GetComponent<DialogueManager>();
    }
	
	void Update () {
        if (transform.position.y > 2) {
            GlobalHelper.autoCollectItems = true;
        } else {
            GlobalHelper.autoCollectItems = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            GlobalHelper.paused = !GlobalHelper.paused;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) { //Updating focus is needed when unpausing
                focused = true;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                transform.GetChild(0).localScale = new Vector3(GlobalHelper.GetStats().hitboxRadius, GlobalHelper.GetStats().hitboxRadius, 1f);
            } else {
                focused = false;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        if (!GlobalHelper.paused) {
            //Check for going focused/unfocused
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) {
                focused = true;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                transform.GetChild(0).localScale = new Vector3(GlobalHelper.GetStats().hitboxRadius, GlobalHelper.GetStats().hitboxRadius, 1f);
            } else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) {
                focused = false;
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            }

            //Things that shouldn't happen when in deathanimation: movement & shot
            if (!GlobalHelper.GetStats().noMovement) {
                //Check what movement should happen
                moveLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) ? 1 : 0;
                moveRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) ? 1 : 0;
                moveUp = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) ? 1 : 0;
                moveDown = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) ? 1 : 0;

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
                if (Input.GetKey(KeyCode.Z) && !GlobalHelper.dialogue && shotCooldown <= 0) {
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
                    GlobalHelper.CreateBullet(shot, transform.position);
                    shotCooldown = 10;
                } else if (GlobalHelper.dialogue && Input.GetKeyDown(KeyCode.Z)) {
                    dialogueManager.advanceDialogue();
                }
            }
            if (shotCooldown > 0) {
                shotCooldown--;
            }

            //Debug stuff
            if (Input.GetKeyDown(KeyCode.Slash)) {
                Debug.Log(GameObject.FindWithTag("BulletParent").transform.childCount);
            }
        }
    }
}
