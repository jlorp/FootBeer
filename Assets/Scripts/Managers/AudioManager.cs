using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource kick;

    void Start()
    {
        Instance = this;
    }

    public void PlaySound(AudioClip sound, float impactMagnitude)
    {
        kick.volume= impactMagnitude;
        kick.PlayOneShot(sound);
    }
}
