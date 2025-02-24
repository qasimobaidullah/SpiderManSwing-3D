using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource WebAudioSource;
    public AudioSource RunningAudioSource;
    public AudioSource ClimbingAudioSource;
    public AudioSource FootAudioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}
