using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
/// <summary>
/// A class reading .txt's describing either enemy or bullet info. Important in those .txt's is the wait(x) function, which is in ticks and not second, because second would not allow replays.
/// </summary>
public class TimelineInterprenter : MonoBehaviour {

    public string patternPath = "";
    private Dictionary<string, float> numberVars = new Dictionary<string, float>();
    private Dictionary<string, BulletTemplate> bulletTemplateVars = new Dictionary<string, BulletTemplate>();
    private Dictionary<string, EnemyTemplate> enemyTemplateVars = new Dictionary<string, EnemyTemplate>();
    private string[] instructions;
    private List<int> repeatStepback = new List<int>(); //What line to go to when encountering an endrepeat.
    private Enemy parentEnemy;
    public int lineCount = 0;
    public int currentLine = 0;
    public int cooldown = 0;
    public int instructionLength;

    void Start() {
        parentEnemy = transform.GetComponent<Enemy>();
        if (patternPath == "") {
            patternPath = parentEnemy.template.attackPath[0];
        }
        ReadAttack();
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
    /* TODO:
     * IF; [..] ENDIF; IF; [..] ELSE; [..] ENDIF;
     * LABEL(name);
     * GOTOLABEL(name);
     */
     /// <summary>
     /// Reads the text (defined in patternPath) line by line and does stuff depending on the info.
     /// The syntax of those lines is usually <functionname>([argument[,other arguments .. ]]);
     /// Any text evaluated should be all-lowercase.
     /// </summary>
    public void ReadAttack() {
        string file = (Instantiate((TextAsset)Resources.Load(patternPath))).text;
        file = file.Replace(" ", ""); //Simple cleanup. This also makes ReadAttack totally inappropriate for dialogue.
        file = file.Replace("\n", "");
        file = file.Replace("\r", "");
        instructions = file.Split(';'); //This splits instructions.
        instructionLength = instructions.Length;
        //Vars needed within the for loop
        string function;
        string[] args;
        int count, layers, findEndRepeatLine, lineDifference;
        float deltax, deltay, num1, num2;
        string findFunction;
        BulletTemplate bulletTemplate;
        EnemyTemplate enemyTemplate;
        Vector3 pos, playerpos;

        for (; currentLine < instructionLength; currentLine++) {
            //Skip if the line is a comment.
            if (instructions[currentLine] == "" ||  instructions[currentLine][0] == '/' && instructions[currentLine][1] == '/' || instructions[currentLine][0] == '#') {
                continue;
            }

            function = GetFunction(instructions[currentLine]);
            //Take the part before the brackets and try to figure out what it says and do something with it.
            switch (function) {
                case "starttimeline":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    //Attaches ANOTHER TimelineInterprenter to this GameObject with path args[0].
                    TimelineInterprenter newpattern = transform.gameObject.AddComponent<TimelineInterprenter>();
                    newpattern.patternPath = args[0];
                    break;
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
                        findFunction = GetFunction(instructions[findEndRepeatLine]);
                        if (findFunction.ToLower() == "repeat") {
                            layers--;
                        } else if (findFunction.ToLower() == "endrepeat") {
                            layers++;
                        }
                    }
                    lineDifference = 1 + currentLine - findEndRepeatLine; //This should be how much to go back after hitting "endrepeat". Add 1g because it's 1 off and would infinite-loop.
                    repeatStepback.Add(0); //After hitting it the final time it should go to after the end. Because of the loops ++, going to exactly the end is fine.
                    for (int i = 0; i < count; i++) {
                        repeatStepback.Add(lineDifference);
                    }
                    break;
                case "endrepeat":
                    currentLine += repeatStepback[repeatStepback.Count - 1];
                    repeatStepback.RemoveAt(repeatStepback.Count - 1);
                    break;
                case "bulletproperty":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    bulletTemplate = new BulletTemplate(GetBulletTemplate(args[0]));
                    switch (args[1].ToLower()) {
                        case "movement":
                            bulletTemplate.movement = new Vector2(ParseValue(args[2]), ParseValue(args[3]));
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
                        default:
                            break;
                    }
                    SetBulletTemplate(args[0], bulletTemplate);
                    break;
                case "enemyproperty":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    enemyTemplate = new EnemyTemplate(GetEnemyTemplate(args[0]));
                    switch (args[1].ToLower()) {
                        case "scale":
                            enemyTemplate.scale = ParseValue(args[2]);
                            break;
                        case "attackpath": //Sets one or more attackpaths of this enemy. Clears previous attackpaths.
                            enemyTemplate.attackPath.Clear();
                            for (int i = 2; i < args.Length; i++) {
                                if (enemyTemplate.attackPath.Count <= i - 2) {
                                    enemyTemplate.attackPath.Add(args[i] + "_" + (int)GlobalHelper.difficulty);
                                } else {
                                    enemyTemplate.attackPath[i - 2] = args[i] + "_" + (int)GlobalHelper.difficulty;
                                }
                            }
                            break;
                        case "spellcardname":
                            //States the name of every spellcard. Sadly neccessary to do this way because of how I built the system.
                            //.. I mean, you have to say what you're casting before the duel! Just like in canon!
                            enemyTemplate.spellcardName.Clear();
                            for (int i = 2; i < args.Length; i++) {
                                if (enemyTemplate.spellcardName.Count <= i - 2) {
                                    enemyTemplate.spellcardName.Add(args[i].Replace('_', ' '));
                                } else {
                                    enemyTemplate.spellcardName[i - 2] = args[i].Replace('_', ' ');
                                }
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
                    break;
                case "createbullet":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    GlobalHelper.CreateBullet(GetBulletTemplate(args[0]), transform.position);
                    break;
                case "createenemy":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    GlobalHelper.CreateEnemy(GetEnemyTemplate(args[0]));
                    break;
                case "moveparent":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    transform.position += new Vector3(ParseValue(args[0]), ParseValue(args[1]), 0f);
                    break;
                case "destroyparent": //Destroys whatever this is attached to.
                    Destroy(transform.gameObject);
                    break;
                case "wait":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    cooldown = Mathf.RoundToInt(ParseValue(args[0]));
                    return;
                case "settime": //Sets the timer of this enemy to some value. Practically useless for regular enemies.
                    args = GetArguments(instructions[currentLine]).Split(',');
                    parentEnemy.timer = Mathf.RoundToInt(ParseValue(args[0]));
                    break;
                case "setparenthealth":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    parentEnemy.health = Mathf.RoundToInt(ParseValue(args[0]));
                    if (parentEnemy.health > parentEnemy.template.maxHealth) {
                        parentEnemy.template.maxHealth = parentEnemy.health;
                    }
                    parentEnemy.UpdateHealthbar();
                    break;
                case "setparentmaxhealth":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    parentEnemy.template.maxHealth = Mathf.RoundToInt(ParseValue(args[0]));
                    parentEnemy.UpdateHealthbar();
                    break;
                case "setparentscore":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    parentEnemy.template.baseScore = (uint)Mathf.RoundToInt(ParseValue(args[0]));
                    break;
                case "angletoplayer": //Returns the angle to the player.
                    args = GetArguments(instructions[currentLine]).Split(',');
                    pos = transform.position;
                    playerpos = GameObject.FindWithTag("Player").transform.position;
                    deltax = pos.x - playerpos.x;
                    deltay = pos.y - playerpos.y;
                    SetNumber(args[0], Mathf.Atan2(-deltax, -deltay));
                    break;
                case "random": //Returns a random value between args[1] and args[2]. Uses the GlobalHelper.random because everything random should do that because of replay support.
                    args = GetArguments(instructions[currentLine]).Split(',');
                    num1 = ParseValue(args[1]);
                    num2 = ParseValue(args[2]);
                    SetNumber(args[0], ((float)GlobalHelper.random.NextDouble()) * (num2 - num1) + num1);
                    break;
                case "set": //What follows are a bunch of selfexplanatory math functions: set, addition, subtraction, multiplication, division, modulo, power, trig functions, absolute
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]));
                    break;
                case "add":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) + ParseValue(args[2]));
                    break;
                case "sub":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) - ParseValue(args[2]));
                    break;
                case "mul":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) * ParseValue(args[2]));
                    break;
                case "div":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) / ParseValue(args[2]));
                    break;
                case "mod":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], ParseValue(args[1]) % ParseValue(args[2]));
                    break;
                case "pow":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Pow(ParseValue(args[1]), ParseValue(args[2])));
                    break;
                case "sin":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Sin(ParseValue(args[1])));
                    break;
                case "asin":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Asin(ParseValue(args[1])));
                    break;
                case "cos":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Cos(ParseValue(args[1])));
                    break;
                case "acos":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Acos(ParseValue(args[1])));
                    break;
                case "tan":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Tan(ParseValue(args[1])));
                    break;
                case "atan":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Atan(ParseValue(args[1])));
                    break;
                case "abs":
                    args = GetArguments(instructions[currentLine]).Split(',');
                    SetNumber(args[0], Mathf.Abs(ParseValue(args[1])));
                    break;
                default:
                    break;
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
        if (numberVars.ContainsKey(name)) {
            numberVars[name] = value;
        } else {
            numberVars.Add(name, value);
        }
    }

    /// <summary>
    /// Gets the number in the numberVars dictionary by name. If it doesn't exist, it creates it, sets it to zero, and returns zero.
    /// </summary>
    /// <param name="name">The var name to retrieve</param>
    /// <returns></returns>
    private float GetNumber(string name) {
        if (!numberVars.ContainsKey(name)) {
            numberVars.Add(name, 0f);
            return 0f;
        } else {
            return numberVars[name];
        }
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
        int negative = 1;
        if (value[0] == '-') {
            negative = -1;
            value.Remove(0);
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
        //Evaluate before the dot.
        for (i = 0; i < dotPosition; i++) {
            switch (value[i]) {
                case '0':
                    break;
                case '1':
                    returnValue += tenPower(1f, dotPosition - i - 1);
                    break;
                case '2':
                    returnValue += tenPower(2f, dotPosition - i - 1);
                    break;
                case '3':
                    returnValue += tenPower(3f, dotPosition - i - 1);
                    break;
                case '4':
                    returnValue += tenPower(4f, dotPosition - i - 1);
                    break;
                case '5':
                    returnValue += tenPower(5f, dotPosition - i - 1);
                    break;
                case '6':
                    returnValue += tenPower(6f, dotPosition - i - 1);
                    break;
                case '7':
                    returnValue += tenPower(7f, dotPosition - i - 1);
                    break;
                case '8':
                    returnValue += tenPower(8f, dotPosition - i - 1);
                    break;
                case '9':
                    returnValue += tenPower(9f, dotPosition - i - 1);
                    break;
            }
        }
        //Evaluate behind the dot.
        for (i = dotPosition + 1; i < stringLength; i++) {
            switch (value[i]) {
                case '0':
                    break;
                case '1':
                    returnValue += tenPower(1f, dotPosition - i);
                    break;
                case '2':
                    returnValue += tenPower(2f, dotPosition - i);
                    break;
                case '3':
                    returnValue += tenPower(3f, dotPosition - i);
                    break;
                case '4':
                    returnValue += tenPower(4f, dotPosition - i);
                    break;
                case '5':
                    returnValue += tenPower(5f, dotPosition - i);
                    break;
                case '6':
                    returnValue += tenPower(6f, dotPosition - i);
                    break;
                case '7':
                    returnValue += tenPower(7f, dotPosition - i);
                    break;
                case '8':
                    returnValue += tenPower(8f, dotPosition - i);
                    break;
                case '9':
                    returnValue += tenPower(9f, dotPosition - i);
                    break;
            }
        }
        returnValue *= negative;
        return returnValue;
    }

    /// <summary>
    /// Returns f * 10^power.
    /// </summary>
    private float tenPower(float f, int power) {
        if (power > 0) {
            float factor = 10f;
            for (int i = 0; i < Mathf.Abs(power); i++) {
                f *= factor;
            }
            return f;
        } else {
            float factor = 10f;
            for (int i = 0; i < Mathf.Abs(power); i++) {
                f /= factor;
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
        if (!bulletTemplateVars.ContainsKey(name)) {
            bulletTemplateVars.Add(name, new BulletTemplate());
            return new BulletTemplate();
        } else {
            return bulletTemplateVars[name];
        }
    }

    /// <summary>
    /// Sets the BulletTemplate in the bulletTemplateVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    /// <param name="name">The name of the var</param>
    /// <param name="value">The value of the var</param>
    private void SetBulletTemplate(string name, BulletTemplate value) {
        if (bulletTemplateVars.ContainsKey(name)) {
            bulletTemplateVars[name] = value;
        } else {
            bulletTemplateVars.Add(name, value);
        }
    }

    /// <summary>
    /// Gets the EnemyTemplate in the numberVars dictionary by name. If it doesn't exist, it creates it, and returns it.
    /// </summary>
    /// <param name="name">The var name to retrieve</param>
    /// <returns></returns>
    private EnemyTemplate GetEnemyTemplate(string name) {
        if (!enemyTemplateVars.ContainsKey(name)) {
            enemyTemplateVars.Add(name, new EnemyTemplate());
            return new EnemyTemplate();
        } else {
            return enemyTemplateVars[name];
        }
    }

    /// <summary>
    /// Sets the EnemyTemplate in the enemyTemplateVars dictionary to whatever value is. If it doesn't exists, it creates it.
    /// </summary>
    /// <param name="name">The name of the var</param>
    /// <param name="value">The value of the var</param>
    private void SetEnemyTemplate(string name, EnemyTemplate value) {
        if (enemyTemplateVars.ContainsKey(name)) {
            enemyTemplateVars[name] = value;
        } else {
            enemyTemplateVars.Add(name, value);
        }
    }

    /// <summary>
    /// A faster method than 26 String.Contains(char)'s for my needs.
    /// </summary>
    /// <param name="toEvaluate">The string to check if it has letters.</param>
    /// <returns>True if it contains any of a-z or A-Z.</returns>
    private bool ContainsLetters(string toEvaluate) {
        //The point is that I'm NOT using regex; it's slow. This is sorted by the most common occurance in the english language, as per https://en.wikipedia.org/wiki/Letter_frequency#/media/File:English_letter_frequency_(frequency).svg
        for (int i = 0; i < toEvaluate.Length; i++) {
            if (toEvaluate[i] == '0' || toEvaluate[i] == '1' || toEvaluate[i] == '2' || toEvaluate[i] == '3' || toEvaluate[i] == '4' ||
                toEvaluate[i] == '5' || toEvaluate[i] == '6' || toEvaluate[i] == '7' || toEvaluate[i] == '8' || toEvaluate[i] == '9') {
                continue;
            }
            if (toEvaluate[i] == 'e' || toEvaluate[i] == 't' || toEvaluate[i] == 'a' || toEvaluate[i] == 'o' || toEvaluate[i] == 'i' ||
            toEvaluate[i] == 'n' || toEvaluate[i] == 's' || toEvaluate[i] == 'h' || toEvaluate[i] == 'r' || toEvaluate[i] == 'd' ||
            toEvaluate[i] == 'l' || toEvaluate[i] == 'c' || toEvaluate[i] == 'u' || toEvaluate[i] == 'm' || toEvaluate[i] == 'w' ||
            toEvaluate[i] == 'f' || toEvaluate[i] == 'g' || toEvaluate[i] == 'y' || toEvaluate[i] == 'p' || toEvaluate[i] == 'b' ||
            toEvaluate[i] == 'v' || toEvaluate[i] == 'k' || toEvaluate[i] == 'j' || toEvaluate[i] == 'x' || toEvaluate[i] == 'q' || toEvaluate[i] == 'z') {
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
    private string GetArguments(string toEvaluate) {
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
    /// Returns whatever is before the first open brace.
    /// </summary>
    /// <param name="toEvaluate">The string to find the function of.</param>
    /// <returns>Returns whatever is before the first open brace.</returns>
    private string GetFunction(string toEvaluate) {
        //Everything before the first open brace.
        return toEvaluate.Split('(')[0];
    }
}
