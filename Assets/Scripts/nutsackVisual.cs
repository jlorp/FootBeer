using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nutsackVisual : MonoBehaviour
{
    public Transform nutsackDirection;
    Vector3 baseScale;
    public float stretchFactor;
    public Vector3 nutDirection;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        nutDirection = nutsackDirection.position - transform.position;

        transform.localRotation = Quaternion.Euler(new Vector3 (0,0, nutDirection.x * -500f));

        float stretch = 1 - (nutDirection.y * stretchFactor);
        float squash = 2-stretch;
        transform.localScale = new Vector3(baseScale.x * squash, baseScale.y * stretch, baseScale.z );
    }
}
