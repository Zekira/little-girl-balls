using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class TimelineCommand {

    private static Dictionary<int, List<TimelineCommand>> commandLists = new Dictionary<int, List<TimelineCommand>>();

    public enum Command { STARTTIMELINE, DIALOGUE, REPEAT, ENDREPEAT, IF, ELSE, ENDIF, WAIT, BULLETPROPERTY, ENEMYPROPERTY, LASERPROPERTY, CREATEBULLET, CREATEENEMY, CREATELASER,
                        MOVEPARENT, DESTROYPARENT, SETPARENTHEALTH, ANGLETOPLAYER, ANGLETOPOINT, GETPOSITION, GETPLAYERPOSITION, RANDOM,
                        SET, ADD, SUB, MUL, DIV, MOD, POW, SIN, ASIN, COS, ACOS, TAN, ATAN, ABS,
                        ATTACKDURATION };
    public enum EnemyProperty { SCALE, ATTACKPATH, ID, MAXHEALTH, BOSS, BOSSPORTRAIT, DROPVALUE, DROPPOWER, DROPSCORE, STARTPOS, BASESCORE };
    public enum BulletProperty { MOVEMENT, ENEMYSHOT, SCALE, ID, INNERCOLOR, OUTERCOLOR, ROTATION, POSITION, RELATIVEPOS, CLEARIMMUNE, SCRIPTROTATION, ADVANCEDPATH, HARMLESS };
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

    public static List<TimelineCommand> GetCommands(string path) {
        List<TimelineCommand> returnList;
        string file = (MonoBehaviour.Instantiate((TextAsset)Resources.Load(path))).text;
        int hash = file.GetHashCode();

        if (!commandLists.TryGetValue(hash, out returnList)) { //If it's already a list of commands recognised, no need to parse it again. Otherwise, parse is needed.
            file = Regex.Replace(file, @"\s+", ""); //Simple cleanup. This also makes ReadAttack totally inappropriate for dialogue.
            file = file.Replace("\n", "");
            file = file.Replace("\r", "");
            string[] instructions = file.Split(';'); //This splits instructions.
            string function;
            List<string> args;
            returnList = new List<TimelineCommand>();
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
                    returnList.Add(new TimelineCommand(
                        (BulletProperty)Enum.Parse(typeof(BulletProperty), function.ToUpperInvariant()),
                        args
                        ));
                    continue;
                } else if (function == "enemyproperty") { //Or an enemy's property
                    function = args[1];
                    args.RemoveAt(1);
                    returnList.Add(new TimelineCommand(
                        (EnemyProperty)Enum.Parse(typeof(EnemyProperty), function.ToUpperInvariant()),
                        args
                        ));
                    continue;
                } else if (function == "laserproperty") { 
                    function = args[1];
                    args.RemoveAt(1);
                    returnList.Add(new TimelineCommand(
                        (LaserProperty)Enum.Parse(typeof(LaserProperty), function.ToUpperInvariant()),
                        args
                        ));
                } else { //At this point no bullet/enemy properties are being set so it's just parsing the TimelineCommand.Command
                    returnList.Add(new TimelineCommand(
                        (Command)Enum.Parse(typeof(Command), function.ToUpperInvariant()),
                        args
                        ));
                    continue;
                }
            }
            commandLists.Add(hash, returnList);
        }
        return returnList;
    }
}
