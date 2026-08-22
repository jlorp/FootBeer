using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnImpact : MonoBehaviour
{
    public float velocityThreshold = 1.0f;
    public float maxVolume;
    public float volumeMultiplier = 1;

    public AudioClip[] ImpactSounds;

    private void OnCollisionEnter(Collision collision)
    {
        float collisionMagnitude = collision.relativeVelocity.magnitude;
        if (collisionMagnitude > velocityThreshold)
        {
            collisionMagnitude *= volumeMultiplier;
            collisionMagnitude = Mathf.Clamp(collisionMagnitude,0,maxVolume);

            int sound = UnityEngine.Random.Range(0, ImpactSounds.Length);
            float pitch = UnityEngine.Random.Range(.95f,1.05f);
            float screenPositionX = CameraManager.Instance.activeCamera.WorldToViewportPoint(transform.position).x;
            screenPositionX = (screenPositionX * 2) -1;
            AudioManager.Instance.PlaySound(ImpactSounds[sound],collisionMagnitude, pitch, screenPositionX);
        }
    }
}
