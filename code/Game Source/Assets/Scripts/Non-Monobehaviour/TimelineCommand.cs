using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class TimelineCommand
{

    public static Dictionary<int, List<TimelineCommand>> commandLists = new Dictionary<int, List<TimelineCommand>>();
    
    public enum Command
    {
        STARTTIMELINE, LOADLEVEL, DIALOGUE, STARTMUSIC, PLAYSOUND, BOSSNAME, REPEAT, ENDREPEAT, IF, ELSE, ENDIF, WAIT, BOSSWAIT, STOP,
        BULLETPROPERTY, ENEMYPROPERTY, LASERPROPERTY, CREATEBULLET, CREATEENEMY, CREATELASER,
        MOVEPARENT, MOVEPARENTPOLAR, DESTROYPARENT, SETPARENTHEALTH, ANGLETOPLAYER, ANGLETOPOINT, GETPOSITION, GETPLAYERPOSITION, RANDOM, MOVETOWARDSPOINT,
        SET, ADD, SUB, MUL, DIV, MOD, POW, SIN, ASIN, COS, ACOS, TAN, ATAN, ABS,
        ATTACKDURATION, LOG, PHASE, TOPHASE
    };
    public enum EnemyProperty { SCALE, ATTACKPATH, ID, MAXHEALTH, BOSS, BOSSPORTRAIT, DROPVALUE, DROPPOWER, DROPSCORE, STARTPOS, BASESCORE };
    public enum BulletProperty { MOVEMENT, MOVEMENTPOLAR, ENEMYSHOT, SCALE, ID, INNERCOLOR, OUTERCOLOR, ROTATION, ROTATIONSPEED, POSITION, RELATIVEPOS, CLEARIMMUNE, SCRIPTROTATION, ADVANCEDPATH, HARMLESS, SNAKELENGTH };
    public enum LaserProperty { WARNDURATION, SHOTDURATION, OUTERCOLOR, INNERCOLOR, WIDTH, MOVEMENT, POSITION, RELATIVEPOS, ROTATION, ROTATIONSPEED };

    public Command command;
    public EnemyProperty enemyProperty = EnemyProperty.SCALE;
    public BulletProperty bulletProperty = BulletProperty.MOVEMENT;
    public LaserProperty laserProperty = LaserProperty.WARNDURATION;
    public List<string> args;
    //Stored in the first entry of the commandlist.
    public int numberVarCount, bulletVarCount, enemyVarCount, laserVarCount;

    //The string represents what would be found in the text, while its index determines what it's replaced with
    private static List<string> numberVars, bulletTemplateVars, enemyTemplateVars, laserTemplateVars;

    public TimelineCommand(Command cmd, List<string> arguments) {
        command = cmd;
        args = arguments;
    }

    public TimelineCommand(EnemyProperty property, List<string> arguments) {
        command = Command.ENEMYPROPERTY;
        enemyProperty = property;
        args = arguments;
    }

    public TimelineCommand(BulletProperty property, List<string> arguments) {
        command = Command.BULLETPROPERTY;
        bulletProperty = property;
        args = arguments;
    }

    public TimelineCommand(LaserProperty property, List<string> arguments) {
        command = Command.LASERPROPERTY;
        laserProperty = property;
        args = arguments;
    }

    public TimelineCommand(TimelineCommand timelineCommand) {
        command = timelineCommand.command;
        enemyProperty = timelineCommand.enemyProperty;
        bulletProperty = timelineCommand.bulletProperty;
        laserProperty = timelineCommand.laserProperty;
        args = timelineCommand.args;
    }

    public static int GetCommands(string path) {
        int hash = path.GetHashCode(); //Paths must be unique so this's usually unique

        if (!commandLists.ContainsKey(hash)) { //If it's already a list of commands recognised, no need to parse it again. Otherwise, parse is needed.
            string file = SaveLoad.TryReadTextFile(path);
            if (file == null) {
                file = (MonoBehaviour.Instantiate((TextAsset)Resources.Load(path))).text;
            }
            file = Regex.Replace(file, @"\s+", ""); //Simple cleanup. This also makes ReadAttack totally inappropriate for dialogue.
            file = file.Replace("\n", "");
            file = file.Replace("\r", "");
            string[] instructions = file.Split(';'); //This splits instructions.
            string function;
            List<string> args;
            List<TimelineCommand> newList = new List<TimelineCommand>();
#if UNITY_EDITOR //Logging invalid functions and preventing them from being used to prevent major console error spam
            bool foundError = false;
            for (int index = 0; index < instructions.Length; index++) {
                string instruction;
                instruction = instructions[index];
                if (instruction.Length == 0 || instruction[0] == '/' || instruction[0] == '#') { //exclude comments
                    continue;
                }
                function = TimelineInterprenter.GetFunction(instruction).ToLowerInvariant();
                args = TimelineInterprenter.GetArguments(instruction);
                switch (function) {
                    case "endrepeat":
                    case "else":
                    case "endif":
                    case "destroyparent":
                    case "bosswait":
                    case "stop":
                        if (args.Count != 0) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected none.");
                            foundError = true;
                        }
                        break;
                    case "starttimeline":
                        if (args.Count != 1) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 1.");
                            foundError = true;
                        } else if (Resources.Load(args[0]) == null) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): file \"<i>" + args[0] + "</i>\" does not exist.");
                            foundError = true;
                        }
                        break;
                    case "dialogue":
                        if (args.Count != 1) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 1.");
                            foundError = true;
                        } else if (Resources.Load(args[0] + "_" + GlobalHelper.GetCharacterType(GlobalHelper.character)) == null) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): file \"<i>" + args[0] + "_" + GlobalHelper.GetCharacterType(GlobalHelper.character) + "</i>\" does not exist.");
                            foundError = true;
                        }
                        break;
                    case "startmusic":
                        if (args.Count != 1) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 1.");
                            foundError = true;
                        } else if (!Enum.IsDefined(typeof(AudioManager.BGM), args[0].ToUpperInvariant())) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): \"<i>" + args[0] + "</i>\" is not a valid music track.");
                            foundError = true;
                        }
                        break;
                    case "playsound":
                        if (args.Count != 1) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 1.");
                            foundError = true;
                        } else if (!Enum.IsDefined(typeof(AudioManager.SFX), args[0].ToUpperInvariant())) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): \"<i>" + args[0] + "</i>\" is not valid sfx.");
                            foundError = true;
                        }
                        break;
                    case "loadlevel":
                    case "repeat":
                    case "wait":
                    case "createbullet":
                    case "createenemy":
                    case "createlaser":
                    case "setparenthealth":
                    case "angletoplayer":
                    case "attackduration":
                    case "bossname":
                    case "phase":
                        if (args.Count != 1) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 1.");
                            foundError = true;
                        }
                        break;
                    case "getposition":
                    case "getplayerposition":
                    case "set":
                    case "sin":
                    case "asin":
                    case "cos":
                    case "acos":
                    case "tan":
                    case "atan":
                    case "abs":
                    case "log":
                        if (args.Count != 2) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 2.");
                            foundError = true;
                        }
                        break;
                    case "moveparent":
                    case "moveparentpolar":
                        if (args.Count != 2 && args.Count != 3) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 2 or 3.");
                            foundError = true;
                        }
                        break;
                    case "if":
                    case "random":
                    case "angletopoint":
                    case "movetowardspoint":
                    case "add":
                    case "sub":
                    case "mul":
                    case "div":
                    case "mod":
                    case "pow":
                        if (args.Count != 3) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 3.");
                            foundError = true;
                        }
                        break;
                    case "tophase":
                        if (args.Count != 4) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 4.");
                            foundError = true;
                        }
                        break;
                    case "bulletproperty":
                        if (args.Count < 3) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): No property values specified");
                            foundError = true;
                            break;
                        }
                        switch (args[1]) {
                            case "enemyshot":
                            case "scale":
                            case "id":
                            case "relativepos":
                            case "clearimmune":
                            case "scriptrotation":
                            case "harmless":
                            case "snakelength":
                            case "rotationspeed":
                                if (args.Count != 3) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 1.");
                                    foundError = true;
                                }
                                break;
                            case "rotation":
                                if (args.Count != 3 && args.Count != 4) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 1 or 2.");
                                    foundError = true;
                                }
                                break;
                            case "position":
                            case "movement":
                            case "movementpolar":
                                if (args.Count != 4 && args.Count != 5) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 2 or 3.");
                                    foundError = true;
                                }
                                break;
                            case "outercolor":
                            case "innercolor":
                                if (args.Count != 6) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 4.");
                                    foundError = true;
                                }
                                break;
                            case "advancedpath":
                                if (Resources.Load(args[2]) == null) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): file \"<i>" + args[2] + "</i>\" does not exist.");
                                    foundError = true;
                                }
                                break;
                            default:
                                Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): Unknown Bullet Property \"<i>" + args[1] + "</i>\"");
                                foundError = true;
                                break;
                        }
                        break;
                    case "enemyproperty":
                        if (args.Count < 3) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): No property values specified");
                            foundError = true;
                            break;
                        }
                        switch (args[1]) {
                            case "scale":
                            case "id":
                            case "maxhealth":
                            case "boss":
                            case "bossportrait":
                            case "dropvalue":
                            case "droppower":
                            case "dropscore":
                            case "basescore":
                                if (args.Count != 3) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 1.");
                                    foundError = true;
                                }
                                break;
                            case "startpos":
                                if (args.Count != 4) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 2.");
                                    foundError = true;
                                }
                                break;
                            case "attackpath": //Any number of values >0 is allowed, so no checking that
                                for (int i = 2; i < args.Count; i++) {
                                    if (Resources.Load(args[i]) == null) {
                                        Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): file \"<i>" + args[i] + "</i>\" does not exist.");
                                        foundError = true;
                                    }
                                }
                                break;
                            default:
                                Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): Unknown Enemy Property \"<i>" + args[1] + "</i>\"");
                                foundError = true;
                                break;
                        }
                        break;
                    case "laserproperty":
                        if (args.Count < 3) {
                            Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): No property values specified");
                            foundError = true;
                            break;
                        }
                        switch (args[1]) {
                            case "warnduration":
                            case "shotduration":
                            case "width":
                            case "relativepos":
                            case "rotationspeed":
                                if (args.Count != 3) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 1.");
                                    foundError = true;
                                }
                                break;
                            case "rotation":
                                if (args.Count != 3 && args.Count != 4) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 1 or 2.");
                                    foundError = true;
                                }
                                break;
                            case "movement":
                                if (args.Count != 4) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 2.");
                                    foundError = true;
                                }
                                break;
                            case "position":
                                if (args.Count != 4 && args.Count != 5) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 2 or 3.");
                                    foundError = true;
                                }
                                break;
                            case "outercolor":
                            case "innercolor":
                                if (args.Count != 6) {
                                    Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 4.");
                                    foundError = true;
                                }
                                break;
                            default:
                                Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): Unknown Laser Property \"<i>" + args[1] + "</i>\"");
                                foundError = true;
                                break;
                        }
                        break;
                    default:
                        Debug.LogError("[Error] Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): Unknown function \"<i>" + function + "</i>\"");
                        foundError = true;
                        break;
                }
            }
            if (foundError) {
                commandLists.Add(hash, new List<TimelineCommand>());
                return hash;
            }
#endif //UNITY_EDITOR
            numberVars = new List<string>();
            bulletTemplateVars = new List<string>();
            enemyTemplateVars = new List<string>();
            laserTemplateVars = new List<string>();
            foreach (string instruction in instructions) { //Turn the file into a list of commands
                //If the first thing is a comment, skip it.
                if (instruction.Length == 0 || instruction[0] == '/' || instruction[0] == '#') {
                    continue;
                }
                function = TimelineInterprenter.GetFunction(instruction).ToLowerInvariant();
                args = TimelineInterprenter.GetArguments(instruction);
                for (int i = 0; i < args.Count; i++) {
                    //Global Vars don't have the luxery of a list, they live in dictionaries and don't need all this effort.
                    if (args[i].Length > 0 && args[i][0] == '_') {
                        continue;
                    }
                    //All situations in which the name isn't meant to be ANY variable
                    if ((function == "if" && i == 1) ||
                        (function == "starttimeline") ||
                        (function == "dialogue") ||
                        (function == "bossname") ||
                        (function == "startmusic") ||
                        (function == "playsound") ||
                        (function == "bulletproperty" && i == 1) ||
                        (function == "bulletproperty" && args[1] == "advancedpath" && i == 2) ||
                        (function == "enemyproperty" && i == 1) ||
                        (function == "enemyproperty" && args[1] == "attackpath" && i >= 2) ||
                        (function == "enemyproperty" && args[1] == "bossportrait" && i == 2) ||
                        (function == "laserproperty" && i == 1) ||
                        (function == "phase") ||
                        (function == "tophase" && i == 0) ||
                        (function == "log" && i == 0)) {
                        continue;
                    }
                    //Sort out the enemyTemplate, bulletTemplate, laserTemplate id's
                    if ((function == "bulletproperty" && i == 0) ||
                        (function == "createbullet")) {
                        args[i] = GetBullet(args[i]);
                        continue;
                    } else if ((function == "enemyproperty" && i == 0) ||
                               (function == "createenemy")) {
                        args[i] = GetEnemy(args[i]);
                        continue;
                    } else if ((function == "laserproperty" && i == 0) ||
                               (function == "createlaser")) {
                        args[i] = GetLaser(args[i]);
                        continue;
                    }
                    //At this point it's a number variable, so it needs a unique number list identifier
                    args[i] = GetNumber(args[i]);
                }
                if (function == "bulletproperty") { //If this command sets a bullet property...
                    function = args[1];
                    args.RemoveAt(1);
                    newList.Add(new TimelineCommand(
                        (BulletProperty)Enum.Parse(typeof(BulletProperty), function.ToUpperInvariant()),
                        args
                        ));
                    continue;
                } else if (function == "enemyproperty") { //Or an enemy's property
                    function = args[1];
                    args.RemoveAt(1);
                    newList.Add(new TimelineCommand(
                        (EnemyProperty)Enum.Parse(typeof(EnemyProperty), function.ToUpperInvariant()),
                        args
                        ));
                    continue;
                } else if (function == "laserproperty") {
                    function = args[1];
                    args.RemoveAt(1);
                    newList.Add(new TimelineCommand(
                        (LaserProperty)Enum.Parse(typeof(LaserProperty), function.ToUpperInvariant()),
                        args
                        ));
                } else { //At this point no bullet/enemy properties are being set so it's just parsing the TimelineCommand.Command
                    newList.Add(new TimelineCommand(
                        (Command)Enum.Parse(typeof(Command), function.ToUpperInvariant()),
                        args
                        ));
                    continue;
                }
            }
            newList[0].numberVarCount = numberVars.Count;
            newList[0].bulletVarCount = bulletTemplateVars.Count;
            newList[0].enemyVarCount = enemyTemplateVars.Count;
            newList[0].laserVarCount = laserTemplateVars.Count;
            commandLists.Add(hash, newList);
            return hash;
        } else {
            return hash;
        }
    }

    private static string GetNumber(string textName) {
        float x = 0; //dummy variable
        if (float.TryParse(textName, out x)) { //It's a number
            return "n" + textName;
        } else { //It's a reference
            for (int i = 0; i < numberVars.Count; i++) {
                if (numberVars[i] == textName) {
                    return "" + i; //The correct index is found
                }
            }
            numberVars.Add(textName);
            return "" + (numberVars.Count-1);
        }
    }

    private static string GetBullet(string textName) {
        for (int i = 0; i < bulletTemplateVars.Count; i++) {
            if (bulletTemplateVars[i] == textName) {
                return "" + i;
            }
        }
        bulletTemplateVars.Add(textName);
        return "" + (bulletTemplateVars.Count-1);
    }

    private static string GetEnemy(string textName) {
        for (int i = 0; i < enemyTemplateVars.Count; i++) {
            if (enemyTemplateVars[i] == textName) {
                return "" + i;
            }
        }
        enemyTemplateVars.Add(textName);
        return "" + (enemyTemplateVars.Count-1);
    }

    private static string GetLaser(string textName) {
        for (int i = 0; i < laserTemplateVars.Count; i++) {
            if (laserTemplateVars[i] == textName) {
                return "" + i;
            }
        }
        laserTemplateVars.Add(textName);
        return "" + (laserTemplateVars.Count-1);
    }
}