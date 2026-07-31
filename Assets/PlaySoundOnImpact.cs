using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnImpact : MonoBehaviour
{
    public float velocityThreshold = 1.0f;
    public AudioClip[] ImpactSounds;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.relativeVelocity.magnitude);
        if (collision.relativeVelocity.magnitude > velocityThreshold)
        {
            int sound = UnityEngine.Random.Range(0, ImpactSounds.Length);
            AudioManager.Instance.PlaySound(ImpactSounds[sound],collision.relativeVelocity.magnitude);
        }
    }
}
