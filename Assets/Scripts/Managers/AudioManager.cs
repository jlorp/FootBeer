using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource kick;
    public AudioSource lakeLoop;
    public AudioSource windloop;

    public AudioClip splashSound;

    void Start()
    {
        Instance = this;
    }

    public void PlaySplash()
    {
        kick.volume = 0.5f;
        kick.panStereo = -0.5f;
        kick.PlayOneShot(splashSound);
    }

    public void PlaySound(AudioClip sound, float impactMagnitude, float pitch, float stereoPosition)
    {
        kick.volume = impactMagnitude;
        kick.pitch = pitch;
        kick.panStereo= stereoPosition;
        kick.PlayOneShot(sound);
    }

    public void ExitWater()
    {
        lakeLoop.volume = 0.1f;
        windloop.volume = 0.5f;
    }
}
