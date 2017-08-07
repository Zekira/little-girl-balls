using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public enum BGM { TITLE, STAGE1, BOSS1, STAGE2, BOSS2, STAGE3, BOSS3, STAGE4, BOSS4, STAGE5, BOSS5, STAGE6, BOSS6, STAGEEXTRA, BOSSEXTRA, CREDITS, ENDING };
    public enum SFX { };

    //Set in inspector
    public Transform thisTransform, bgmTransform, sfxTransform;
    public AudioClip[] music = new AudioClip[17]; //0 = main menu; 2n-1, 2n = stage n music, stage n boss music for stages 1 through 7, 15+16 = credits+ending music;
    public AudioSource bgm;
    public AudioSource sfx;

    private void Awake() {
        bgm = bgmTransform.GetComponent<AudioSource>();
        sfx = sfxTransform.GetComponent<AudioSource>();
        //TODO: Initialise the sfx's multiple audio sources here, one for each in the SFX enum
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
