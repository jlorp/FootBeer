using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustEffect : MonoBehaviour
{
    public Transform beerCan;
    public GroundedCheck groundchecker;
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        var emission = ps.emission;
        emission.rateOverDistanceMultiplier = 0;
    }

    void Update()
    {
        Vector3 _position = transform.position;
        _position.x = beerCan.position.x;
        transform.position = _position;
    }

    void FixedUpdate()
    {
        var emission = ps.emission;

        if(!groundchecker.OnGround)
        {
            emission.rateOverDistanceMultiplier = 0;
        }

        if(emission.rateOverDistanceMultiplier == 0 && groundchecker.OnGround) emission.rateOverDistanceMultiplier = 20;
    }
}
