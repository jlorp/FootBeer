using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWiggle : MonoBehaviour
{
    public float intensity, speed;
    Vector3 startScale;
    float randomOffset;

    void Start()
    {
        startScale= transform.localScale;
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

        transform.localScale= startScale + (intensity * scrollingNoise);
    }
}
