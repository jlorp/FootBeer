using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource kick;
    public AudioSource lakeLoop;
    public AudioSource windloop;

    void Start()
    {
        Instance = this;
    }

    public void PlaySound(AudioClip sound, float impactMagnitude, float pitch)
    {
        kick.volume = impactMagnitude;
        kick.pitch = pitch;
        kick.PlayOneShot(sound);
    }

    public void ExitWater()
    {
        lakeLoop.volume = 0.1f;
        windloop.volume = 0.75f;
    }
}
