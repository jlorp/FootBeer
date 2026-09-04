using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnImpact : MonoBehaviour
{
    public float velocityThreshold = 1.0f;
    public float maxVolume;
    public float minVolume;
    public float volumeMultiplier = 1;

    public float maxVolumeSand, minVolumeSand;

    public float minTimeBetweenSounds;

    public PhysicMaterial sandMaterial;
    

    public AudioClip[] ImpactSounds;

    public AudioClip[] SandImpactSounds;

    float timeSinceSound;

    void Start()
    {
        timeSinceSound = minTimeBetweenSounds;
    }
    void Update()
    {
        timeSinceSound += Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(timeSinceSound < minTimeBetweenSounds) return;

        float collisionMagnitude = collision.relativeVelocity.magnitude;
        if (collisionMagnitude > velocityThreshold)
        {
            collisionMagnitude *= volumeMultiplier;


            float pitch = UnityEngine.Random.Range(.95f,1.05f);

            if (collision.collider.sharedMaterial == sandMaterial)
            {
                collisionMagnitude = Mathf.Clamp(collisionMagnitude,minVolumeSand,maxVolumeSand);
                AudioManager.Instance.PlaySound(SandImpactSounds,collisionMagnitude * 2, pitch, transform.position);
            }
            else if(ImpactSounds.Length > 0)
            {
                collisionMagnitude = Mathf.Clamp(collisionMagnitude,minVolume,maxVolume);
                AudioManager.Instance.PlaySound(ImpactSounds,collisionMagnitude, pitch, transform.position);
            }

            timeSinceSound = 0;
        }
    }
}
