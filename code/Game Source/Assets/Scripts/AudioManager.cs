using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    //Set in inspector
    public Transform thisTransform;
    public AudioClip[] music = new AudioClip[16]; //0 = main menu; 2n-1, 2n = stage n music, stage n boss music for stages 1 through 7, 15 = credits music;

    //TODO: EVERYTHING
}
