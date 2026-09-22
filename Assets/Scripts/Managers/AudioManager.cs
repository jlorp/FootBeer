using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource kick;
    public AudioSource bubble;
    public AudioSource wooshSource;

    public AudioSource dialogue;
    public AudioSource lakeLoop;
    public AudioSource windloop;
    public AudioSource metalCreakLoop;
    public AudioSource armMoveLoop;

    public AudioClip fuck;
    public AudioClip omg;

    public AudioClip splashSound;
    public AudioClip canOpenSound;
    public AudioClip canOpenSound2;
    public AudioClip armFireSound;
    public AudioClip armEnterWater;
    public AudioClip waterSwish;
    public AudioClip[] bubblePopSounds;
    public AudioClip[] canKickSounds;
    public AudioClip[] canGrabSounds;
    public AudioClip[] tabTouchSounds;

    void Start()
    {
        Instance = this;
    }

    public void SetCreakVolume(float volume, float pitch)
    {
        metalCreakLoop.volume = volume;
        metalCreakLoop.pitch = pitch;
    }

    public void SetLoopVolume(AudioSource _loop, float _volume, float _pitch)
    {
        _loop.volume = _volume;
        _loop.pitch = _pitch;
    }

    public void PlaySplash()
    {
        kick.volume = 0.5f;
        kick.panStereo = -0.5f;
        kick.PlayOneShot(splashSound);
    }

    public void PlaySoundAfterDelay(AudioClip _sound, float _volume, float _pitch, Vector3 _worldPosition, float delay)
    {
        StartCoroutine(SoundAfterDelay(_sound,_volume,_pitch, _worldPosition, delay));
    }

    IEnumerator SoundAfterDelay(AudioClip _sound, float _volume, float _pitch, Vector3 _worldPosition, float delay)
    {
        yield return new WaitForSeconds(delay);

        PlaySound(_sound,_volume,_pitch, _worldPosition, 1);
    }

    public void PlaySound(AudioClip _sound, float _volume, float _pitch, Vector3 _worldPosition, int _source = 2)
    {
        AudioSource _sourceA = kick;
        if (_source == 1) _sourceA = wooshSource;

        _sourceA.volume = _volume;
        _sourceA.pitch = _pitch;
        _sourceA.panStereo= GetStereoPosition(_worldPosition);
        _sourceA.PlayOneShot(_sound);
    }

    public void PlayDialogue(AudioClip _sound, float _volume, Vector3 _worldPosition)
    {
        dialogue.volume = _volume;
        dialogue.panStereo= GetStereoPosition(_worldPosition);
        dialogue.PlayOneShot(_sound);
    }

    public void PlaySound(AudioClip[] _soundArray, float _volume, float _pitch, Vector3 _worldPosition)
    {
        kick.volume = _volume;
        kick.pitch = _pitch;
        kick.panStereo= GetStereoPosition(_worldPosition);
        int sound = UnityEngine.Random.Range(0, _soundArray.Length);
        kick.PlayOneShot(_soundArray[sound]);
    }

    public void PlayBubble(AudioClip[] _soundArray, float _volume, float _pitch, Vector3 _worldPosition)
    {
        bubble.volume = _volume;
        bubble.pitch = _pitch;
        bubble.panStereo= GetStereoPosition(_worldPosition);
        int sound = UnityEngine.Random.Range(0, _soundArray.Length);
        bubble.PlayOneShot(_soundArray[sound]);
    }

    public float GetStereoPosition(Vector3 _worldPosition)
    {
        float screenPositionX = CameraManager.Instance.activeCamera.WorldToViewportPoint(_worldPosition).x;
        screenPositionX = (screenPositionX * 2) -1;
        return screenPositionX;
    }

    public void ExitWater()
    {
        lakeLoop.volume = 0.25f;
        windloop.volume = 0.4f;
        windloop.pitch = 1f;
    }

    public void EnterWater()
    {
        lakeLoop.volume = 0.75f;
        windloop.volume = 0.25f;
        windloop.pitch = 0.8f;
    }
}
