using UnityEngine;
using System.Collections.Generic;

public class TimelineCommand {

    public enum Command { STARTTIMELINE, DIALOGUE, REPEAT, ENDREPEAT, WAIT, BULLETPROPERTY, ENEMYPROPERTY, LASERPROPERTY, CREATEBULLET, CREATEENEMY, CREATELASER,
                        MOVEPARENT, DESTROYPARENT, SETPARENTHEALTH, SETPARENTSCORE, ANGLETOPLAYER, RANDOM, SET, ADD, SUB, MUL, DIV, MOD, POW, SIN, ASIN, COS, ACOS, TAN, ATAN, ABS };
    public enum EnemyProperty { SCALE, ATTACKPATH, SPELLCARDNAME, TIME, ID, COLORISE, COLOR, MAXHEALTH, BOSS, BOSSPORTRAIT, DROPVALUE, DROPPOWER, DROPSCORE, STARTPOS };
    public enum BulletProperty { MOVEMENT, ENEMYSHOT, SCALE, ID, INNERCOLOR, OUTERCOLOR, ROTATION, POSITION, RELATIVEPOS, CLEARIMMUNE, SCRIPTROTATION, ADVANCEDPATH, HARMLESS };
    public enum LaserProperty { WARNDURATION, SHOTDURATION, OUTERCOLOR, INNERCOLOR, BULLET };

    public Command command;
    public EnemyProperty enemyProperty = EnemyProperty.SCALE;
    public BulletProperty bulletProperty = BulletProperty.MOVEMENT;
    public LaserProperty laserProperty = LaserProperty.WARNDURATION;
    public List<string> args;

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
}
