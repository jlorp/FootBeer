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
        StartCoroutine(RecoilAnimationSpine(1));
        headAnimation.NutshotFace();
        Stop(0.05f);
        impactPFX.Play();
        AudioManager.Instance.PlaySound(AudioManager.Instance.slapNut, 0.025f, Random.Range(0.4f, 0.6f), transform.position, 3);
        //NutshotComment();
    }

    void RecoilPoseSpine()
    {
        StartCoroutine(startIFrames());
    }

    IEnumerator RecoilAnimationSpine(float duration)
    {
        float elapsedTime = 0f;
        Quaternion recoilPose = Quaternion.Euler(spineRecoilPose);

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
        _beer.body.velocity = beerToNutsDirection * 0.5f;
        _beer.body.angularVelocity = beerToNutsDirection.x * 5f * Vector3.forward;
        }
    }

    void PokeNuts(Vector3 _impactVelocity)
    {
        StartCoroutine(startIFrames());
        crotchRigidBody.velocity += Vector3.up * 0.2f;
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

    void NutshotComment()
    {
        if(timeSinceLastComment < minTimeBetweenComments) return;

        switch (commentsMade)
        {
            case 0:
                GameManager.Instance.PlayDialogue("fuck", .1f, 1f, 0, AudioManager.Instance.fuck);
                break;
            case 1:
                GameManager.Instance.PlayDialogue("god damnit", .1f, 1f, 0f, AudioManager.Instance.fuck);
                break;
            case 2:
                GameManager.Instance.PlayDialogue("christ", .1f, 1f, 0f, AudioManager.Instance.fuck);
                break;
            case 3:
                GameManager.Instance.PlayDialogue("my nuts!", .1f, 1f, 0f, AudioManager.Instance.fuck);
                break;
            case 4:
                GameManager.Instance.PlayDialogue("come ON", .1f, 1f, 0f, AudioManager.Instance.fuck);
                break;
            case 5:
                GameManager.Instance.PlayDialogue("again?", .1f, 1f, 0f, AudioManager.Instance.fuck);
                break;
            case 6:
                GameManager.Instance.PlayDialogue("Gahh", .1f, 1f, 0f, AudioManager.Instance.fuck);
                break;
            default:
                commentsMade = 0;
                GameManager.Instance.PlayDialogue("GHHHR", .1f, 1f, 0f, AudioManager.Instance.fuck);
                break;
        }

        commentsMade +=1;
        timeSinceLastComment = 0;
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
