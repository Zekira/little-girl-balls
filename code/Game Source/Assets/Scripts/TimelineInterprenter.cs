using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// A class reading .txt's describing either enemy or bullet info. Important in those .txt's is the wait(x) function, which is in ticks and not second, because second would not allow replays.
/// </summary>
public class TimelineInterprenter : MonoBehaviour {
    //TODO: GC is horrible, mostly because of String.Split();
    //TODO II: Dictionaries are expensive when called from 1500 bullets.
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
    private string[] instructions;
    private List<int> repeatStepback = new List<int>(); //What line to go to when encountering an endrepeat.
    private Enemy parentEnemy;
    private int currentLine = 0;
    private int cooldown = 0;

    //Vars needed within the for loop
    private int instructionLength;
    private string[] functions;
    private string[] args;
    private int count, layers, findEndRepeatLine, lineDifference;
    private float num1, num2;
    private string findFunction;
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
    /// Reads the text (defined in patternPath) line by line and does stuff depending on the info.
    /// The syntax of those lines is usually <functionname>([argument[,other arguments .. ]]);
    /// Any text evaluated should be all-lowercase.
    /// Set "initialise" to true to reset all values before starting reading.
    /// </summary>
    public void ReadAttack(bool initialise) {
        if (initialise) {
            string file = (Instantiate((TextAsset)Resources.Load(patternPath))).text;
            file = file.Replace(" ", ""); //Simple cleanup. This also makes ReadAttack totally inappropriate for dialogue.
            file = file.Replace("\n", "");
            file = file.Replace("\r", "");
            instructions = file.Split(';'); //This splits instructions.
            instructionLength = instructions.Length;

            functions = GetFunctions(instructions);
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
    public void ReadAttack() { //Somehow the self ms is 25ms with 1300 calls. Unacceptable. TODO.
        for (; currentLine < instructionLength; currentLine++) {
            //Skip if the line is a comment or empty.
            if (instructions[currentLine].Length == 0 ||  instructions[currentLine][0] == '/' || instructions[currentLine][0] == '#') {
                continue;
            }

            //Take the part before the brackets and try to figure out what it says and do something with it.
            switch (functions[currentLine]) {
                case "starttimeline":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    //Attaches ANOTHER TimelineInterprenter to this GameObject with path args[0].
                    TimelineInterprenter newpattern = transform.gameObject.AddComponent<TimelineInterprenter>();
                    newpattern.patternPath = args[0];
                    args = null;
                    continue;
                case "dialogue":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    GlobalHelper.levelManager.GetComponent<DialogueManager>().startDialogue(args[0]);
                    cooldown = 1;
                    return; //The next loop should not happen immediatly, but after the dialogue has been processed. That's checked within Update().
                case "repeat":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    //Executes everything between here and the matching endrepeat args[0] times.
                    count = Mathf.RoundToInt(ParseValue(args[0])) - 1;
                    layers = 0; //Goes down for every Repeat(x) line. Goes up for every Endrepeat line.
                    for (findEndRepeatLine = currentLine + 1; layers != 1; findEndRepeatLine++) {
                        findFunction = functions[findEndRepeatLine];
                        if (findFunction == "repeat") {
                            layers--;
                        } else if (findFunction == "endrepeat") {
                            layers++;
                        }
                    }
                    lineDifference = 1 + currentLine - findEndRepeatLine; //This should be how much to go back after hitting "endrepeat". Add 1g because it's 1 off and would infinite-loop.
                    repeatStepback.Add(0); //After hitting it the final time it should go to after the end. Because of the loops ++, going to exactly the end is fine.
                    for (int i = 0; i < count; i++) {
                        repeatStepback.Add(lineDifference);
                    }
                    continue;
                case "endrepeat":
                    currentLine += repeatStepback[repeatStepback.Count - 1];
                    repeatStepback.RemoveAt(repeatStepback.Count - 1);
                    continue;
                case "bulletproperty":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    bulletTemplate = new BulletTemplate(GetBulletTemplate(args[0]));
                    switch (args[1]) {
                        case "scriptrotation":
                            if (GetComponent<Bullet>() != null) { //If the parent is a bullet also add its angle.
                                bulletTemplate.Rotate(GetComponent<Bullet>().bulletTemplate.scriptRotation + ParseValue(args[2]));
                                break;
                            } else {
                                bulletTemplate.Rotate(ParseValue(args[2]));
                                break;
                            }
                        case "movement":
                            num1 = ParseValue(args[2]);
                            num2 = ParseValue(args[3]);
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
                        case "position":
                            num1 = ParseValue(args[2]);
                            num2 = ParseValue(args[3]);
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
                        case "isharmful":
                            bulletTemplate.isHarmful = ParseValue(args[2]) > 0 ? true : false;
                            break;
                        case "scale":
                            bulletTemplate.scale = ParseValue(args[2]);
                            break;
                        case "id":
                            bulletTemplate.bulletID = (byte)Mathf.RoundToInt(ParseValue(args[2]));
                            break;
                        case "innercolor":
                            bulletTemplate.innerColor.r = ParseValue(args[2]);
                            bulletTemplate.innerColor.g = ParseValue(args[3]);
                            bulletTemplate.innerColor.b = ParseValue(args[4]);
                            bulletTemplate.innerColor.a = ParseValue(args[5]);
                            break;
                        case "outercolor":
                            bulletTemplate.outerColor.r = ParseValue(args[2]);
                            bulletTemplate.outerColor.g = ParseValue(args[3]);
                            bulletTemplate.outerColor.b = ParseValue(args[4]);
                            bulletTemplate.outerColor.a = ParseValue(args[5]);
                            break;
                        case "rotation":
                            bulletTemplate.rotation = ParseValue(args[2]);
                            break;
                        case "relativepos":
                            bulletTemplate.positionIsRelative = ParseValue(args[2]) > 0 ? true : false;
                            break;
                        case "advancedpath":
                            bulletTemplate.advancedAttackPath = args[2];
                            break;
                        case "clearimmune":
                            bulletTemplate.clearImmune = ParseValue(args[2]) > 0 ? true : false;
                            break;
                        default:
                            break;
                    }
                    SetBulletTemplate(args[0], bulletTemplate);
                    continue;
                case "enemyproperty":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    enemyTemplate = new EnemyTemplate(GetEnemyTemplate(args[0]));
                    switch (args[1]) {
                        case "scale":
                            enemyTemplate.scale = ParseValue(args[2]);
                            break;
                        case "attackpath": //Sets one or more attackpaths of this enemy. Clears previous attackpaths.
                            enemyTemplate.attackPath.Clear();
                            for (int i = 2; i < args.Length; i++) {
                                enemyTemplate.attackPath.Add(args[i] + "_" + (int)GlobalHelper.difficulty);
                            }
                            break;
                        case "spellcardname":
                            //States the name of every spellcard. Sadly neccessary to do this way because of how I built the system.
                            //.. I mean, you have to say what you're casting before the duel! Just like in canon!
                            enemyTemplate.spellcardName.Clear();
                            for (int i = 2; i < args.Length; i++) {
                                enemyTemplate.spellcardName.Add(args[i].Replace('_', ' '));
                            }
                            break;
                        case "time": //States the time for each spellcard. ..probably best off making spellcards a class at this point.
                            enemyTemplate.spellTimers.Clear();
                            for (int i = 2; i < args.Length; i++) {
                                enemyTemplate.spellTimers.Add(Mathf.RoundToInt(ParseValue(args[i])));
                            }
                            break;
                        case "id": //Sets the ID of the IMAGE of the enemy, as defined in Resources/Graphics/Enemies.
                            enemyTemplate.enemyID = Mathf.RoundToInt(ParseValue(args[2]));
                            break;
                        case "colorise":
                            enemyTemplate.colorise = ParseValue(args[2]) > 0 ? true : false;
                            break;
                        case "color":
                            enemyTemplate.color.r = ParseValue(args[2]);
                            enemyTemplate.color.g = ParseValue(args[3]);
                            enemyTemplate.color.b = ParseValue(args[4]);
                            enemyTemplate.color.a = ParseValue(args[5]);
                            break;
                        case "boss": //Requires special UI stuff.
                            enemyTemplate.isBoss = ParseValue(args[2]) > 0 ? true : false;
                            break;
                        case "bossportrait": //Enum name of the boss, used with the caster's portrait
                            enemyTemplate.character = (DialogueEntry.character)Enum.Parse(typeof(DialogueEntry.character), args[2].ToUpperInvariant());
                            break;
                        case "maxhealth":
                            enemyTemplate.maxHealth = Mathf.RoundToInt(ParseValue(args[2]));
                            break;
                        case "dropvalue":
                            enemyTemplate.dropValueCount = Mathf.RoundToInt(ParseValue(args[2]));
                            break;
                        case "droppower":
                            //Sets different values depending on args[2] for the different types of power items.
                            int power = Mathf.RoundToInt(ParseValue(args[2]));
                            if (power >= 400) {
                                enemyTemplate.dropPowerFullCount = 1;
                                break;
                            }
                            enemyTemplate.dropPowerCount = (power % 100) / 5;
                            enemyTemplate.dropPowerLargeCount = power / 100;
                            break;
                        case "dropscore":
                            enemyTemplate.dropScoreCount = Mathf.RoundToInt(ParseValue(args[2]));
                            break;
                        case "startpos":
                            enemyTemplate.startpostion = new Vector2(ParseValue(args[2]), ParseValue(args[3]));
                            break;
                        default:
                            break;
                    }
                    SetEnemyTemplate(args[0], enemyTemplate);
                    continue;
                case "createbullet":
                    if (GetComponent<Bullet>() != null) { //If a bulet is firing this, pass the script rotation data onto the new bullet.
                        bulletTemplate.Rotate(GetComponent<Bullet>().bulletTemplate.scriptRotation + bulletTemplate.rotation);
                    }
                    args = GetArguments(instructions[currentLine]).Split(',');
                    GlobalHelper.CreateBullet(GetBulletTemplate(args[0]), transform.position);
                    continue;
                case "createenemy":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    GlobalHelper.CreateEnemy(GetEnemyTemplate(args[0]));
                    continue;
                case "moveparent":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    if (transform.GetComponent<Bullet>() != null) { //If this is a bullet, posx,y(,z) should be modified, not its direct position
                        Bullet bullet = transform.GetComponent<Bullet>();
                        num1 = ParseValue(args[0]);
                        num2 = ParseValue(args[1]);
                        pos.x = num1 * bullet.bulletTemplate.scriptRotationMatrix.x + num2 * bullet.bulletTemplate.scriptRotationMatrix.y;
                        pos.y = num1 * bullet.bulletTemplate.scriptRotationMatrix.z + num2 * bullet.bulletTemplate.scriptRotationMatrix.w;
                        bullet.posx += pos.x;
                        bullet.posy += pos.y;
                    } else {
                        transform.position += new Vector3(ParseValue(args[0]), ParseValue(args[1]), 0f);
                    }
                    continue;
                case "destroyparent": //Destroys whatever this is attached to.
                    Destroy(transform.gameObject);
                    continue;
                case "wait":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    cooldown = Mathf.RoundToInt(ParseValue(args[0]));
                    return;
                case "setparenthealth":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    parentEnemy.health = Mathf.RoundToInt(ParseValue(args[0]));
                    if (parentEnemy.health > parentEnemy.template.maxHealth) {
                        parentEnemy.template.maxHealth = parentEnemy.health;
                    }
                    parentEnemy.UpdateHealthbar();
                    continue;
                case "setparentmaxhealth":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    parentEnemy.template.maxHealth = Mathf.RoundToInt(ParseValue(args[0]));
                    parentEnemy.UpdateHealthbar();
                    continue;
                case "setparentscore":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    parentEnemy.template.baseScore = (uint)Mathf.RoundToInt(ParseValue(args[0]));
                    continue;
                case "angletoplayer": //Returns the angle to the player.
                    args = GetArguments(instructions[currentLine]).Split(',');
                    pos = transform.position;
                    playerpos = GameObject.FindWithTag("Player").transform.position;
                    num1 = pos.x - playerpos.x;
                    num2 = pos.y - playerpos.y;
                    SetNumber(args[0], Mathf.Atan2(-num1, -num2));
                    continue;
                case "random": //Returns a random value between args[1] and args[2]. Uses the GlobalHelper.random because everything random should do that because of replay support.
                    args = GetArguments(instructions[currentLine]).Split(',');
                    num1 = ParseValue(args[1]);
                    num2 = ParseValue(args[2]);
                    SetNumber(args[0], ((float)GlobalHelper.random.NextDouble()) * (num2 - num1) + num1);
                    continue;
                case "set": //What follows are a bunch of selfexplanatory math functions: set, addition, subtraction, multiplication, division, modulo, power, trig functions, absolute
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]));
                    continue;
                case "add":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) + ParseValue(args[2]));
                    continue;
                case "sub":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) - ParseValue(args[2]));
                    continue;
                case "mul":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) * ParseValue(args[2]));
                    continue;
                case "div":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) / ParseValue(args[2]));
                    continue;
                case "mod":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) % ParseValue(args[2]));
                    continue;
                case "pow":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Pow(ParseValue(args[1]), ParseValue(args[2])));
                    continue;
                case "sin":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Sin(ParseValue(args[1])));
                    continue;
                case "asin":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Asin(ParseValue(args[1])));
                    continue;
                case "cos":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Cos(ParseValue(args[1])));
                    continue;
                case "acos":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Acos(ParseValue(args[1])));
                    continue;
                case "tan":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Tan(ParseValue(args[1])));
                    continue;
                case "atan":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Atan(ParseValue(args[1])));
                    continue;
                case "abs":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Abs(ParseValue(args[1])));
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
    private float GetNumber(string name) { //4000 calls from 1300 interprenters, 9 ms. Maybe optimisable? TODO
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
        float returnValue;
        if (ContainsLetters(value)) {
            returnValue = GetNumber(value);
        } else {
            returnValue = ParseFloat(value);
        }
        return returnValue;
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

    /// <summary>
    /// A faster method than 26 String.Contains(char)'s for my needs.
    /// </summary>
    /// <param name="toEvaluate">The string to check if it has letters.</param>
    /// <returns>True if it contains any of a-z or A-Z.</returns>
    private bool ContainsLetters(string toEvaluate) {
        for (int i = 0; i < toEvaluate.Length; i++) {
            int charNumber = toEvaluate[i];
            if (charNumber >= 65 && charNumber <= 122) { //All letters in UTF-16 from A to Z to a to z
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns whatever is between the first open brace and second to last character.
    /// </summary>
    /// <param name="toEvaluate">The string to evaluate.</param>
    /// <returns>Everything that was originally between braces in <function>(args[0],args[1] ... )</returns>
    private string GetArguments(string toEvaluate) { //5ms with 4000 calls from 1300 bullets. TODO?
        //Assuming only one set of braces
        int i = 0;
        while (toEvaluate[i] != '(') {
            i++;
        }
        string returnString = "";
        returnString += toEvaluate.Substring(i + 1, toEvaluate.Length - i - 2);
        return returnString;
    }

    /// <summary>
    /// Returns whatever is before the first open brace for each entry in the toEvaluate array.
    /// </summary>
    /// <param name="toEvaluate">The string to find the function of.</param>
    /// <returns>Returns whatever is before the first open brace.</returns>
    private string[] GetFunctions(string[] toEvaluate) { 
        string[] returnString = new string[toEvaluate.Length];
        for (int i = 0; i < toEvaluate.Length; i++) {
            returnString[i] = toEvaluate[i].Split('(')[0];
        }
        //Everything before the first open brace.
        return returnString;
    }
}
