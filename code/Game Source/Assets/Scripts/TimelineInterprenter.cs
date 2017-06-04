﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// A class reading .txt's describing either enemy or bullet info. Important in those .txt's is the wait(x) function, which is in ticks and not second, because second would not allow replays.
/// </summary>
public class TimelineInterprenter : MonoBehaviour {
    //TODO: Dictionaries are expensive when called from 1500 bullets.
    /* Also dictionary trygetvalue example from stackoverflow, slightly faster on its own:
     *    obj item;
     *    if(!dict.TryGetValue(name, out item))
     *      return null;
     *    return item;
     */
    public string patternPath = "";
    private Dictionary<int, float> numberVars = new Dictionary<int, float>();
    private Dictionary<int, BulletTemplate> bulletTemplateVars = new Dictionary<int, BulletTemplate>();
    private Dictionary<int, EnemyTemplate> enemyTemplateVars = new Dictionary<int, EnemyTemplate>();
    private Dictionary<int, LaserTemplate> laserTemplateVars = new Dictionary<int, LaserTemplate>();
    private string[] instructions;
    private List<int> repeatStepback = new List<int>(); //What line to go to when encountering an endrepeat.
    private Enemy parentEnemy;
    private int currentLine = 0;
    private int cooldown = 0;

    //Vars needed within the for loop
    private List<TimelineCommand> commands = new List<TimelineCommand>();
    private int count, layers, findEndRepeatLine, lineDifference;
    private float num1, num2;
    private TimelineCommand currentCommand;
    private TimelineCommand.Command findFunction;
    private BulletTemplate bulletTemplate;
    private BulletTemplate parentTemplate;
    private EnemyTemplate enemyTemplate;
    private Vector3 pos, playerpos;
    private int stringHash;

    public void Reset(string newTimeLine) {
        patternPath = newTimeLine;
        numberVars.Clear();
        bulletTemplateVars.Clear();
        enemyTemplateVars.Clear();
        instructions = null;
        repeatStepback = new List<int>();
        parentEnemy = transform.GetComponent<Enemy>();
        currentLine = 0;
        cooldown = 0;
        ReadAttack(true);
    }

    //OnEnable() to add the TickTimeline to the tick all timelines delegate, OnDisable() to remove them.
    void OnEnable() {
        GlobalHelper.Tick += TickTimeline;
    }

    void OnDisable() {
        GlobalHelper.Tick -= TickTimeline;
    }

    void Start() {
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
            string file = (Instantiate((TextAsset)Resources.Load(patternPath))).text;
            int hash = file.GetHashCode();

            if (!GlobalHelper.commandLists.TryGetValue(hash, out commands)) { //If it's already a list of commands recognised, no need to parse it again. Otherwise, parse is needed.
                file = file.Replace(" ", ""); //Simple cleanup. This also makes ReadAttack totally inappropriate for dialogue.
                file = file.Replace("\n", "");
                file = file.Replace("\r", "");
                instructions = file.Split(';'); //This splits instructions.
                string function;
                List<string> args;
                commands = new List<TimelineCommand>();
                foreach (string instruction in instructions) { //Turn the file into a list of commands
                    //If the first thing is a comment, skip it.
                    if (instruction.Length == 0 || instruction[0] == '/' || instruction[0] == '#') {
                        continue;
                    }

                    function = GetFunction(instruction).ToLowerInvariant();
                    args = GetArguments(instruction);
                    if (function == "bulletproperty") { //If this command sets a bullet property...
                        function = args[1];
                        args.RemoveAt(1);
                        commands.Add(new TimelineCommand(
                            (TimelineCommand.BulletProperty)Enum.Parse(typeof(TimelineCommand.BulletProperty), function.ToUpperInvariant()),
                            args
                            ));
                        continue;
                    } else if (function == "enemyproperty") { //Or an enemy's property
                        function = args[1];
                        args.RemoveAt(1);
                        commands.Add(new TimelineCommand(
                            (TimelineCommand.EnemyProperty)Enum.Parse(typeof(TimelineCommand.EnemyProperty), function.ToUpperInvariant()),
                            args
                            ));
                        continue;
                    } else if (function == "laserproperty") {
                        function = args[1];
                        args.RemoveAt(1);
                        commands.Add(new TimelineCommand(
                            (TimelineCommand.LaserProperty)Enum.Parse(typeof(TimelineCommand.LaserProperty), function.ToUpperInvariant()),
                            args
                            ));
                    } else { //At this point no bullet/enemy properties are being set so it's just parsing the TimelineCommand.Command
                        commands.Add(new TimelineCommand(
                            (TimelineCommand.Command)Enum.Parse(typeof(TimelineCommand.Command), function.ToUpperInvariant()),
                            args
                            ));
                        continue;
                    }
                }
                GlobalHelper.commandLists.Add(hash, commands);
            }
        }
        ReadAttack();
    }

    /* TODO:
     * IF; [..] ENDIF; IF; [..] ELSE; [..] ENDIF;
     */
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
                    layers = 0; //Goes down for every Repeat(x) line. Goes up for every Endrepeat line.
                    for (findEndRepeatLine = currentLine + 1; layers != 1; findEndRepeatLine++) {
                        findFunction = commands[findEndRepeatLine].command;
                        if (findFunction == TimelineCommand.Command.REPEAT) {
                            layers--;
                        } else if (findFunction == TimelineCommand.Command.ENDREPEAT) {
                            layers++;
                        }
                    }
                    lineDifference = 1 + currentLine - findEndRepeatLine; //This should be how much to go back after hitting "endrepeat". Add 1g because it's 1 off and would infinite-loop.
                    repeatStepback.Add(0); //After hitting it the final time it should go to after the end. Because of the loops ++, going to exactly the end is fine.
                    for (int i = 0; i < count; i++) {
                        repeatStepback.Add(lineDifference);
                    }
                    continue;
                case TimelineCommand.Command.ENDREPEAT:
                    currentLine += repeatStepback[repeatStepback.Count - 1];
                    repeatStepback.RemoveAt(repeatStepback.Count - 1);
                    continue;
                case TimelineCommand.Command.BULLETPROPERTY:
                    bulletTemplate = new BulletTemplate(GetBulletTemplate(currentCommand.args[0]));
                    switch (currentCommand.bulletProperty) {
                        case TimelineCommand.BulletProperty.SCRIPTROTATION:
                            if (GetComponent<Bullet>() != null) { //If the parent is a bullet also add its angle.
                                bulletTemplate.Rotate(GetComponent<Bullet>().bulletTemplate.scriptRotation + ParseValue(currentCommand.args[1]));
                                break;
                            } else {
                                bulletTemplate.Rotate(ParseValue(currentCommand.args[1]));
                                break;
                            }
                        case TimelineCommand.BulletProperty.MOVEMENT:
                            num1 = ParseValue(currentCommand.args[1]);
                            num2 = ParseValue(currentCommand.args[2]);
                            if (GetComponent<Bullet>() != null) { //If the parent is a bullet, change its movement to be rotated
                                parentTemplate = GetComponent<Bullet>().bulletTemplate;
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
                            if (GetComponent<Bullet>() != null) { //If the parent is a bullet, change its position to be rotated 
                                parentTemplate = GetComponent<Bullet>().bulletTemplate;
                                pos.x = num1 * parentTemplate.scriptRotationMatrix.x + num2 * parentTemplate.scriptRotationMatrix.y;
                                pos.y = num1 * parentTemplate.scriptRotationMatrix.z + num2 * parentTemplate.scriptRotationMatrix.w;
                            } else {
                                pos.x = num1;
                                pos.y = num2;
                            }
                            bulletTemplate.position = pos;
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
                            bulletTemplate.rotation = ParseValue(currentCommand.args[1]);
                            break;
                        case TimelineCommand.BulletProperty.RELATIVEPOS:
                            bulletTemplate.positionIsRelative = ParseValue(currentCommand.args[1]) > 0 ? true : false;
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
                            for (int i = 1; i < currentCommand.args.Count; i++) {
                                enemyTemplate.attackPath.Add(currentCommand.args[i]/* + "_" + (int)GlobalHelper.difficulty*/);
                            }
                            break;
                        /*case TimelineCommand.EnemyProperty.SPELLCARDNAME:
                            //States the name of every spellcard. Sadly neccessary to do this way because of how I built the system.
                            //.. I mean, you have to say what you're casting before the duel! Just like in canon!
                            enemyTemplate.spellcardName.Clear();
                            for (int i = 1; i < currentCommand.args.Count; i++) {
                                enemyTemplate.spellcardName.Add(currentCommand.args[i].Replace('_', ' '));
                            }
                            break;*/
                        case TimelineCommand.EnemyProperty.TIME: //States the time for each spellcard. ..probably best off making spellcards a class at this point.
                            enemyTemplate.spellTimers.Clear();
                            for (int i = 1; i < currentCommand.args.Count; i++) {
                                enemyTemplate.spellTimers.Add(Mathf.RoundToInt(ParseValue(currentCommand.args[i])));
                            }
                            break;
                        case TimelineCommand.EnemyProperty.ID: //Sets the ID of the IMAGE of the enemy, as defined in Resources/Graphics/Enemies.
                            enemyTemplate.enemyID = Mathf.RoundToInt(ParseValue(currentCommand.args[1]));
                            break;
                        case TimelineCommand.EnemyProperty.COLORISE:
                            enemyTemplate.colorise = ParseValue(currentCommand.args[1]) > 0 ? true : false;
                            break;
                        case TimelineCommand.EnemyProperty.COLOR:
                            enemyTemplate.color.r = ParseValue(currentCommand.args[1]);
                            enemyTemplate.color.g = ParseValue(currentCommand.args[2]);
                            enemyTemplate.color.b = ParseValue(currentCommand.args[3]);
                            enemyTemplate.color.a = ParseValue(currentCommand.args[4]);
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
                        case TimelineCommand.LaserProperty.BULLET:
                            laserTemplate.shotBullet = GetBulletTemplate(currentCommand.args[1]);
                            break;
                    }
                    SetLaserTemplate(currentCommand.args[0], laserTemplate);
                    continue;
                case TimelineCommand.Command.CREATEBULLET:
                    if (GetComponent<Bullet>() != null) { //If a bulet is firing this, pass the script rotation data onto the new bullet.
                        bulletTemplate.Rotate(GetComponent<Bullet>().bulletTemplate.scriptRotation + bulletTemplate.rotation);
                    }
                    GlobalHelper.CreateBullet(GetBulletTemplate(currentCommand.args[0]), transform.position);
                    continue;
                case TimelineCommand.Command.CREATEENEMY:
                    GlobalHelper.CreateEnemy(GetEnemyTemplate(currentCommand.args[0]));
                    continue;
                case TimelineCommand.Command.CREATELASER:
                    //TODO GlobalHelper.CreateLaser(GetLaserTemplate(currentCommand.args[0]), transform.position); //todo: test
                    continue;
                case TimelineCommand.Command.MOVEPARENT:
                    if (transform.GetComponent<Bullet>() != null) { //If this is a bullet, posx,y(,z) should be modified, not its direct position
                        Bullet bullet = transform.GetComponent<Bullet>();
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
                case TimelineCommand.Command.DESTROYPARENT: //Destroys whatever this is attached to.
                    Destroy(transform.gameObject);
                    continue;
                case TimelineCommand.Command.WAIT:
                    cooldown = Mathf.RoundToInt(ParseValue(currentCommand.args[0]));
                    return;
                case TimelineCommand.Command.SETPARENTHEALTH:
                    parentEnemy.health = Mathf.RoundToInt(ParseValue(currentCommand.args[0]));
                    parentEnemy.template.maxHealth = parentEnemy.health;
                    parentEnemy.UpdateHealthbar();
                    continue;
                case TimelineCommand.Command.SETPARENTSCORE:
                    parentEnemy.template.baseScore = (uint)Mathf.RoundToInt(ParseValue(currentCommand.args[0]));
                    continue;
                case TimelineCommand.Command.ANGLETOPLAYER: //Returns the angle to the player.
                    pos = transform.position;
                    playerpos = GameObject.FindWithTag("Player").transform.position;
                    num1 = pos.x - playerpos.x;
                    num2 = pos.y - playerpos.y;
                    SetNumber(currentCommand.args[0], Mathf.Atan2(-num1, -num2));
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
        stringHash = name.GetHashCode();
        if (numberVars.ContainsKey(stringHash)) {
            numberVars[stringHash] = value;
        } else {
            numberVars.Add(stringHash, value);
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
        if (!numberVars.TryGetValue(stringHash, out returnFloat)) {
            numberVars.Add(stringHash, 0f);
            return 0f;
        }
        return returnFloat;
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

    private float ParseFloat(string value) {
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
    private float tenPower(float f, int power) {
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
    /// Gets the BulletTemplate in the numberVars dictionary by name. If it doesn't exist, it creates it, and returns it.
    /// </summary>
    /// <param name="name">The var name to retrieve</param>
    /// <returns></returns>
    private BulletTemplate GetBulletTemplate(string name) {
        stringHash = name.GetHashCode();
        if (!bulletTemplateVars.ContainsKey(stringHash)) {
            bulletTemplateVars.Add(stringHash, new BulletTemplate());
            return new BulletTemplate();
        } else {
            return bulletTemplateVars[stringHash];
        }
    }

    /// <summary>
    /// Sets the BulletTemplate in the bulletTemplateVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    /// <param name="name">The name of the var</param>
    /// <param name="value">The value of the var</param>
    private void SetBulletTemplate(string name, BulletTemplate value) {
        stringHash = name.GetHashCode();
        if (bulletTemplateVars.ContainsKey(stringHash)) {
            bulletTemplateVars[stringHash] = value;
        } else {
            bulletTemplateVars.Add(stringHash, value);
        }
    }

    /// <summary>
    /// Gets the EnemyTemplate in the numberVars dictionary by name. If it doesn't exist, it creates it, and returns it.
    /// </summary>
    /// <param name="name">The var name to retrieve</param>
    /// <returns></returns>
    private EnemyTemplate GetEnemyTemplate(string name) {
        stringHash = name.GetHashCode();
        if (!enemyTemplateVars.ContainsKey(stringHash)) {
            enemyTemplateVars.Add(stringHash, new EnemyTemplate());
            return new EnemyTemplate();
        } else {
            return enemyTemplateVars[stringHash];
        }
    }

    /// <summary>
    /// Sets the EnemyTemplate in the enemyTemplateVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    /// <param name="name">The name of the var</param>
    /// <param name="value">The value of the var</param>
    private void SetEnemyTemplate(string name, EnemyTemplate value) {
        stringHash = name.GetHashCode();
        if (enemyTemplateVars.ContainsKey(stringHash)) {
            enemyTemplateVars[stringHash] = value;
        } else {
            enemyTemplateVars.Add(stringHash, value);
        }
    }

    private LaserTemplate GetLaserTemplate(string name) {
        stringHash = name.GetHashCode();
        if (!laserTemplateVars.ContainsKey(stringHash)) {
            laserTemplateVars.Add(stringHash, new LaserTemplate());
            return new LaserTemplate();
        } else {
            return laserTemplateVars[stringHash];
        }
    }

    private void SetLaserTemplate(string name, LaserTemplate value) {
        stringHash = name.GetHashCode();
        if (laserTemplateVars.ContainsKey(stringHash)) {
            laserTemplateVars[stringHash] = value;
        } else {
            laserTemplateVars.Add(stringHash, value);
        }
    }

    /// <summary>
    /// A faster method than 26 String.Contains(char)'s for my needs.
    /// </summary>
    /// <param name="toEvaluate">The string to check if it has letters.</param>
    /// <returns>True if it contains any of a-z or A-Z.</returns>
    private bool ContainsLetters(string toEvaluate) {
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
    private List<string> GetArguments(string toEvaluate) {
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
    private string GetFunction(string toEvaluate) { 
        string returnString = toEvaluate.Split('(')[0];
        return returnString;
    }
}
