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

    [SerializeField] private ParticleSystem impactPFX;

    [Header("Spine Animation")]
    public Transform spineTransform;
    Quaternion restingRotationSpine;
    public AnimationCurve SpineAnimationCurve;
    public Vector3 spineRecoilPose;
    public HeadAnimation headAnimation;
    public float minTimeBetweenComments; 
    int commentsMade = 0;
    float timeSinceLastComment;
    private Coroutine spineAnimRoutine;

    [Header("Dependencies")]
    public FeetMovement _player;
    public Rigidbody crotchRigidBody, rFootBody, lFootBody;
    public AnimationCurve inputresponseCurve;

    void Start()
    {
        currentVelocity = Vector3.zero;
        transform.position = restingPosition.position;
        restingRotationSpine = spineTransform.localRotation;
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
        timeSinceLastComment += Time.deltaTime;
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
            OnNutshot(sackTapVelocity);
        }

        if(collision.gameObject.TryGetComponent<Beer>(out Beer _beer))
        {
            ApplyBeerHitback(_beer);
        }
    }

    void OnNutshot(Vector3 sackTapVelocity)
    {
        PokeNuts(sackTapVelocity);

        if(spineAnimRoutine != null) StopCoroutine(spineAnimRoutine);
        spineAnimRoutine = StartCoroutine(RecoilAnimationSpine(1, Mathf.Sign(sackTapVelocity.x)));

        _player.OnHitStun(nutsackIframeTime, inputresponseCurve);

        headAnimation.NutshotFace();
        Stop(0.05f);
        impactPFX.Play();
        AudioManager.Instance.PlaySound(AudioManager.Instance.slapNut, 0.025f, Random.Range(0.4f, 0.6f), transform.position, 3);
    }

    void RecoilPoseSpine()
    {
        StartCoroutine(startIFrames());
    }

    IEnumerator RecoilAnimationSpine(float duration, float _direction)
    {
        float elapsedTime = 0f;
        float strength = Random.Range(1f, 1.25f);
        Quaternion recoilPose = Quaternion.Euler(new Vector3(0,0,spineRecoilPose.z * _direction * strength));

        while (elapsedTime < duration)
        {
            float percentComplete = elapsedTime/duration;
            float t = SpineAnimationCurve.Evaluate(percentComplete);
            spineTransform.localRotation = Quaternion.Lerp(restingRotationSpine, recoilPose, t);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }
    }


    void ApplyBeerHitback(Beer _beer)
    {
        Vector3 beerToNutsDirection =(_beer.transform.position - transform.position).normalized;
        if (!_beer.OnGround)
        {
            beerToNutsDirection.y *= .25f;
        _beer.body.velocity = beerToNutsDirection * 0.4f;
        _beer.body.angularVelocity = beerToNutsDirection.x * 5f * Vector3.forward;
        }
    }

    void PokeNuts(Vector3 _impactVelocity)
    {
        StartCoroutine(startIFrames());
        crotchRigidBody.velocity += Vector3.up * 0.125f;
        currentVelocity += _impactVelocity * pokeMultiplier;
        currentVelocity.y *= 2.5f ;

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
        float minForceDistance = 0.15f;
        float maxForceDistance = 0.2f;
        distanceToNut = Mathf.Clamp(distanceToNut, minForceDistance, maxForceDistance);
        distanceToNut = (distanceToNut - minForceDistance) * (1 / (maxForceDistance - minForceDistance));

        float nutForceAmount = 1-distanceToNut;

        _body.velocity = repulseDirection * force.magnitude * nutForceAmount * 0.75f;
    }

    void ApplyDrag(float _drag)
    {
        currentVelocity *= 1f - _drag * Time.deltaTime;
    }

    private bool isWaiting = false;

    public void Stop(float duration)
    {
        if (isWaiting) return;
        StartCoroutine(WaitHitStop(duration));
    }

    private IEnumerator WaitHitStop(float duration)
    {
        isWaiting = true;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        isWaiting = false;
    }
}
