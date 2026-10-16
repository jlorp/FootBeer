using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNuts : MonoBehaviour
{
    [Header("Nut Poke")]
    public float painThreshould;
    public float pokeMultiplier;

    [Header("Position Spring")]
    public float drag;
    public float springStrength;

    public Transform restingPosition, beerTransform;
    public Vector3 currentVelocity;

    public float nutsackIframeTime;
    bool iframes = false;

    public Rigidbody crotchRigidBody, rFootBody, lFootBody;

    void Start()
    {
        currentVelocity = Vector3.zero;
        transform.position = restingPosition.position;
    }

    IEnumerator startIFrames()
    {
        iframes = true;
        yield return new WaitForSeconds(nutsackIframeTime); 
        iframes = false;
    }

    void Update()
    {
        ApplyPositionSpring(springStrength);
        ApplyDrag(drag);

        transform.localPosition += currentVelocity * Time.deltaTime;
    }

    Vector3 GetCurrentDisplacement()
    {
        return transform.localPosition - restingPosition.position;
    }

    void ApplyPositionSpring(float _springStrength)
    {
        currentVelocity = currentVelocity - GetCurrentDisplacement() * _springStrength * Time.deltaTime;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        HandleNutCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleNutCollision(collision);
    }

    void HandleNutCollision(Collision collision)
    {
        if(iframes) return;
        Vector3 sackTapVelocity = collision.relativeVelocity;

        if(sackTapVelocity.magnitude > painThreshould)
        {
            PokeNuts(sackTapVelocity);
        }

        if(collision.gameObject.TryGetComponent<Beer>(out Beer _beer))
        {
            ApplyBeerHitback(_beer);
        }
    }

    void ApplyBeerHitback(Beer _beer)
    {
        Vector3 beerToNutsDirection =(_beer.transform.position - transform.position).normalized;
        if (!_beer.OnGround)
        {
            beerToNutsDirection.y *= .25f;
        _beer.body.velocity = beerToNutsDirection * 0.5f;
        _beer.body.angularVelocity = beerToNutsDirection.x * 5f * Vector3.forward;
        }

        //messy but also applying crotch hitback here
        crotchRigidBody.velocity += Vector3.up * 0.2f;
    }

    void PokeNuts(Vector3 _impactVelocity)
    {
        StartCoroutine(startIFrames());
        currentVelocity += _impactVelocity * pokeMultiplier;

        NutRepulse(rFootBody, _impactVelocity, true);
        NutRepulse(lFootBody, _impactVelocity, false);
    }

    void NutRepulse(Rigidbody _body, Vector3 force, bool rightFoot)
    {
        force = force.normalized;
        Vector3 directionToNuts = (transform.position - _body.transform.position).normalized;
        //Vector3 directionToBeer = (beerTransform.position - _body.transform.position).normalized;

        Vector3 repulseDirection = (-Vector3.up * directionToNuts.y) + (Vector3.right * 1.5f);
        if(!rightFoot) repulseDirection.x *= -1;

        float distanceToNut = Vector3.Distance(transform.position, _body.transform.position);
        float minForceDistance = 0.1f;
        float maxForceDistance = 0.3f;
        distanceToNut = Mathf.Clamp(distanceToNut, minForceDistance, maxForceDistance);
        distanceToNut = (distanceToNut - minForceDistance) * (1 / (maxForceDistance - minForceDistance));

        float nutForceAmount = 1-distanceToNut;

        _body.velocity = repulseDirection * force.magnitude * nutForceAmount;
    }

    void ApplyDrag(float _drag)
    {
        currentVelocity *= 1f - _drag * Time.deltaTime;
    }
}
