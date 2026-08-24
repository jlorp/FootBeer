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
    public AudioClip canOpenSound;

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

    public void PlaySound(AudioClip _sound, float _volume, float _pitch, Vector3 _worldPosition)
    {
        kick.volume = _volume;
        kick.pitch = _pitch;
        kick.panStereo= GetStereoPosition(_worldPosition);
        kick.PlayOneShot(_sound);
    }

    float GetStereoPosition(Vector3 _worldPosition)
    {
        float screenPositionX = CameraManager.Instance.activeCamera.WorldToViewportPoint(_worldPosition).x;
        screenPositionX = (screenPositionX * 2) -1;
        return screenPositionX;
    }

    public void ExitWater()
    {
        lakeLoop.volume = 0.1f;
        windloop.volume = 0.5f;
    }
}
