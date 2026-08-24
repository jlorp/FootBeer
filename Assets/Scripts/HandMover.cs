using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMover : MonoBehaviour
{

    [Header("Hand Movement")]
    public float acceleration;
    public float maxSpeed;
    public Rigidbody rightHandRB, leftHandRB;

    [Header("Finger Curl")]
    public Transform fingerTipPosition;
    public Transform tabPosition;
    public float curlThreshold;
    public float uncurlThreshold;
    public Animator rHandAnimator;
    bool curled = false;

    [Header("Tab Grab")]
    public Transform tabGrabParent;
    bool tabGrabbed = false;
    public OrientationConstraint rightConstraint;
    public Transform cantab, canMouthPiece;

    [Header("Tab Pull")]
    public Transform canVibratePoint;
    public Transform canTabTip;
    public Transform beerCan;
    public float tabMinRotate, tabMaxRotate, tabOpenRotate;
    public float canOpenTime, maxVibrateIntensity;
    public AnimationCurve TabWiggle;
    public ParticleSystem canExplodeParticle;

    Transform rHandTargetTransform, originalRightHandParent;
    Quaternion originalRightHandRotation;

    bool canOpen = false;
    float tabTipStartPositionZ, parentPositionZ;
    Vector3 tabStartPosition;

    [Header("Beer Tap/Drop")]
    public float tapCastDistance;
    public LayerMask beerTapMask;

    //Input
    Vector2 rightHandInput, leftHandInput;
    bool pullingTab;
    float framesPullingTab;
    float curlHeight, pokeHeight;
    float rTargetHeight;

    public bool sceneActive;

    void Start()
    {
        tabTipStartPositionZ = beerCan.InverseTransformPoint(canTabTip.position).z;
        parentPositionZ = tabGrabParent.localPosition.z;
        rHandTargetTransform = rightHandRB.transform;
        originalRightHandParent = rHandTargetTransform.parent;
        originalRightHandRotation = rHandTargetTransform.localRotation;
        tabStartPosition = cantab.localPosition;

        pokeHeight= rHandTargetTransform.position.y;
        curlHeight = pokeHeight + 0.3f;
        rTargetHeight = pokeHeight;
    }

    void Update()
    {
        if(!sceneActive) return;

        UpdateInputs();
        if(Input.GetKeyDown(KeyCode.Alpha4)) GrabTab();
        VibrateCan();
        PullTab();

        if(framesPullingTab > canOpenTime) OpenCan();
        
        UpdateFingerPosition();
    }

    void FixedUpdate()
    {
        if(!sceneActive)return;
        BeerTapCast();

        MoveHands();
        UpdateCurl();   
        AdjustRightHandHeight();
    }

    void AdjustRightHandHeight()
    {
        if(tabGrabbed || canOpen) return;
        if(rHandTargetTransform.position.y == rTargetHeight) return;
        rHandTargetTransform.position =  new Vector3(rHandTargetTransform.position.x, rTargetHeight, rHandTargetTransform.position.z);
    }

    void UpdateFingerPosition() 
    {
        float tabTipZ = beerCan.InverseTransformPoint(canTabTip.position).z;
        float tabOffset = Mathf.Abs(tabTipZ - tabTipStartPositionZ);

        Vector3 tabGrabTargetPosition = tabGrabParent.localPosition;
        tabGrabTargetPosition.z = parentPositionZ + tabOffset;
        tabGrabParent.localPosition = tabGrabTargetPosition;
    }

    void BeerTapCast()
    {
        if(tabGrabbed || curled || canOpen) return;

        RaycastHit hit; 
        //senses beer during point phase to knock over
        
        Vector3 castDirection = rightHandRB.velocity.normalized;
        if(castDirection.magnitude < .1f) castDirection = -fingerTipPosition.right;

        Debug.DrawRay(fingerTipPosition.position, castDirection * tapCastDistance, Color.red);

        if (Physics.SphereCast(fingerTipPosition.position, 0.05f, castDirection, out hit, tapCastDistance, beerTapMask))
        {
            Vector3 handDirection = (fingerTipPosition.position - beerCan.position).normalized;
            leftHandRB.velocity = -maxSpeed * handDirection * 1.5f;
            rightHandRB.velocity = maxSpeed * handDirection * 1.5f;
        }
    }
    
    void PullTab()
    {
        if(!pullingTab || canOpen) return;
        float pullPercent = framesPullingTab/canOpenTime;
        float tabRotationCurrent = Mathf.Lerp(tabMinRotate, tabMaxRotate, pullPercent);
        cantab.localRotation = Quaternion.Euler(tabRotationCurrent,0,0);
    }

    void OpenCan()
    {
        if(canOpen) return;
        StartCoroutine(OpenCanAnimation(.3f));
        

        canOpen = true;
        DropTab();
        Uncurl();
        rightHandRB.velocity = Vector3.right * -2f;
        leftHandRB.velocity = Vector3.right * 1f;
    }

    void VibrateCan()
    {
        if(!pullingTab || canOpen)
        {
            canVibratePoint.localPosition = Vector3.zero;
            cantab.localPosition = tabStartPosition;
            return;
        } 

        float speed = 10.0f;
        
        float intensity = Mathf.Clamp(framesPullingTab/canOpenTime,0,1);
        intensity = Mathf.Lerp(0, maxVibrateIntensity, intensity);
        Vector3 scrollingNoise = new Vector3(
            Mathf.PerlinNoise(speed * Time.time, 1),
            Mathf.PerlinNoise(speed * Time.time, 2),
            Mathf.PerlinNoise(speed * Time.time, 3));

        cantab.localPosition = tabStartPosition + (intensity * 0.05f * scrollingNoise);
        canVibratePoint.localPosition = intensity * scrollingNoise;
    }

    IEnumerator LerpLocalPostion(float duration, Transform _transform, Vector3 _endPosition)
    {
        float elapsedTime = 0;
        Vector3 _startPosition = _transform.localPosition;
        Quaternion _startRotation = _transform.localRotation;

        while(elapsedTime < duration && tabGrabbed)
        {
            float t = elapsedTime / duration;

            _transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, t);
            _transform.localRotation= Quaternion.Lerp(_startRotation, Quaternion.identity, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if(tabGrabbed)
        {
            _transform.localPosition = _endPosition;
            _transform.localRotation = Quaternion.identity;
        }
    }

    IEnumerator OpenCanAnimation(float duration)
    {
        float elapsedTime = 0;
        Quaternion startRotTab = cantab.localRotation;
        Quaternion targetRotTab = Quaternion.Euler(tabOpenRotate,0,0);

        canMouthPiece.localRotation = Quaternion.Euler(100f,0,0);
        AudioManager.Instance.PlaySound(AudioManager.Instance.canOpenSound, 0.4f, 1.0f, canMouthPiece.position);

        while(elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float tAnimated = TabWiggle.Evaluate(t);
            cantab.localRotation = Quaternion.Lerp(startRotTab,targetRotTab, tAnimated);
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        canExplodeParticle.Play();
    }

    void GrabTab()
    {
        if(tabGrabbed) return;
        if(canOpen) return;
        if(rightHandInput.x > 0) return;

        tabGrabbed = true;
        rightHandRB.transform.SetParent(tabGrabParent);
        
        StartCoroutine(LerpLocalPostion(0.1f, rightHandRB.transform,Vector3.zero));
        rightConstraint.rotationSpeed = 100;

        cantab.localRotation = Quaternion.Euler(tabMinRotate,0,0);
        Destroy(rightHandRB);
    }

    void DropTab()
    {
        tabGrabbed = false;
        rHandTargetTransform.SetParent(originalRightHandParent);
        rHandTargetTransform.transform.localRotation= originalRightHandRotation;
        rightConstraint.rotationSpeed = 10;

        rHandTargetTransform.gameObject.AddComponent<Rigidbody>();
        rightHandRB = rHandTargetTransform.gameObject.GetComponent<Rigidbody>();
        rightHandRB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        rightHandRB.useGravity = false; 
    }

    void UpdateCurl()
    {
        if(tabGrabbed || canOpen) return;

        float tipDistance = fingerTipPosition.position.x - tabPosition.position.x;
        float tipDistanceY = fingerTipPosition.position.z - tabPosition.position.z;

        float curlYMinimum = -0.35f;
        float curlYMaximum = -.2f;

        bool inCurlXRange = tipDistance > curlThreshold;
        bool inUncurlXRange = tipDistance < uncurlThreshold;
        bool inCurlYRange =  tipDistanceY < curlYMinimum;
        bool inUncurlYrange = tipDistanceY > curlYMaximum;

        bool inGrabXRange = tipDistance > .15f && tipDistance < .35f;
        

        if(!curled && (inCurlYRange)) Curl();
        if(curled && (inUncurlYrange && inUncurlXRange)) Uncurl();

        if(curled && tipDistanceY > -.25f && inGrabXRange) GrabTab();
    }

    void Curl()
    {
        curled = true;
        rHandAnimator.SetBool("curled", true);
        rTargetHeight = curlHeight;
    }

    void Uncurl()
    {
        curled = false;
        rHandAnimator.SetBool("curled", false);
        rTargetHeight = pokeHeight;
    }

    void MoveHands()
    {
        if(!tabGrabbed) AdjustBodyVelocity(rightHandRB, rightHandInput);
        AdjustBodyVelocity(leftHandRB, leftHandInput);
    }

    void AdjustBodyVelocity(Rigidbody body, Vector2 desiredVelocity)
    {
        Vector3 adjustedAxis = new Vector3(desiredVelocity.x, 0, desiredVelocity.y);
        body.velocity = Vector3.MoveTowards(body.velocity, adjustedAxis * maxSpeed, acceleration * Time.deltaTime);
    }

    void  UpdateInputs()
    {
        float leftX = -Input.GetAxisRaw("Left Horizontal");
        float leftY = -Input.GetAxisRaw("Left Vertical");
        float rightX = -Input.GetAxisRaw("Right Horizontal");
        float rightY = -Input.GetAxisRaw("Right Vertical");

        leftHandInput = new Vector2(leftX, leftY);
        rightHandInput = new Vector2(rightX, rightY);

        if(tabGrabbed)
        {
            leftHandInput = (leftHandInput * 0.5f + rightHandInput * 0.5f);
            leftHandInput = Vector2.ClampMagnitude(leftHandInput,1f);

            if(rightHandInput.y < 0 || rightHandInput.x > 0) DropTab();

            pullingTab = (rightX < 0 && leftX > 0);

            if (pullingTab && !canOpen)
            {
                framesPullingTab += 1 * Time.deltaTime;
            }
            else
            {
                framesPullingTab = 0;
            }
        } 
    }
}
