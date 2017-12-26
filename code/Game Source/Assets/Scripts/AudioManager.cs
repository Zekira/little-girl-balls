using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public enum BGM { TITLE, STAGE1, BOSS1, STAGE2, BOSS2, STAGE3, BOSS3, STAGE4, BOSS4, STAGE5, BOSS5, STAGE6, BOSS6, STAGEEXTRA, BOSSEXTRA, CREDITS, ENDING };
    public enum SFX { ATTACK_BWZZ_HEAVY, ATTACK_BWZZ_LIGHT, ATTACK_CHARGE, ATTACK_CHARGE_LONG, ATTACK_DING, ATTACK_DING_HIGH, ATTACK_DRUM, ATTACK_DWUH, ATTACK_DWUH_HIGH, ATTACK_EXPLOSION, ATTACK_FIRE, ATTACK_FIRE_DIMMED, ATTACK_FIRE_LOTS, ATTACK_LASER, ATTACK_LASER_LONG, BONUS1, BONUS2, BONUS3, DAMAGE_HIGH, DAMAGE_LOW, EXTEND, GRAZE, HIT, ITEM, MENU_CANCEL, MENU_CHANGE_SELECTION, MENU_CHANGE_SELECTION2, MENU_INVALID, MENU_SELECT, POWERUP, SPELLCARD_BONUS, SPELLCARD_CHARGE, SPELLCARD_END, SPELLCARD_FINAL_END, TIMER, TIMER_FINAL };

    //Set in inspector
    public Transform thisTransform, bgmTransform, sfxTransform;
    public AudioClip[] music = new AudioClip[17]; //0 = main menu; 2n-1, 2n = stage n music, stage n boss music for stages 1 through 7, 15+16 = credits+ending music;
    public AudioClip[] sfx = new AudioClip[36];
    private GameObject sfxObject;
    private AudioSource bgm;

    private List<AudioSource> sfxPlaying = new List<AudioSource>();
    private static List<SFX> sfxQueued = new List<SFX>();

    public static bool enabledManager = false;

    private void OnEnable() {
        if (!enabledManager) { //TODO: Put a copy of the finalised gameobject this is attached to into the main menu as well.
            DontDestroyOnLoad(gameObject);
            GlobalHelper.audioManager = this;
            bgm = bgmTransform.GetComponent<AudioSource>();
            sfxObject = sfxTransform.gameObject;
            enabledManager = true;
        } else {
            Destroy(gameObject);
            return;
        }

        //TODO: Initialise the sfx's multiple audio sources here, one for each in the SFX enum
    }

    private void Update() {
        //Remove any sounds that have stopped playing
        for(int i = sfxPlaying.Count-1; i >= 0; i--) {
            if (!sfxPlaying[i].isPlaying) {
                Destroy(sfxPlaying[i]);
                sfxPlaying.RemoveAt(i);
            }
        }
    }

    private void LateUpdate() {
        foreach(SFX track in sfxQueued) {
            PlaySound(track);
        }
        sfxQueued.Clear();
    }

    public static void QueueSound(SFX track) {
        if (!sfxQueued.Contains(track)) {
            sfxQueued.Add(track);
        }
    }

    private void PlaySound(SFX track) {
        AudioSource audio = sfxObject.AddComponent<AudioSource>();
        audio.volume = Config.otherVolume / 20f;
        audio.clip = sfx[(int)track];
        sfxPlaying.Add(audio);
        audio.Play();
    }

    public void PlayMusic(BGM track) {
        bgm.Stop();
        bgm.volume = Config.musicVolume/20f;
        bgm.clip = music[(int)track];
        bgm.Play();
    }

    public void PauseMusic() {
        bgm.Pause();
    }

    public void UnpauseMusic() {
        bgm.UnPause();
    }

    //TODO: queue sound effects, to be executed at the same time, and prevent multiples
}
