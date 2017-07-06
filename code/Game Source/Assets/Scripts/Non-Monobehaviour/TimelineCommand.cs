using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class TimelineCommand {

    public static Dictionary<int, List<TimelineCommand>> commandLists = new Dictionary<int, List<TimelineCommand>>();
    //private static Dictionary<int, int> commandListsIds = new Dictionary<int, int>();

    public enum Command { STARTTIMELINE, DIALOGUE, REPEAT, ENDREPEAT, IF, ELSE, ENDIF, WAIT, BOSSWAIT, BULLETPROPERTY, ENEMYPROPERTY, LASERPROPERTY, CREATEBULLET, CREATEENEMY, CREATELASER,
                        MOVEPARENT, DESTROYPARENT, SETPARENTHEALTH, ANGLETOPLAYER, ANGLETOPOINT, GETPOSITION, GETPLAYERPOSITION, RANDOM, MOVETOWARDSPOINT,
                        SET, ADD, SUB, MUL, DIV, MOD, POW, SIN, ASIN, COS, ACOS, TAN, ATAN, ABS,
                        ATTACKDURATION };
    public enum EnemyProperty { SCALE, ATTACKPATH, ID, MAXHEALTH, BOSS, BOSSPORTRAIT, DROPVALUE, DROPPOWER, DROPSCORE, STARTPOS, BASESCORE };
    public enum BulletProperty { MOVEMENT, MOVEMENTPOLAR, ENEMYSHOT, SCALE, ID, INNERCOLOR, OUTERCOLOR, ROTATION, POSITION, RELATIVEPOS, CLEARIMMUNE, SCRIPTROTATION, ADVANCEDPATH, HARMLESS };
    public enum LaserProperty { WARNDURATION, SHOTDURATION, OUTERCOLOR, INNERCOLOR, WIDTH, MOVEMENT, POSITION, RELATIVEPOS, ROTATION, ROTATIONSPEED };

    public Command command;
    public EnemyProperty enemyProperty = EnemyProperty.SCALE;
    public BulletProperty bulletProperty = BulletProperty.MOVEMENT;
    public LaserProperty laserProperty = LaserProperty.WARNDURATION;
    public List<string> args;

    //private static Regex whitespaceRegex = new Regex(@"\s+");

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
        //int returnInt;
        int hash = path.GetHashCode(); //Paths must be unique so this's usually unique

        if (!commandLists.ContainsKey(hash)) { //If it's already a list of commands recognised, no need to parse it again. Otherwise, parse is needed.
            string file = (MonoBehaviour.Instantiate((TextAsset)Resources.Load(path))).text;
            file = Regex.Replace(file, @"\s+", ""); //Simple cleanup. This also makes ReadAttack totally inappropriate for dialogue.
            file = file.Replace("\n", "");
            file = file.Replace("\r", "");
            string[] instructions = file.Split(';'); //This splits instructions.
            string function;
            List<string> args;
            List<TimelineCommand> newList = new List<TimelineCommand>();
#if UNITY_EDITOR //Logging invalid functions and preventing them from being used to prevent major console error spam //TODO: Add check if repeat/endrepeat and if/else/endif pairs match properly
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
                        if (args.Count != 0) {
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected none.");
                            foundError = true;
                        }
                        break;
                    case "starttimeline":
                    case "dialogue":
                        if (args.Count != 1) {
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 1.");
                            foundError = true;
                        } else if (Resources.Load(args[0]) == null) {
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): file \"<i>" + args[0] + "</i>\" does not exist.");
                            foundError = true;
                        }
                        break;
                    case "repeat":
                    case "wait":
                    case "createbullet":
                    case "createenemy":
                    case "createlaser":
                    case "setparenthealth":
                    case "angletoplayer":
                    case "attackduration":
                        if (args.Count != 1) {
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 1.");
                            foundError = true;
                        }
                        break;
                    case "moveparent":
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
                        if (args.Count != 2) {
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 2.");
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
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + args.Count + " args, expected 3.");
                            foundError = true;
                        }
                        break;
                    case "bulletproperty":
                        if (args.Count < 3) {
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): No property values specified");
                            foundError = true;
                            break;
                        }
                        switch (args[1]) {
                            case "enemyshot":
                            case "scale":
                            case "id":
                            case "rotation":
                            case "relativepos":
                            case "clearimmune":
                            case "scriptrotation":
                            case "harmless":
                                if (args.Count != 3) {
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 1.");
                                    foundError = true;
                                }
                                break;
                            case "position":
                            case "movement":
                            case "movementpolar":
                                if (args.Count != 4) {
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 2.");
                                    foundError = true;
                                }
                                break;
                            case "outercolor":
                            case "innercolor":
                                if (args.Count != 6) {
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 4.");
                                    foundError = true;
                                }
                                break;
                            case "advancedpath":
                                if (Resources.Load(args[2]) == null) {
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): file \"<i>" + args[2] + "</i>\" does not exist.");
                                    foundError = true;
                                }
                                break;
                            default:
                                Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): Unknown Enemy Property \"<i>" + args[1] + "</i>\"");
                                foundError = true;
                                break;
                        }
                        break;
                    case "enemyproperty":
                        if (args.Count < 3) {
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): No property values specified");
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
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 1.");
                                    foundError = true;
                                }
                                break;
                            case "startpos":
                                if (args.Count != 4) {
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 2.");
                                    foundError = true;
                                }
                                break;
                            case "attackpath": //Any number of values >0 is allowed, so no checking that
                                for (int i = 2; i < args.Count; i++) {
                                    if (Resources.Load(args[i]) == null) {
                                        Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): file \"<i>" + args[i] + "</i>\" does not exist.");
                                        foundError = true;
                                    }
                                }
                                break;
                            default:
                                Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): Unknown Enemy Property \"<i>" + args[1] + "</i>\"");
                                foundError = true;
                                break;
                        }
                        break;
                    case "laserproperty":
                        if (args.Count < 3) {
                            Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): No property values specified");
                            foundError = true;
                            break;
                        }
                        switch (args[1]) {
                            case "warnduration":
                            case "shotduration":
                            case "width":
                            case "relativepos":
                            case "rotation":
                            case "rotationspeed":
                                if (args.Count != 3) {
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 1.");
                                    foundError = true;
                                }
                                break;
                            case "position":
                            case "movement":
                                if (args.Count != 4) {
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 2.");
                                    foundError = true;
                                }
                                break;
                            case "outercolor":
                            case "innercolor":
                                if (args.Count != 6) {
                                    Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): " + (args.Count - 2) + " property values, \"<i>" + args[1] + "</i>\" expects 4.");
                                    foundError = true;
                                }
                                break;
                            default:
                                Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): Unknown Enemy Property \"<i>" + args[1] + "</i>\"");
                                foundError = true;
                                break;
                        }
                        break;
                    default:
                        Debug.LogError("Error in Timeline \"<i>" + path + "</i>\" with instruction \"<i>" + instruction + "</i>\" (instruction " + index + "): Unknown function \"<i>" + function + "</i>\"");
                        foundError = true;
                        break;
                }
            }
            if (foundError) {
                commandLists.Add(hash, new List<TimelineCommand>());
                //commandListsIds.Add(hash, commandLists.Count - 1);
                return hash;
            }
#endif
            foreach (string instruction in instructions) { //Turn the file into a list of commands
                                                           //If the first thing is a comment, skip it.
                if (instruction.Length == 0 || instruction[0] == '/' || instruction[0] == '#') {
                    continue;
                }
                function = TimelineInterprenter.GetFunction(instruction).ToLowerInvariant();
                args = TimelineInterprenter.GetArguments(instruction);
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
            commandLists.Add(hash, newList);
            //commandListsIds.Add(hash, commandLists.Count - 1);
            return hash;
        } else {
            return hash;
        }
    }
}
