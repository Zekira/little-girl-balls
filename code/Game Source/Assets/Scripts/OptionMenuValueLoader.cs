using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets the setting of what this is attached to to what the saved value is
/// </summary>
public class OptionMenuValueLoader : MonoBehaviour {

	// this is bad code and i should feel bad
	void OnEnable () {
        SaveLoad.LoadApplyConfig();
        switch (gameObject.name) {
            case "MusicVolumeVariable":
                Config.SetMusicVolume(transform, Config.musicVolume, false);
                break;
            case "OtherVolumeVariable":
                Config.SetOtherVolume(transform, Config.otherVolume, false);
                break;
            case "FullscreenVariable":
                Config.SetFullscreen(transform, Config.defaultFullscreen, false);
                break;
            case "KeyLeftVariable":
                Config.SetKeyLeft(transform, Config.keyLeft, false);
                break;
            case "KeyRightVariable":
                Config.SetKeyRight(transform, Config.keyRight, false);
                break;
            case "KeyUpVariable":
                Config.SetKeyUp(transform, Config.keyUp, false);
                break;
            case "KeyDownVariable":
                Config.SetKeyDown(transform, Config.keyDown, false);
                break;
            case "KeyShootVariable":
                Config.SetKeyShoot(transform, Config.keyShoot, false);
                break;
            case "KeyBombVariable":
                Config.SetKeyBomb(transform, Config.keyBomb, false);
                break;
            case "KeyFocusVariable":
                Config.SetKeyFocus(transform, Config.keyFocus, false);
                break;
            case "KeySkipVariable":
                Config.SetKeySkip(transform, Config.keySkip, false);
                break;
            case "KeyPauseVariable":
                Config.SetKeyPause(transform, Config.keyPause, false);
                break;
            case "KeyRestartVariable":
                Config.SetKeyRestart(transform, Config.keyRestart, false);
                break;
        }
	}
}
