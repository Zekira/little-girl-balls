using UnityEngine;
using System.Collections.Generic;

public class TimelineCommand {

    public enum Command { STARTTIMELINE, DIALOGUE, REPEAT, ENDREPEAT, WAIT, BULLETPROPERTY, ENEMYPROPERTY, CREATEBULLET, CREATEENEMY, MOVEPARENT, DESTROYPARENT, SETPARENTHEALTH, SETPARENTSCORE, ANGLETOPLAYER,
                        RANDOM, SET, ADD, SUB, MUL, DIV, MOD, POW, SIN, ASIN, COS, ACOS, TAN, ATAN, ABS };
    public enum EnemyProperty { SCALE, ATTACKPATH, SPELLCARDNAME, TIME, ID, COLORISE, COLOR, MAXHEALTH, BOSS, BOSSPORTRAIT, DROPVALUE, DROPPOWER, DROPSCORE, STARTPOS };
    public enum BulletProperty { MOVEMENT, ISHARMFUL, SCALE, ID, INNERCOLOR, OUTERCOLOR, ROTATION, POSITION, RELATIVEPOS, CLEARIMMUNE, SCRIPTROTATION, ADVANCEDPATH };

    public Command command;
    public EnemyProperty enemyProperty = EnemyProperty.SCALE;
    public BulletProperty bulletProperty = BulletProperty.MOVEMENT;
    public List<string> args;

    public TimelineCommand(Command cmd, List<string> arguments) {
        command = cmd;
        args = arguments;
    }

    public TimelineCommand(Command cmd, EnemyProperty property, List<string> arguments) {
        command = cmd;
        enemyProperty = property;
        args = arguments;
    }

    public TimelineCommand(Command cmd, BulletProperty property, List<string> arguments) {
        command = cmd;
        bulletProperty = property;
        args = arguments;
    }

    public TimelineCommand(TimelineCommand timelineCommand) {
        command = timelineCommand.command;
        enemyProperty = timelineCommand.enemyProperty;
        bulletProperty = timelineCommand.bulletProperty;
        args = timelineCommand.args;
    }
}
