using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationWiggle : MonoBehaviour
{
    public float intensity, speed;
    Quaternion startRotation;
    float randomOffset;

    void Start()
    {
        startRotation= transform.localRotation;
        randomOffset = (transform.position.x + transform.position.y) * 10f;
    }

    void Update()
    {
        Wiggle();
    }


    void Wiggle()
    {
        Vector3 scrollingNoise = new Vector3(
            Mathf.PerlinNoise(speed * Time.time, 1 + randomOffset),
            Mathf.PerlinNoise(speed * Time.time, 2 + randomOffset),
            Mathf.PerlinNoise(speed * Time.time, 3 + randomOffset));

        transform.localRotation = startRotation * Quaternion.Euler(scrollingNoise*intensity);
    }
}
