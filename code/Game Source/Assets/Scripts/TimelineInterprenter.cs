﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
/// <summary>
/// A class reading .txt's describing either enemy or bullet info. Important in those .txt's is the wait(x) function, which is in ticks and not second, because second would not allow replays.
/// </summary>
public class TimelineInterprenter : MonoBehaviour {
    public string patternPath = "";
    public bool levelTimeline = false; //Set via inspector
    private int commandsId; //Which of TimelineCommand's CommandLists to use.

    /* The (not-global) vars-arrays store their respective data. In memory,
     * strings of numbers represent their array indices, while (for numbers)
     * strings starting with an "n" represent actual numerical values.
     */
    private float[] numberVars;
    private BulletTemplate[] bulletTemplateVars;
    private EnemyTemplate[] enemyTemplateVars;
    private LaserTemplate[] laserTemplateVars;
    private static Dictionary<int, float> globalNumberVars = new Dictionary<int, float>();
    private static Dictionary<int, BulletTemplate> globalBulletTemplateVars = new Dictionary<int, BulletTemplate>();
    private static Dictionary<int, EnemyTemplate> globalEnemyTemplateVars = new Dictionary<int, EnemyTemplate>();
    private static Dictionary<int, LaserTemplate> globalLaserTemplateVars = new Dictionary<int, LaserTemplate>();
    private List<Phase> phases = new List<Phase>();
    private List<int> repeatStepback = new List<int>(); //What line to go to when encountering an endrepeat.
    private Bullet parentBullet;
    private Enemy parentEnemy;
    private int currentLine = 0;
    private int cooldown = 0;
    private bool bosswaiting = false;

    //Vars needed within the for loop. Static because they don't get used for multiple ticks, so they can be reused, so it's useless to create these for every object needing the interprenter.
    private static int count, layers, findLine, lineDifference;
    private static float num1, num2, num3;
    private static TimelineCommand currentCommand;
    private static BulletTemplate bulletTemplate;
    private static BulletTemplate parentTemplate;
    private static EnemyTemplate enemyTemplate;
    private static Vector3 pos, playerpos;
    private static int stringHash;
    private static bool ifevaluation;
    private static int enemyCommandsId;

    /// <summary>
    /// For initialisation of the var arrays, to be used by TimelineCommand when parsing files into memory.
    /// </summary>
    public void SetVarArrayLengths(int numlength, int bulletlength, int enemylength, int laserlength) {
        numberVars = new float[numlength];
        bulletTemplateVars = new BulletTemplate[bulletlength];
        for (int i = 0; i < bulletTemplateVars.Length; i++) {
            bulletTemplateVars[i] = BulletTemplate.basic;
        }
        enemyTemplateVars = new EnemyTemplate[enemylength];
        for (int i = 0; i < enemyTemplateVars.Length; i++) {
            enemyTemplateVars[i] = new EnemyTemplate();
        }
        laserTemplateVars = new LaserTemplate[laserlength];
        for (int i = 0; i < laserTemplateVars.Length; i++) {
            laserTemplateVars[i] = LaserTemplate.basic;
        }
    }

    public void Reset(string newTimeLine) {
        patternPath = newTimeLine;
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
            patternPath = "Timelines/Stages/Stage" + (GlobalHelper.level + 1 /*The level files are 1-indexed, everything else 0-indexed*/) + "_" + ((int)GlobalHelper.difficulty);
        }
        parentEnemy = transform.GetComponent<Enemy>();
        if (patternPath == "") {
            patternPath = parentEnemy.template.attackPath[0];
        }
        Reset(patternPath);
    }

    void Update() {
        if (!GlobalHelper.paused && !GlobalHelper.dialogue) {
            CheckPhases();
            if (cooldown <= 0 && !bosswaiting) {
                //+1 because it would otherwise start at the line it terminated at (only "wait(x)"), resulting in an infinite loop, which is a nightmare. That is longer than 12 seconds.
                currentLine++;
                ReadAttack();
            }
            cooldown--;
            if (bosswaiting && GlobalHelper.activeBosses == 0) {
                bosswaiting = false;
            }
        }
    }

    private void CheckPhases() {
        foreach(Phase phase in new List<Phase>(phases)) {
            if (phase.CheckTrigger()) {
                currentLine = phase.goalLine;
                //Clear all bullets if they should be killed
                if (phase.clear) {
                    GlobalHelper.levelManager.GetComponent<BulletClear>().Clear(10f, BulletClear.BulletClearType.FULLCLEAR, 30);
                }
                //Reset all repeats because that behaviour could be weird. If's could also be weird but that only gets evaluated at IF/ELSE
                repeatStepback = new List<int>();
                //Destroy the relevant lock if it's there
                if (phase.lockReference != null) {
                    Destroy(phase.lockReference);
                }
                //Remove the phase checked from the "to check" list
                phases.Remove(phase);
                break;
            }
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
        if (patternPath == null) {
            return;
        }
        if (initialise) {
            commandsId = TimelineCommand.GetCommands(patternPath);
            SetVarArrayLengths(TimelineCommand.commandLists[commandsId][0].numberVarCount,
                               TimelineCommand.commandLists[commandsId][0].bulletVarCount,
                               TimelineCommand.commandLists[commandsId][0].enemyVarCount,
                               TimelineCommand.commandLists[commandsId][0].laserVarCount);
        }
        ReadAttack();
    }

     /// <summary>
     /// Reads the text (defined in patternPath) line by line and does stuff depending on the info.
     /// The syntax of those lines is usually <functionname>([argument[,other arguments .. ]]);
     /// Any text evaluated should be all-lowercase.
     /// </summary>
    public void ReadAttack() { 
        if (patternPath == null) {
            return;
        }
        for (; currentLine < TimelineCommand.commandLists[commandsId].Count; currentLine++) {
            currentCommand = TimelineCommand.commandLists[commandsId][currentLine];
            //Take the part before the brackets and try to figure out what it says and do something with it.
            switch (currentCommand.command) {
                case TimelineCommand.Command.STARTTIMELINE:
                    //Attaches ANOTHER TimelineInterprenter to this GameObject with path args[0].
                    TimelineInterprenter newpattern = transform.gameObject.AddComponent<TimelineInterprenter>();
                    newpattern.patternPath = currentCommand.args[0];
                    continue;
                case TimelineCommand.Command.STOP:
                    currentLine = TimelineCommand.commandLists[commandsId].Count;
                    break;
                case TimelineCommand.Command.LOADLEVEL:
                    SceneSwitcher.ContinueToLevel(Mathf.RoundToInt(ParseValue(currentCommand.args[0])));
                    break;
                case TimelineCommand.Command.DIALOGUE:
                    GlobalHelper.levelManager.GetComponent<DialogueManager>().StartDialogue(currentCommand.args[0] + "_" + GlobalHelper.GetCharacterType(GlobalHelper.character));
                    cooldown = 1;
                    return; //The next loop should not happen immediatly, but after the dialogue has been processed. That's checked within Update().
                case TimelineCommand.Command.STARTMUSIC:
                    GlobalHelper.audioManager.PlayMusic((AudioManager.BGM)Enum.Parse(typeof(AudioManager.BGM), currentCommand.args[0].ToUpperInvariant()));
                    continue;
                case TimelineCommand.Command.PLAYSOUND:
                    AudioManager.QueueSound((AudioManager.SFX)Enum.Parse(typeof(AudioManager.SFX), currentCommand.args[0].ToUpperInvariant()));
                    continue;
                case TimelineCommand.Command.BOSSNAME:
                    GlobalHelper.bossUI.transform.Find("Name").GetComponent<Text>().text = currentCommand.args[0];
                    continue;
                //case TimelineCommand.Command.PHASE:
                    //Do nothing, this is for being processed by TOPHASE
                    //continue;
                case TimelineCommand.Command.TOPHASE:
                    string tag = currentCommand.args[0];
                    for (findLine = currentLine + 1; findLine < TimelineCommand.commandLists[commandsId].Count; findLine++) {
                        if (TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.PHASE &&
                            TimelineCommand.commandLists[commandsId][findLine].args[0] == tag) {
                            break;
                        }
                    }
                    int health = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));

                    //Add a "phase lock" to the enemy healthbar if it's there
                    GameObject createdObject = new GameObject();
                    if (parentEnemy.transform.Find("Healthbar") != null) { //There's a healthbar
                        createdObject = Instantiate((GameObject)Resources.Load("Prefabs/PhaseLock"));
                        //Set the object to be the child of the healthbar
                        createdObject.transform.SetParent(parentEnemy.transform.Find("Healthbar").GetChild(0));
                        float angle = -(Mathf.PI * (health * 2f) / parentEnemy.template.maxHealth);
                        createdObject.transform.position = new Vector3(parentEnemy.transform.position.x + 1.2f * Mathf.Sin(angle),parentEnemy.transform.position.y + 1.2f * Mathf.Cos(angle),parentEnemy.transform.position.z - 0.1f);
                        createdObject.transform.Rotate(new Vector3(0, 0, -angle * Mathf.Rad2Deg));
                    }
                    //Add the new phase line to keep track off.
                    phases.Add(new Phase(findLine, health, Mathf.RoundToInt(ParseValue(currentCommand.args[2])),
                        ParseValue(currentCommand.args[3]) != 0, parentEnemy, createdObject));
                    continue;
                case TimelineCommand.Command.REPEAT:
                    //Executes everything between here and the matching endrepeat args[0] times.
                    count = Mathf.RoundToInt(ParseValue(currentCommand.args[0])) - 1;
                    layers = 0; //Goes down for every Repeat(x) line. Goes up for every Endrepeat line. When it reaches "1", it hit the right endrepeat.
                    for (findLine = currentLine + 1; layers != 1; findLine++) {
                        if (TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.REPEAT) {
                            layers--;
                        } else if (TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.ENDREPEAT) {
                            layers++;
                        }
                    }
                    lineDifference = 1 + currentLine - findLine; //This should be how much to go back after hitting "endrepeat". Add 1 because it's 1 off and would infinite-loop.
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
                        layers = 0;
                        findLine = currentLine + 1;
                        //"layers" goes down for every "if" line, goes up every "endif" line, "else" does not affect it. When it reaches "1" on endif or 0 on else it's done
                        while (true) {
                            if (TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.IF) {
                                layers--;
                            } else if (TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.ENDIF) {
                                layers++;
                            }
                            if ((layers >= 0 && TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.ELSE) ||
                            (layers >= 1 && TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.ENDIF)) {
                                break;
                            }
                            findLine++;
                        }
                        currentLine = findLine; //No +1 because that's already in the for loop this is in.
                        continue;
                    }
                case TimelineCommand.Command.ELSE:
                    //If it hits here, the if was true, so go to the endif. Only have to worry about one end possibility so it's easier, and guaranteed ends on an "endif".
                    for (findLine = currentLine + 1; layers != 1; findLine++) {
                        if (TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.IF) {
                            layers--;
                        } else if (TimelineCommand.commandLists[commandsId][findLine].command == TimelineCommand.Command.ENDIF) {
                            layers++;
                        }
                    }
                    currentLine = findLine;
                    continue;
                //case TimelineCommand.Command.ENDIF: //Do nothing
                //    continue;
                case TimelineCommand.Command.BULLETPROPERTY:
                    bulletTemplate = GetBulletTemplate(currentCommand.args[0]);
                    switch (currentCommand.bulletProperty) {
                        case TimelineCommand.BulletProperty.SCRIPTROTATION:
                            if (parentBullet != null) { //If the parent is a bullet also add its angle.
                                if (currentCommand.args.Count == 3) { //There shouldn't be scriptrotation if an extra argument exists
                                    bulletTemplate.Rotate(ParseValue(currentCommand.args[1]));
                                    break;
                                }
                                bulletTemplate.Rotate(parentBullet.bulletTemplate.scriptRotation + ParseValue(currentCommand.args[1]));
                                break;
                            } else {
                                bulletTemplate.Rotate(ParseValue(currentCommand.args[1]));
                                break;
                            }
                        case TimelineCommand.BulletProperty.MOVEMENT:
                            num1 = ParseValue(currentCommand.args[1]);
                            num2 = ParseValue(currentCommand.args[2]);
                            if (parentBullet != null &&           //If the parent is a bullet, change its movement to be rotated
                                currentCommand.args.Count != 4) { //It however shouldn't have script rotation if an extra argument exists.
                                parentTemplate = parentBullet.bulletTemplate;
                                pos.x = num1 * parentTemplate.scriptRotationMatrix.x + num2 * parentTemplate.scriptRotationMatrix.y;
                                pos.y = num1 * parentTemplate.scriptRotationMatrix.z + num2 * parentTemplate.scriptRotationMatrix.w;
                            } else {
                                pos.x = num1;
                                pos.y = num2;
                            }
                            bulletTemplate.movement = pos;
                            break;
                        case TimelineCommand.BulletProperty.MOVEMENTPOLAR:
                            num3 = ParseValue(currentCommand.args[1]); //angle
                            num2 = ParseValue(currentCommand.args[2]); //length
                            num1 = Mathf.Sin(num3) * num2; //x
                            num2 = Mathf.Cos(num3) * num2; //y
                            if (parentBullet != null &&           //If the parent is a bullet, change its movement to be rotated
                                currentCommand.args.Count != 4) { //It however shouldn't have script rotation if an extra argument exists.
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
                            if (parentBullet != null &&              //If the parent is a bullet, change its position to be rotated 
                                !(currentCommand.args.Count == 4)) { //It however shouldn't have script rotation if an extra argument exists.
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
                            if (parentBullet != null &&              //If the parent is a bullet, change its rotation to be rotated if the script as a whole should be rotated
                                !(currentCommand.args.Count == 3)) { //It however shouldn't have script rotation if an extra argument exists.
                                parentTemplate = parentBullet.bulletTemplate;
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
                        case TimelineCommand.BulletProperty.SNAKELENGTH:
                            bulletTemplate.snakeLength = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
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
                                enemyCommandsId = TimelineCommand.GetCommands(currentCommand.args[i]);
                                count = 9999;
                                foreach (TimelineCommand c in TimelineCommand.commandLists[enemyCommandsId]) {
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
                            enemyTemplate.character = (DialogueEntry.Character)Enum.Parse(typeof(DialogueEntry.Character), currentCommand.args[1].ToUpperInvariant());
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
                            enemyTemplate.baseScore = (uint)Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            continue;
                        default:
                            break;
                    }
                    SetEnemyTemplate(currentCommand.args[0], enemyTemplate);
                    continue;
                case TimelineCommand.Command.LASERPROPERTY:
                    LaserTemplate laserTemplate = GetLaserTemplate(currentCommand.args[0]);
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
                            if (parentBullet != null &&              //If the parent is a bullet, change its position to be rotated 
                                !(currentCommand.args.Count == 4)) { //It however shouldn't have script rotation if an extra argument exists.
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
                    bulletTemplate = GetBulletTemplate(currentCommand.args[0]);
                    if (bulletTemplate.snakeLength > 0) {
                        ThingCreator.CreateSnake(bulletTemplate.snakeLength, bulletTemplate, transform.position);
                    } else {
                        ThingCreator.CreateBullet(bulletTemplate, transform.position);
                    }
                    continue;
                case TimelineCommand.Command.CREATEENEMY:
                    ThingCreator.CreateEnemy(GetEnemyTemplate(currentCommand.args[0]));
                    continue;
                case TimelineCommand.Command.CREATELASER:
                    ThingCreator.CreateLaser(GetLaserTemplate(currentCommand.args[0]), transform.position);
                    continue;
                case TimelineCommand.Command.MOVEPARENT:
                    if (parentBullet != null) {  //If this is a bullet, posx,y(,z) should be modified, not its direct position
                        Bullet bullet = parentBullet;
                        num1 = ParseValue(currentCommand.args[0]);
                        num2 = ParseValue(currentCommand.args[1]);
                        if (!(currentCommand.args.Count == 3)) {
                            pos.x = num1 * bullet.bulletTemplate.scriptRotationMatrix.x + num2 * bullet.bulletTemplate.scriptRotationMatrix.y;
                            pos.y = num1 * bullet.bulletTemplate.scriptRotationMatrix.z + num2 * bullet.bulletTemplate.scriptRotationMatrix.w;
                        } else { //It however shouldn't have script rotation if an extra argument exists.
                            pos.x = num1;
                            pos.y = num2;
                        }
                        bullet.pos.x += pos.x;
                        bullet.pos.y += pos.y;
                    } else {
                        transform.position += new Vector3(ParseValue(currentCommand.args[0]), ParseValue(currentCommand.args[1]), 0f);
                    }
                    continue;
                case TimelineCommand.Command.MOVEPARENTPOLAR:
                    num3 = ParseValue(currentCommand.args[0]); //angle
                    num2 = ParseValue(currentCommand.args[1]); //length
                    num1 = Mathf.Sin(num3) * num2; //x
                    num2 = Mathf.Cos(num3) * num2; //y
                    if (parentBullet != null) { //If this is a bullet, posx,y(,z) should be modified, not its direct position
                        Bullet bullet = parentBullet;
                        if (!(currentCommand.args.Count == 3)) {
                            pos.x = num1 * bullet.bulletTemplate.scriptRotationMatrix.x + num2 * bullet.bulletTemplate.scriptRotationMatrix.y;
                            pos.y = num1 * bullet.bulletTemplate.scriptRotationMatrix.z + num2 * bullet.bulletTemplate.scriptRotationMatrix.w;
                        } else { //It however shouldn't have script rotation if an extra argument exists.
                            pos.x = num1;
                            pos.y = num2;
                        }
                        bullet.pos.x += pos.x;
                        bullet.pos.y += pos.y;
                    } else {
                        transform.position += new Vector3(num1, num2, 0f);
                    }
                    continue;
                case TimelineCommand.Command.MOVETOWARDSPOINT:
                    StopCoroutine("moveTowardsSmooth");
                    StartCoroutine(MoveTowardsSmooth(new Vector3(ParseValue(currentCommand.args[0]), ParseValue(currentCommand.args[1]), transform.position.z), ParseValue(currentCommand.args[2])));
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
                    return; //return true because nothing else should execute this tick
                case TimelineCommand.Command.BOSSWAIT:
                    bosswaiting = true;
                    return; //return true because nothing else should execute this tick
                case TimelineCommand.Command.SETPARENTHEALTH:
                    parentEnemy.health = Mathf.RoundToInt(ParseValue(currentCommand.args[0]));
                    parentEnemy.template.maxHealth = parentEnemy.health;
                    parentEnemy.UpdateHealthbar();
                    continue;
                case TimelineCommand.Command.ANGLETOPLAYER: //Returns the angle to the player. Undoes the difference made by the ScriptRotationMatrix
                    pos = transform.position;
                    playerpos = PlayerPosGetter.playerPos;
                    num1 = pos.x - playerpos.x;
                    num2 = pos.y - playerpos.y;
                    num1 = Mathf.Atan2(-num1, -num2);
                    if (parentBullet != null) {
                        num1 -= parentBullet.bulletTemplate.scriptRotation;
                    }
                    SetNumber(currentCommand.args[0], num1);
                    continue;
                case TimelineCommand.Command.ANGLETOPOINT:
                    pos = transform.position;
                    Vector2 pos2 = new Vector2(ParseValue(currentCommand.args[1]), ParseValue(currentCommand.args[2]));
                    num1 = pos.x - pos2.x;
                    num2 = pos.y - pos2.y;
                    SetNumber(currentCommand.args[0], Mathf.Atan2(-num1, -num2));
                    continue;
                case TimelineCommand.Command.GETPLAYERPOSITION:
                    playerpos = PlayerPosGetter.playerPos;
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
                case TimelineCommand.Command.LOG:
                    Debug.Log("[Timeline Log] " + currentCommand.args[0] + ":" + ParseValue(currentCommand.args[1]));
                    continue;
                default:
                    continue;
            }
        }

        return;
    }

    /// <summary>
    /// Sets the number in the numberVars list to whatever value is.
    /// </summary>
    private void SetNumber(string name, float value) {
        if (name[0] == 95) { //Starts with '_', so global
            stringHash = name.GetHashCode();
            if (globalNumberVars.ContainsKey(stringHash)) {
                globalNumberVars[stringHash] = value;
            } else {
                globalNumberVars.Add(stringHash, value);
            }
        } else {
            numberVars[int.Parse(name)] = value;
        }
    }

    /// <summary>
    /// Gets the number in the numberVars dictionary by name. If it doesn't exist, it creates it, sets it to zero, and returns zero.
    /// </summary>
    private float GetNumber(string name) { 
        float returnFloat;
        if (name[0] == 95) { //Starts with '_', so global
            stringHash = name.GetHashCode();
            if (!globalNumberVars.TryGetValue(stringHash, out returnFloat)) {
                globalNumberVars.Add(stringHash, 0f);
                return 0f;
            }
            return returnFloat;
        } else {
            return numberVars[int.Parse(name)];
        }
    }

    /// <summary>
    /// Gets the Bullet
    /// late in the numberVars dictionary by name. If it doesn't exist, it creates it, and returns it.
    /// </summary>
    private BulletTemplate GetBulletTemplate(string name) {
        if (name[0] == 95) { //Starts with '_', so global
            stringHash = name.GetHashCode();
            if (!globalBulletTemplateVars.ContainsKey(stringHash)) {
                globalBulletTemplateVars.Add(stringHash, BulletTemplate.basic);
                return BulletTemplate.basic;
            } else {
                return globalBulletTemplateVars[stringHash];
            }
        } else {
            return bulletTemplateVars[int.Parse(name)];
        }
    }

    /// <summary>
    /// Sets the BulletTemplate in the bulletTemplateVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    private void SetBulletTemplate(string name, BulletTemplate value) {
        stringHash = name.GetHashCode();
        if (name[0] == 95) { //Starts with '_', so global
            if (globalBulletTemplateVars.ContainsKey(stringHash)) {
                globalBulletTemplateVars[stringHash] = value;
            } else {
                globalBulletTemplateVars.Add(stringHash, value);
            }
        } else {
            bulletTemplateVars[int.Parse(name)] = value;
        }
    }

    /// <summary>
    /// Gets the EnemyTemplate in the numberVars dictionary by name. If it doesn't exist, it creates it, and returns it.
    /// </summary>
    private EnemyTemplate GetEnemyTemplate(string name) {
        if (name[0] == 95) { //Starts with '_', so global
            stringHash = name.GetHashCode();
            if (!globalEnemyTemplateVars.ContainsKey(stringHash)) {
                globalEnemyTemplateVars.Add(stringHash, new EnemyTemplate());
                return new EnemyTemplate();
            } else {
                return globalEnemyTemplateVars[stringHash];
            }
        } else {
            return enemyTemplateVars[int.Parse(name)];
        }
    }

    /// <summary>
    /// Sets the EnemyTemplate in the enemyTemplateVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    private void SetEnemyTemplate(string name, EnemyTemplate value) {
        if (name[0] == 95) { //Starts with '_', so global
            stringHash = name.GetHashCode();
            if (globalEnemyTemplateVars.ContainsKey(stringHash)) {
                globalEnemyTemplateVars[stringHash] = value;
            } else {
                globalEnemyTemplateVars.Add(stringHash, value);
            }
        } else {
            enemyTemplateVars[int.Parse(name)] = value;
        }
    }

    private LaserTemplate GetLaserTemplate(string name) {
        if (name[0] == 95) { //Starts with '_', so global
            stringHash = name.GetHashCode();
            if (!globalLaserTemplateVars.ContainsKey(stringHash)) {
                globalLaserTemplateVars.Add(stringHash, LaserTemplate.basic);
                return LaserTemplate.basic;
            } else {
                return globalLaserTemplateVars[stringHash];
            }
        } else {
            return laserTemplateVars[int.Parse(name)];
        }
    }

    private void SetLaserTemplate(string name, LaserTemplate value) {
        if (name[0] == 95) { //Starts with '_',so global
            stringHash = name.GetHashCode();
            if (globalLaserTemplateVars.ContainsKey(stringHash)) {
                globalLaserTemplateVars[stringHash] = value;
            } else {
                globalLaserTemplateVars.Add(stringHash, value);
            }
        } else {
            laserTemplateVars[int.Parse(name)] = value;
        }
    }

    /// <summary>
    /// Returns whatever is between the first open brace and second to last character, seperated by comma's.
    /// </summary>
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
    public static string GetFunction(string toEvaluate) { 
        string returnString = toEvaluate.Split('(')[0];
        return returnString;
    }
    
    /// <summary>
    /// Parses value - whether it's a number or numberVars var name. Numbers start with an "n", while numberVars var names are full numbers
    /// </summary>
    private float ParseValue(string value) {
        if (value[0] != 'n') { //It's a variable
            return GetNumber(value);
        } else { //It's a number
            return NumberFunctions.ParseFloat(value.Substring(1));
        }
    }

    /// <summary>
    /// Moves from the current position to endPos, taking "time" ticks. (It's a float, but has the same 60-ticks-is-a-second rule.)
    /// </summary>
    private IEnumerator MoveTowardsSmooth(Vector3 endPos, float time) {
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
