﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// A class reading .txt's describing either enemy or bullet info. Important in those .txt's is the wait(x) function, which is in ticks and not second, because second would not allow replays.
/// </summary>
public class TimelineInterprenter : MonoBehaviour {
    public string patternPath = "";
    public bool levelTimeline = false; //Set via inspector
    private Dictionary<int, float> numberVars = new Dictionary<int, float>();
    private Dictionary<int, BulletTemplate> bulletTemplateVars = new Dictionary<int, BulletTemplate>();
    private Dictionary<int, EnemyTemplate> enemyTemplateVars = new Dictionary<int, EnemyTemplate>();
    private Dictionary<int, LaserTemplate> laserTemplateVars = new Dictionary<int, LaserTemplate>();
    private static Dictionary<int, float> globalNumberVars = new Dictionary<int, float>();
    private static Dictionary<int, BulletTemplate> globalBulletTemplateVars = new Dictionary<int, BulletTemplate>();
    private static Dictionary<int, EnemyTemplate> globalEnemyTemplateVars = new Dictionary<int, EnemyTemplate>();
    private static Dictionary<int, LaserTemplate> globalLaserTemplateVars = new Dictionary<int, LaserTemplate>();
    private List<int> repeatStepback = new List<int>(); //What line to go to when encountering an endrepeat.
    private Bullet parentBullet;
    private Enemy parentEnemy;
    private int currentLine = 0;
    private int cooldown = 0;
    private List<TimelineCommand> commands = new List<TimelineCommand>();

    //Vars needed within the for loop. Static because they don't get used for multiple ticks, so they can be reused, so it's useless to create these for every object needing the interprenter.
    private static int count, layers, findEndRepeatLine, lineDifference;
    private static float num1, num2;
    private static TimelineCommand currentCommand;
    private static BulletTemplate bulletTemplate;
    private static BulletTemplate parentTemplate;
    private static EnemyTemplate enemyTemplate;
    private static Vector3 pos, playerpos;
    private static int stringHash;
    private static bool ifevaluation;
    private static List<TimelineCommand> enemyCommands = new List<TimelineCommand>();

    public void Reset(string newTimeLine) {
        patternPath = newTimeLine;
        numberVars.Clear();
        bulletTemplateVars.Clear();
        enemyTemplateVars.Clear();
        repeatStepback = new List<int>();
        parentBullet = transform.GetComponent<Bullet>();
        parentEnemy = transform.GetComponent<Enemy>();
        currentLine = 0;
        cooldown = 0;
        ReadAttack(true);
    }

    void OnDestroy() {
        //Start() to add the TickTimeline to the tick all timelines delegate, OnDestroy() to remove them.
        GlobalHelper.Tick -= TickTimeline;
    }

    void Start() {
        GlobalHelper.Tick += TickTimeline;
        if (levelTimeline) {
            patternPath = "Timelines/Stages/Stage" + GlobalHelper.level + "_" + ((int)GlobalHelper.difficulty);
        }
        parentEnemy = transform.GetComponent<Enemy>();
        if (patternPath == "") {
            patternPath = parentEnemy.template.attackPath[0];
        }
        ReadAttack(true);
    }

    void Update() {
        if (!GlobalHelper.paused && !GlobalHelper.dialogue) {
            if (cooldown == 0) {
                //+1 because it would otherwise start at the line it terminated at (only "wait(x)"), resulting in an infinite loop, which is a nightmare. That is longer than 12 seconds.
                currentLine++;
                ReadAttack();
            }
            cooldown = cooldown >= 0 ? cooldown - 1 : 0;
        }
    }

    /// <summary>
    /// Increases the currentLine by one and ReadAttack()'s.
    /// </summary>
    public void TickTimeline() {
        currentLine++;
        ReadAttack();
    }

    /// <summary>
    /// Reads the text (defined in patternPath) line by line and does stuff depending on the info.
    /// The syntax of those lines is usually <functionname>([argument[,other arguments .. ]]);
    /// Any text evaluated should be all-lowercase.
    /// Set "initialise" to true to reset all values before starting reading.
    /// </summary>
    public void ReadAttack(bool initialise) {
        if (initialise) {
            commands = TimelineCommand.GetCommands(patternPath);
        }
        ReadAttack();
    }

     /// <summary>
     /// Reads the text (defined in patternPath) line by line and does stuff depending on the info.
     /// The syntax of those lines is usually <functionname>([argument[,other arguments .. ]]);
     /// Any text evaluated should be all-lowercase.
     /// </summary>
    public void ReadAttack() { 
        for (; currentLine < commands.Count; currentLine++) {
            currentCommand = commands[currentLine];
            //Take the part before the brackets and try to figure out what it says and do something with it.
            switch (currentCommand.command) {
                case TimelineCommand.Command.STARTTIMELINE:
                    //Attaches ANOTHER TimelineInterprenter to this GameObject with path args[0].
                    TimelineInterprenter newpattern = transform.gameObject.AddComponent<TimelineInterprenter>();
                    newpattern.patternPath = currentCommand.args[0];
                    continue;
                case TimelineCommand.Command.DIALOGUE:
                    GlobalHelper.levelManager.GetComponent<DialogueManager>().StartDialogue(currentCommand.args[0]);
                    cooldown = 1;
                    return; //The next loop should not happen immediatly, but after the dialogue has been processed. That's checked within Update().
                case TimelineCommand.Command.REPEAT:
                    //Executes everything between here and the matching endrepeat args[0] times.
                    count = Mathf.RoundToInt(ParseValue(currentCommand.args[0])) - 1;
                    layers = 0; //Goes down for every Repeat(x) line. Goes up for every Endrepeat line. When it reaches "1", it hit the right endrepeat.
                    for (findEndRepeatLine = currentLine + 1; layers != 1; findEndRepeatLine++) {
                        if (commands[findEndRepeatLine].command == TimelineCommand.Command.REPEAT) {
                            layers--;
                        } else if (commands[findEndRepeatLine].command == TimelineCommand.Command.ENDREPEAT) {
                            layers++;
                        }
                    }
                    lineDifference = 1 + currentLine - findEndRepeatLine; //This should be how much to go back after hitting "endrepeat". Add 1 because it's 1 off and would infinite-loop.
                    repeatStepback.Add(0); //After hitting it the final time it should go to after the end. Because of the loops ++, going to exactly the end is fine.
                    for (int i = 0; i < count; i++) {
                        repeatStepback.Add(lineDifference);
                    }
                    continue;
                case TimelineCommand.Command.ENDREPEAT:
                    currentLine += repeatStepback[repeatStepback.Count - 1];
                    repeatStepback.RemoveAt(repeatStepback.Count - 1);
                    continue;
                case TimelineCommand.Command.IF:
                    //If false: continu to this level's "else", +1 line
                    //If true : continu until else
                    layers = 0;
                    //Find out "ifevaluation". Format of text: if(var1,comparator,var2);
                    switch (currentCommand.args[1]) {
                        case "==":
                        case "equal":
                            ifevaluation = ParseValue(currentCommand.args[0]) == ParseValue(currentCommand.args[2]);
                            break;
                        case "!=":
                        case "notequal":
                            ifevaluation = ParseValue(currentCommand.args[0]) != ParseValue(currentCommand.args[2]);
                            break;
                        case ">":
                        case "larger":
                            ifevaluation = ParseValue(currentCommand.args[0]) > ParseValue(currentCommand.args[2]);
                            break;
                        case "<":
                        case "smaller":
                            ifevaluation = ParseValue(currentCommand.args[0]) < ParseValue(currentCommand.args[2]);
                            break;
                        case ">=":
                        case "largerequal":
                            ifevaluation = ParseValue(currentCommand.args[0]) >= ParseValue(currentCommand.args[2]);
                            break;
                        case "<=":
                        case "smallerequal":
                            ifevaluation = ParseValue(currentCommand.args[0]) <= ParseValue(currentCommand.args[2]);
                            break;
                        default:
                            ifevaluation = false;
                            break;
                    }
                    if (ifevaluation) {
                        //currentLine++; //Next line is the correct one. No ++ needed, the for loop does that.
                        continue;
                    } else {
                        for (findEndRepeatLine = currentLine + 1; //Goes down for every "if" line, goes up every "endif" line, "else" does not affect it. When it reaches "1", it hits the right else / endif, check which
                            !(layers >= 0 && commands[findEndRepeatLine].command == TimelineCommand.Command.ELSE) &&
                            !(layers >= 1 && commands[findEndRepeatLine].command == TimelineCommand.Command.ENDIF);
                            findEndRepeatLine++) {
                            if (commands[findEndRepeatLine].command == TimelineCommand.Command.IF) {
                                layers--;
                            } else if (commands[findEndRepeatLine].command == TimelineCommand.Command.ENDIF) {
                                layers++;
                            }
                        }
                        currentLine = findEndRepeatLine/* + 1*/; //No +1 because that's already in the for loop this is in.
                        continue;
                    }
                case TimelineCommand.Command.ELSE:
                    //If it hits here, the if was true, so go to the endif. Only have to worry about one end possibility so it's easier, and guaranteed ends on an "endif".
                    for (findEndRepeatLine = currentLine + 1; layers != 1; findEndRepeatLine++) {
                        if (commands[findEndRepeatLine].command == TimelineCommand.Command.IF) {
                            layers--;
                        } else if (commands[findEndRepeatLine].command == TimelineCommand.Command.ENDIF) {
                            layers++;
                        }
                    }
                    currentLine = findEndRepeatLine;
                    continue;
                //case TimelineCommand.Command.ENDIF: //Do nothing
                //    continue;
                case TimelineCommand.Command.BULLETPROPERTY:
                    bulletTemplate = new BulletTemplate(GetBulletTemplate(currentCommand.args[0]));
                    switch (currentCommand.bulletProperty) {
                        case TimelineCommand.BulletProperty.SCRIPTROTATION:
                            if (parentBullet != null) { //If the parent is a bullet also add its angle.
                                bulletTemplate.Rotate(parentBullet.bulletTemplate.scriptRotation + ParseValue(currentCommand.args[1]));
                                break;
                            } else {
                                bulletTemplate.Rotate(ParseValue(currentCommand.args[1]));
                                break;
                            }
                        case TimelineCommand.BulletProperty.MOVEMENT:
                            num1 = ParseValue(currentCommand.args[1]);
                            num2 = ParseValue(currentCommand.args[2]);
                            if (parentBullet != null) { //If the parent is a bullet, change its movement to be rotated
                                parentTemplate = parentBullet.bulletTemplate;
                                pos.x = num1 * parentTemplate.scriptRotationMatrix.x + num2 * parentTemplate.scriptRotationMatrix.y;
                                pos.y = num1 * parentTemplate.scriptRotationMatrix.z + num2 * parentTemplate.scriptRotationMatrix.w;
                            } else {
                                pos.x = num1;
                                pos.y = num2;
                            }
                            bulletTemplate.movement = pos;
                            break;
                        case TimelineCommand.BulletProperty.POSITION:
                            num1 = ParseValue(currentCommand.args[1]);
                            num2 = ParseValue(currentCommand.args[2]);
                            if (parentBullet != null) { //If the parent is a bullet, change its position to be rotated 
                                parentTemplate = parentBullet.bulletTemplate;
                                pos.x = num1 * parentTemplate.scriptRotationMatrix.x + num2 * parentTemplate.scriptRotationMatrix.y;
                                pos.y = num1 * parentTemplate.scriptRotationMatrix.z + num2 * parentTemplate.scriptRotationMatrix.w;
                            } else {
                                pos.x = num1;
                                pos.y = num2;
                            }
                            bulletTemplate.position = pos;
                            break;
                        case TimelineCommand.BulletProperty.RELATIVEPOS:
                            bulletTemplate.positionIsRelative = ParseValue(currentCommand.args[1]) > 0 ? true : false;
                            break;
                        case TimelineCommand.BulletProperty.ENEMYSHOT:
                            bulletTemplate.enemyShot = ParseValue(currentCommand.args[1]) > 0 ? true : false;
                            break;
                        case TimelineCommand.BulletProperty.SCALE:
                            bulletTemplate.scale = ParseValue(currentCommand.args[1]);
                            break;
                        case TimelineCommand.BulletProperty.ID:
                            bulletTemplate.bulletID = (byte)Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            break;
                        case TimelineCommand.BulletProperty.INNERCOLOR:
                            bulletTemplate.innerColor.r = ParseValue(currentCommand.args[1]);
                            bulletTemplate.innerColor.g = ParseValue(currentCommand.args[2]);
                            bulletTemplate.innerColor.b = ParseValue(currentCommand.args[3]);
                            bulletTemplate.innerColor.a = ParseValue(currentCommand.args[4]);
                            break;
                        case TimelineCommand.BulletProperty.OUTERCOLOR:
                            bulletTemplate.outerColor.r = ParseValue(currentCommand.args[1]);
                            bulletTemplate.outerColor.g = ParseValue(currentCommand.args[2]);
                            bulletTemplate.outerColor.b = ParseValue(currentCommand.args[3]);
                            bulletTemplate.outerColor.a = ParseValue(currentCommand.args[4]);
                            break;
                        case TimelineCommand.BulletProperty.ROTATION:
                            if (parentBullet != null) { //If the parent is a bullet, change its rotation to be rotated if the script as a whole should be rotated
                                parentTemplate = parentBullet.bulletTemplate;
                                //Debug.Log(Mathf.Acos(parentTemplate.scriptRotationMatrix.x));
                                bulletTemplate.rotation = ParseValue(currentCommand.args[1]) + Mathf.Acos(parentTemplate.scriptRotationMatrix.x);
                            } else {
                                bulletTemplate.rotation = ParseValue(currentCommand.args[1]);
                            }
                            break;
                        case TimelineCommand.BulletProperty.ADVANCEDPATH:
                            bulletTemplate.advancedAttackPath = currentCommand.args[1];
                            break;
                        case TimelineCommand.BulletProperty.CLEARIMMUNE:
                            bulletTemplate.clearImmune = ParseValue(currentCommand.args[1]) > 0 ? true : false;
                            break;
                        case TimelineCommand.BulletProperty.HARMLESS:
                            bulletTemplate.harmless = ParseValue(currentCommand.args[1]) > 0 ? true : false;
                            break;
                    }
                    SetBulletTemplate(currentCommand.args[0], bulletTemplate);
                    continue;
                case TimelineCommand.Command.ENEMYPROPERTY:
                    enemyTemplate = new EnemyTemplate(GetEnemyTemplate(currentCommand.args[0]));
                    switch (currentCommand.enemyProperty) {
                        case TimelineCommand.EnemyProperty.SCALE:
                            enemyTemplate.scale = ParseValue(currentCommand.args[1]);
                            break;
                        case TimelineCommand.EnemyProperty.ATTACKPATH: //Sets one or more attackpaths of this enemy. Clears previous attackpaths.
                            enemyTemplate.attackPath.Clear();
                            enemyTemplate.spellTimers.Clear();
                            for (int i = 1; i < currentCommand.args.Count; i++) {
                                enemyTemplate.attackPath.Add(currentCommand.args[i]);
                                //Read the files to get the time the attacks should last, and if it doesn't exist, let it be the default 9999. "Enemy" handles the rest, like survival cards etc.
                                enemyCommands = TimelineCommand.GetCommands(currentCommand.args[i]);
                                count = 9999;
                                foreach (TimelineCommand c in enemyCommands) {
                                    if (c.command == TimelineCommand.Command.ATTACKDURATION) {
                                        count = Mathf.RoundToInt(ParseValue(c.args[0]));
                                        break;
                                    }
                                }
                                enemyTemplate.spellTimers.Add(count);
                            }
                            break;
                        case TimelineCommand.EnemyProperty.ID: //Sets the ID of the IMAGE of the enemy, as defined in Resources/Graphics/Enemies.
                            enemyTemplate.enemyID = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            break;
                        case TimelineCommand.EnemyProperty.BOSS: //Requires special UI stuff.
                            enemyTemplate.isBoss = ParseValue(currentCommand.args[1]) > 0 ? true : false;
                            break;
                        case TimelineCommand.EnemyProperty.BOSSPORTRAIT: //Enum name of the boss, used with the caster's portrait
                            enemyTemplate.character = (DialogueEntry.character)Enum.Parse(typeof(DialogueEntry.character), currentCommand.args[1].ToUpperInvariant());
                            break;
                        case TimelineCommand.EnemyProperty.MAXHEALTH:
                            enemyTemplate.maxHealth = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            break;
                        case TimelineCommand.EnemyProperty.DROPVALUE:
                            enemyTemplate.dropValueCount = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            break;
                        case TimelineCommand.EnemyProperty.DROPPOWER:
                            //Sets different values depending on args[2] for the different types of power items.
                            int power = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            if (power >= 400) {
                                enemyTemplate.dropPowerFullCount = 1;
                                break;
                            }
                            enemyTemplate.dropPowerCount = (power % 100) / 5;
                            enemyTemplate.dropPowerLargeCount = power / 100;
                            break;
                        case TimelineCommand.EnemyProperty.DROPSCORE:
                            enemyTemplate.dropScoreCount = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            break;
                        case TimelineCommand.EnemyProperty.STARTPOS:
                            enemyTemplate.startpostion = new Vector2(ParseValue(currentCommand.args[1]), ParseValue(currentCommand.args[2]));
                            break;
                        case TimelineCommand.EnemyProperty.BASESCORE:
                            enemyTemplate.baseScore = (uint)Mathf.RoundToInt(ParseValue(currentCommand.args[0]));
                            continue;
                        default:
                            break;
                    }
                    SetEnemyTemplate(currentCommand.args[0], enemyTemplate);
                    continue;
                case TimelineCommand.Command.LASERPROPERTY:
                    LaserTemplate laserTemplate = new LaserTemplate(GetLaserTemplate(currentCommand.args[0]));
                    switch (currentCommand.laserProperty) {
                        case TimelineCommand.LaserProperty.WARNDURATION:
                            laserTemplate.warnDuration = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            break;
                        case TimelineCommand.LaserProperty.SHOTDURATION:
                            laserTemplate.shotDuration = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            break;
                        case TimelineCommand.LaserProperty.INNERCOLOR:
                            laserTemplate.innerColor.r = ParseValue(currentCommand.args[1]);
                            laserTemplate.innerColor.g = ParseValue(currentCommand.args[2]);
                            laserTemplate.innerColor.b = ParseValue(currentCommand.args[3]);
                            laserTemplate.innerColor.a = ParseValue(currentCommand.args[4]);
                            break;
                        case TimelineCommand.LaserProperty.OUTERCOLOR:
                            laserTemplate.outerColor.r = ParseValue(currentCommand.args[1]);
                            laserTemplate.outerColor.g = ParseValue(currentCommand.args[2]);
                            laserTemplate.outerColor.b = ParseValue(currentCommand.args[3]);
                            laserTemplate.outerColor.a = ParseValue(currentCommand.args[4]);
                            break;
                        case TimelineCommand.LaserProperty.WIDTH:
                            laserTemplate.width = ParseValue(currentCommand.args[1]);
                            break;
                        case TimelineCommand.LaserProperty.MOVEMENT:
                            laserTemplate.movement = new Vector2(ParseValue(currentCommand.args[1]), ParseValue(currentCommand.args[2]));
                            break;
                        case TimelineCommand.LaserProperty.ROTATION:
                            if (parentBullet != null) { //If the parent is a bullet, change its rotation to be rotated if the script as a whole should be rotated
                                parentTemplate = parentBullet.bulletTemplate;
                                //Debug.Log(Mathf.Acos(parentTemplate.scriptRotationMatrix.x));
                                laserTemplate.rotation = ParseValue(currentCommand.args[1]) * -1 /*-1 to fit with angletoplayer()*/ + Mathf.Acos(parentTemplate.scriptRotationMatrix.x);
                            } else {
                                laserTemplate.rotation = ParseValue(currentCommand.args[1]) * -1;
                            }
                            break;
                        case TimelineCommand.LaserProperty.ROTATIONSPEED:
                            laserTemplate.rotationSpeed = ParseValue(currentCommand.args[1]);
                            break;
                        case TimelineCommand.LaserProperty.POSITION:
                            num1 = ParseValue(currentCommand.args[1]);
                            num2 = ParseValue(currentCommand.args[2]);
                            if (parentBullet != null) { //If the parent is a bullet, change its position to be rotated 
                                parentTemplate = parentBullet.bulletTemplate;
                                pos.x = num1 * parentTemplate.scriptRotationMatrix.x + num2 * parentTemplate.scriptRotationMatrix.y;
                                pos.y = num1 * parentTemplate.scriptRotationMatrix.z + num2 * parentTemplate.scriptRotationMatrix.w;
                            } else {
                                pos.x = num1;
                                pos.y = num2;
                            }
                            laserTemplate.position = pos;
                            break;
                        case TimelineCommand.LaserProperty.RELATIVEPOS:
                            laserTemplate.positionIsRelative = ParseValue(currentCommand.args[1]) > 0 ? true : false;
                            break;
                    }
                    SetLaserTemplate(currentCommand.args[0], laserTemplate);
                    continue;
                case TimelineCommand.Command.CREATEBULLET:
                    if (parentBullet != null) { //If a bulet is firing this, pass the script rotation data onto the new bullet.
                        bulletTemplate.Rotate(parentBullet.bulletTemplate.scriptRotation + bulletTemplate.rotation);
                    }
                    GlobalHelper.CreateBullet(GetBulletTemplate(currentCommand.args[0]), transform.position);
                    continue;
                case TimelineCommand.Command.CREATEENEMY:
                    GlobalHelper.CreateEnemy(GetEnemyTemplate(currentCommand.args[0]));
                    continue;
                case TimelineCommand.Command.CREATELASER:
                    GlobalHelper.CreateLaser(GetLaserTemplate(currentCommand.args[0]), transform.position);
                    continue;
                case TimelineCommand.Command.MOVEPARENT:
                    if (parentBullet != null) { //If this is a bullet, posx,y(,z) should be modified, not its direct position
                        Bullet bullet = parentBullet;
                        num1 = ParseValue(currentCommand.args[0]);
                        num2 = ParseValue(currentCommand.args[1]);
                        pos.x = num1 * bullet.bulletTemplate.scriptRotationMatrix.x + num2 * bullet.bulletTemplate.scriptRotationMatrix.y;
                        pos.y = num1 * bullet.bulletTemplate.scriptRotationMatrix.z + num2 * bullet.bulletTemplate.scriptRotationMatrix.w;
                        bullet.posx += pos.x;
                        bullet.posy += pos.y;
                    } else {
                        transform.position += new Vector3(ParseValue(currentCommand.args[0]), ParseValue(currentCommand.args[1]), 0f);
                    }
                    continue;
                case TimelineCommand.Command.MOVETOWARDSPOINT:
                    StopCoroutine("moveTowardsSmooth");
                    StartCoroutine(moveTowardsSmooth(new Vector3(ParseValue(currentCommand.args[0]), ParseValue(currentCommand.args[1]), transform.position.z), ParseValue(currentCommand.args[2])));
                    //TODO: Test
                    continue;
                case TimelineCommand.Command.DESTROYPARENT: //Destroys whatever this is attached to.
                    if (parentBullet != null) { //Destroying bullets is wasteful, they should be added to the dead bullet pile.
                        parentBullet.Deactivate();
                    } else {
                        Destroy(transform.gameObject);
                    }
                    continue;
                case TimelineCommand.Command.WAIT:
                    cooldown = Mathf.RoundToInt(ParseValue(currentCommand.args[0]));
                    return;
                case TimelineCommand.Command.SETPARENTHEALTH:
                    parentEnemy.health = Mathf.RoundToInt(ParseValue(currentCommand.args[0]));
                    parentEnemy.template.maxHealth = parentEnemy.health;
                    parentEnemy.UpdateHealthbar();
                    continue;
                case TimelineCommand.Command.ANGLETOPLAYER: //Returns the angle to the player.
                    pos = transform.position;
                    playerpos = GameObject.FindWithTag("Player").transform.position;
                    num1 = pos.x - playerpos.x;
                    num2 = pos.y - playerpos.y;
                    SetNumber(currentCommand.args[0], Mathf.Atan2(-num1, -num2));
                    continue;
                case TimelineCommand.Command.ANGLETOPOINT:
                    pos = transform.position;
                    Vector2 pos2 = new Vector2(ParseValue(currentCommand.args[1]), ParseValue(currentCommand.args[2]));
                    num1 = pos.x - pos2.x;
                    num2 = pos.y - pos2.y;
                    SetNumber(currentCommand.args[0], Mathf.Atan2(-num1, -num2));
                    continue;
                case TimelineCommand.Command.GETPLAYERPOSITION:
                    playerpos = GameObject.FindWithTag("Player").transform.position;
                    SetNumber(currentCommand.args[0], playerpos.x);
                    SetNumber(currentCommand.args[1], playerpos.y);
                    continue;
                case TimelineCommand.Command.GETPOSITION:
                    pos = transform.position;
                    SetNumber(currentCommand.args[0], pos.x);
                    SetNumber(currentCommand.args[1], pos.y);
                    continue;
                case TimelineCommand.Command.RANDOM: //Returns a random value between args[1] and args[2]. Uses the GlobalHelper.random because everything random should do that because of replay support.
                    num1 = ParseValue(currentCommand.args[1]);
                    num2 = ParseValue(currentCommand.args[2]);
                    SetNumber(currentCommand.args[0], ((float)GlobalHelper.random.NextDouble()) * (num2 - num1) + num1);
                    continue;
                case TimelineCommand.Command.SET: //What follows are a bunch of selfexplanatory math functions: set, addition, subtraction, multiplication, division, modulo, power, trig functions, absolute
                    SetNumber(currentCommand.args[0], ParseValue(currentCommand.args[1]));
                    continue;
                case TimelineCommand.Command.ADD:
                    SetNumber(currentCommand.args[0], ParseValue(currentCommand.args[1]) + ParseValue(currentCommand.args[2]));
                    continue;
                case TimelineCommand.Command.SUB:
                    SetNumber(currentCommand.args[0], ParseValue(currentCommand.args[1]) - ParseValue(currentCommand.args[2]));
                    continue;
                case TimelineCommand.Command.MUL:
                    SetNumber(currentCommand.args[0], ParseValue(currentCommand.args[1]) * ParseValue(currentCommand.args[2]));
                    continue;
                case TimelineCommand.Command.DIV:
                    SetNumber(currentCommand.args[0], ParseValue(currentCommand.args[1]) / ParseValue(currentCommand.args[2]));
                    continue;
                case TimelineCommand.Command.MOD:
                    SetNumber(currentCommand.args[0], ParseValue(currentCommand.args[1]) % ParseValue(currentCommand.args[2]));
                    continue;
                case TimelineCommand.Command.POW:
                    SetNumber(currentCommand.args[0], Mathf.Pow(ParseValue(currentCommand.args[1]), ParseValue(currentCommand.args[2])));
                    continue;
                case TimelineCommand.Command.SIN:
                    SetNumber(currentCommand.args[0], Mathf.Sin(ParseValue(currentCommand.args[1])));
                    continue;
                case TimelineCommand.Command.ASIN:
                    SetNumber(currentCommand.args[0], Mathf.Asin(ParseValue(currentCommand.args[1])));
                    continue;
                case TimelineCommand.Command.COS:
                    SetNumber(currentCommand.args[0], Mathf.Cos(ParseValue(currentCommand.args[1])));
                    continue;
                case TimelineCommand.Command.ACOS:
                    SetNumber(currentCommand.args[0], Mathf.Acos(ParseValue(currentCommand.args[1])));
                    continue;
                case TimelineCommand.Command.TAN:
                    SetNumber(currentCommand.args[0], Mathf.Tan(ParseValue(currentCommand.args[1])));
                    continue;
                case TimelineCommand.Command.ATAN:
                    SetNumber(currentCommand.args[0], Mathf.Atan(ParseValue(currentCommand.args[1])));
                    continue;
                case TimelineCommand.Command.ABS:
                    SetNumber(currentCommand.args[0], Mathf.Abs(ParseValue(currentCommand.args[1])));
                    continue;
                default:
                    continue;
            }
        }

        return;
    }

    /// <summary>
    /// Sets the number in the numberVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    /// <param name="name">The name of the var</param>
    /// <param name="value">The value of the var</param>
    private void SetNumber(string name, float value) {
        if (name[0] == 95) { //Starts with '_'
            stringHash = name.GetHashCode();
            if (globalNumberVars.ContainsKey(stringHash)) {
                globalNumberVars[stringHash] = value;
            } else {
                globalNumberVars.Add(stringHash, value);
            }
        } else {
            stringHash = name.GetHashCode();
            if (numberVars.ContainsKey(stringHash)) {
                numberVars[stringHash] = value;
            } else {
                numberVars.Add(stringHash, value);
            }
        }
    }

    /// <summary>
    /// Gets the number in the numberVars dictionary by name. If it doesn't exist, it creates it, sets it to zero, and returns zero.
    /// </summary>
    /// <param name="name">The var name to retrieve</param>
    /// <returns></returns>
    private float GetNumber(string name) { 
        stringHash = name.GetHashCode();
        float returnFloat;
        if (name[0] == 95) { //Starts with '_'
            if (!globalNumberVars.TryGetValue(stringHash, out returnFloat)) {
                globalNumberVars.Add(stringHash, 0f);
                return 0f;
            }
        } else {
            if (!numberVars.TryGetValue(stringHash, out returnFloat)) {
                numberVars.Add(stringHash, 0f);
                return 0f;
            }
        }
        return returnFloat;
    }

    /// <summary>
    /// Gets the BulletTemplate in the numberVars dictionary by name. If it doesn't exist, it creates it, and returns it.
    /// </summary>
    /// <param name="name">The var name to retrieve</param>
    /// <returns></returns>
    private BulletTemplate GetBulletTemplate(string name) {
        stringHash = name.GetHashCode();
        if (name[0] == 95) { //Starts with '_'
            if (!globalBulletTemplateVars.ContainsKey(stringHash)) {
                globalBulletTemplateVars.Add(stringHash, new BulletTemplate());
                return new BulletTemplate();
            } else {
                return globalBulletTemplateVars[stringHash];
            }
        } else {
            if (!bulletTemplateVars.ContainsKey(stringHash)) {
                bulletTemplateVars.Add(stringHash, new BulletTemplate());
                return new BulletTemplate();
            } else {
                return bulletTemplateVars[stringHash];
            }
        }
    }

    /// <summary>
    /// Sets the BulletTemplate in the bulletTemplateVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    /// <param name="name">The name of the var</param>
    /// <param name="value">The value of the var</param>
    private void SetBulletTemplate(string name, BulletTemplate value) {
        stringHash = name.GetHashCode();
        if (name[0] == 95) { //Starts with '_'
            if (globalBulletTemplateVars.ContainsKey(stringHash)) {
                globalBulletTemplateVars[stringHash] = value;
            } else {
                globalBulletTemplateVars.Add(stringHash, value);
            }
        } else {
            if (bulletTemplateVars.ContainsKey(stringHash)) {
                bulletTemplateVars[stringHash] = value;
            } else {
                bulletTemplateVars.Add(stringHash, value);
            }
        }
    }

    /// <summary>
    /// Gets the EnemyTemplate in the numberVars dictionary by name. If it doesn't exist, it creates it, and returns it.
    /// </summary>
    /// <param name="name">The var name to retrieve</param>
    /// <returns></returns>
    private EnemyTemplate GetEnemyTemplate(string name) {
        stringHash = name.GetHashCode();
        if (name[0] == 95) { //Starts with '_'
            if (!globalEnemyTemplateVars.ContainsKey(stringHash)) {
                globalEnemyTemplateVars.Add(stringHash, new EnemyTemplate());
                return new EnemyTemplate();
            } else {
                return globalEnemyTemplateVars[stringHash];
            }
        } else {
            if (!enemyTemplateVars.ContainsKey(stringHash)) {
                enemyTemplateVars.Add(stringHash, new EnemyTemplate());
                return new EnemyTemplate();
            } else {
                return enemyTemplateVars[stringHash];
            }
        }
    }

    /// <summary>
    /// Sets the EnemyTemplate in the enemyTemplateVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    /// <param name="name">The name of the var</param>
    /// <param name="value">The value of the var</param>
    private void SetEnemyTemplate(string name, EnemyTemplate value) {
        stringHash = name.GetHashCode();
        if (name[0] == 95) { //Starts with '_'
            if (globalEnemyTemplateVars.ContainsKey(stringHash)) {
                globalEnemyTemplateVars[stringHash] = value;
            } else {
                globalEnemyTemplateVars.Add(stringHash, value);
            }
        } else {
            if (enemyTemplateVars.ContainsKey(stringHash)) {
                enemyTemplateVars[stringHash] = value;
            } else {
                enemyTemplateVars.Add(stringHash, value);
            }
        }
    }

    private LaserTemplate GetLaserTemplate(string name) {
        stringHash = name.GetHashCode();
        if (name[0] == 95) { //Starts with '_'
            if (!globalLaserTemplateVars.ContainsKey(stringHash)) {
                globalLaserTemplateVars.Add(stringHash, new LaserTemplate());
                return new LaserTemplate();
            } else {
                return globalLaserTemplateVars[stringHash];
            }
        } else {
            if (!laserTemplateVars.ContainsKey(stringHash)) {
                laserTemplateVars.Add(stringHash, new LaserTemplate());
                return new LaserTemplate();
            } else {
                return laserTemplateVars[stringHash];
            }
        }
    }

    private void SetLaserTemplate(string name, LaserTemplate value) {
        stringHash = name.GetHashCode();
        if (name[0] == 95) { //Starts with '_'
        } else {
            if (laserTemplateVars.ContainsKey(stringHash)) {
                laserTemplateVars[stringHash] = value;
            } else {
                laserTemplateVars.Add(stringHash, value);
            }
        }
    }

    /// <summary>
    /// A faster method than 26 String.Contains(char)'s for my needs.
    /// </summary>
    /// <param name="toEvaluate">The string to check if it has letters.</param>
    /// <returns>True if it contains any of a-z or A-Z.</returns>
    private static bool ContainsLetters(string toEvaluate) {
        foreach (char c in toEvaluate) {
            if (c >= 65 && c <= 122) { //All letters in UTF-16 from A to Z to a to z
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns whatever is between the first open brace and second to last character, seperated by comma's.
    /// </summary>
    /// <param name="toEvaluate">The string to evaluate.</param>
    /// <returns>Everything that was originally between braces in <function>(args[0],args[1] ... )</returns>
    public static List<string> GetArguments(string toEvaluate) {
        //Assuming only one set of braces
        int i = 0;
        while (toEvaluate[i] != '(') {
            i++;
            if (i >= toEvaluate.Length) {
                return new List<string>();
            }
        }
        string returnString = toEvaluate.Substring(i + 1, toEvaluate.Length - i - 2);
        List<string> returnList = new List<string>();
        foreach (string str in returnString.Split(',')) {
            returnList.Add(str);
        }
        return returnList;
    }

    /// <summary>
    /// Returns whatever is before the first open brace for each entry in the toEvaluate array.
    /// </summary>
    /// <param name="toEvaluate">The string to find the function of.</param>
    /// <returns>Returns whatever is before the first open brace.</returns>
    public static string GetFunction(string toEvaluate) { 
        string returnString = toEvaluate.Split('(')[0];
        return returnString;
    }
    
    /// <summary>
    /// Parses value - whether it's a number or numberVars var name.
    /// </summary>
    /// <param name="value">The thing to parse.</param>
    /// <returns></returns>
    private float ParseValue(string value) {
        if (ContainsLetters(value)) {
            return GetNumber(value);
        } else {
            return ParseFloat(value);
        }
    }

    private static float ParseFloat(string value) {
        bool negative = false;
        if (value[0] == '-') { //Remember the minus sign and remove it from the string.
            negative = true;
            value = value.Substring(1);
        }
        float returnValue = 0f;
        int stringLength = value.Length;
        int dotPosition = stringLength; //Defaults to a dot after the entire string.
        //Find the dot position if it's there
        int i;
        for (i = 0; i < stringLength; i++) {
            if (value[i] == '.') {
                dotPosition = i;
                break;
            }
        }
        //Evaluate before the dot. UTF-16 '0' = 48, '9' = 57, so '0' - 48 = 0, and '9' - 48 = 9.
        for (i = 0; i < dotPosition; i++) {
            returnValue += tenPower(value[i] - 48, dotPosition - i - 1);
        }
        //Evaluate behind the dot.
        for (i = dotPosition + 1; i < stringLength; i++) {
            returnValue += tenPower(value[i] - 48, dotPosition - i);
        }
        if (negative) {
            returnValue = 0 - returnValue;
        }
        return returnValue;
    }

    /// <summary>
    /// Returns f * 10^power.
    /// </summary>
    private static float tenPower(float f, int power) {
        if (power > 0) {
            for (int i = 0; i < power; i++) {
                f *= 10f;
            }
            return f;
        } else {
            for (int i = 0; i < -power; i++) {
                f /= 10f;
            }
            return f;
        }
    }

    /// <summary>
    /// Moves from the current position to endPos, taking "time" ticks. (It's a float, but has the same 60-ticks-is-a-second rule.)
    /// </summary>
    private IEnumerator moveTowardsSmooth(Vector3 endPos, float time) {
        time = Mathf.Round(time);
        Vector3 startPos = transform.position;
        float linearProgress = 0f;
        float actualProgress = 0f;
        //The difference every tick is determined by 3/(2*time) * 4x-4x^2, because for all a sum k from 0 to a (3/2a * (4k/a - 4(k/a)^2)) returns almost 1. (WA gives 1 - 1/(a^2), and a is usually at least 2 digits).
        while (linearProgress < 1f) {
            if (!GlobalHelper.paused) {
                actualProgress = 3/(2 * time) * (4 * (linearProgress - linearProgress*linearProgress));
                transform.position += actualProgress * (endPos - startPos);
                linearProgress += 1 / time;
            }
            yield return null;
        }
        transform.position = endPos;
    }
}
