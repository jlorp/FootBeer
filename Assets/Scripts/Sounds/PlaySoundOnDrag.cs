using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnDrag : MonoBehaviour
{
    public AudioSource dragLoop;
    public Rigidbody body;

    public float minSpeed,maxSpeed;
    public float minVolume,maxVolume;
    

    private void OnCollisionStay(Collision collision)
    {
        //float collisionMagnitude = collision.relativeVelocity.magnitude;

        float speed = body.velocity.magnitude;
        if (speed < minSpeed)
        {
            SetDragVolume(0,1);
            return;
        }

        dragLoop.panStereo = AudioManager.Instance.GetStereoPosition(transform.position);

        speed = Mathf.Clamp(speed,0,maxSpeed);
        speed = speed/maxSpeed;

        float _volume = Mathf.Lerp(minVolume,maxVolume,speed);
        SetDragVolume(_volume, 1);
    }

    private void OnCollisionExit()
    {
        SetDragVolume(0,1);
    }

    void SetDragVolume(float volume, float pitch)
    {
        dragLoop.volume = volume;
        dragLoop.pitch = pitch;
    }
}
