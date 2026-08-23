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

    Transform rHandTargetTransform, originalRightHandParent;
    Quaternion originalRightHandRotation;


    public float canOpenTime, maxVibrateIntensity;
    bool canOpen = false;
    float tabTipStartPositionZ, parentPositionZ;

    //Input
    Vector2 rightHandInput, leftHandInput;
    bool pullingTab;
    float framesPullingTab;

    public bool sceneActive;

    void Start()
    {
        tabTipStartPositionZ = beerCan.InverseTransformPoint(canTabTip.position).z;
        parentPositionZ = tabGrabParent.localPosition.z;
        rHandTargetTransform = rightHandRB.transform;
        originalRightHandParent = rHandTargetTransform.parent;
        originalRightHandRotation = rHandTargetTransform.localRotation;
    }

    void Update()
    {
        if(!sceneActive) return;

        UpdateInputs();
        if(Input.GetKeyDown(KeyCode.Alpha4)) GrabTab();
        VibrateCan();
        if(framesPullingTab > canOpenTime) OpenCan();
        UpdateFingerPosition();
    }

    void FixedUpdate()
    {
        if(!sceneActive)return;

        MoveHands();
        UpdateCurl();   
    }

    void UpdateFingerPosition() 
    {
        float tabTipZ = beerCan.InverseTransformPoint(canTabTip.position).z;
        float tabOffset = Mathf.Abs(tabTipZ - tabTipStartPositionZ);

        Vector3 tabGrabTargetPosition = tabGrabParent.localPosition;
        tabGrabTargetPosition.z = parentPositionZ + tabOffset;
        tabGrabParent.localPosition = tabGrabTargetPosition;
    }
    void OpenCan()
    {
        if(canOpen) return;

        cantab.localRotation = Quaternion.Euler(60f,0,0);
        canMouthPiece.localRotation = Quaternion.Euler(100f,0,0);
        canOpen = true;
        DropTab();
        Uncurl();
        rightHandRB.velocity = Vector3.right * -2f;
        leftHandRB.velocity = Vector3.right * 2f;
    }

    void VibrateCan()
    {
        if(!pullingTab || canOpen)
        {
            canVibratePoint.localPosition = Vector3.zero;
            return;
        } 

        float speed = 5.0f;
        
        float intensity = Mathf.Clamp(framesPullingTab/canOpenTime,0,1);
        intensity = Mathf.Lerp(0, maxVibrateIntensity, intensity);

        canVibratePoint.localPosition = intensity * new Vector3(
            Mathf.PerlinNoise(speed * Time.time, 1),
            Mathf.PerlinNoise(speed * Time.time, 2),
            Mathf.PerlinNoise(speed * Time.time, 3));
  
    }

    IEnumerator LerpLocalPostion(float duration, Transform _transform, Vector3 _endPosition)
    {
        float elapsedTime = 0;
        Vector3 _startPosition = _transform.localPosition;

        while(elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            _transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _transform.localPosition = _endPosition;
    }

    void GrabTab()
    {
        if(tabGrabbed) return;
        if(canOpen) return;

        tabGrabbed = true;
        rightHandRB.velocity = Vector3.zero;
        rightHandRB.transform.SetParent(tabGrabParent);
        rightHandRB.transform.localRotation= Quaternion.identity;
        StartCoroutine(LerpLocalPostion(0.2f, rightHandRB.transform,Vector3.zero));
        rightConstraint.rotationSpeed = 100;

        cantab.localRotation = Quaternion.Euler(22.5f,0,0);
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
        if(tabGrabbed) return;

        float tipDistance = fingerTipPosition.position.x - tabPosition.position.x;
        float tipDistanceY = fingerTipPosition.position.z - tabPosition.position.z;

        float curlYMinimum = -2f;
        float curlYMaximum = 0;

        bool inCurlXRange = tipDistance > curlThreshold;
        bool inUncurlXRange = tipDistance < uncurlThreshold;
        bool inCurlYRange = tipDistanceY > curlYMinimum && tipDistanceY < curlYMaximum;

        if(curled && (inUncurlXRange || !inCurlYRange)) Uncurl();
        if(!curled && (inCurlXRange && inCurlYRange)) Curl();

        if(curled && tipDistanceY > -.25f) GrabTab();
    }

    void Curl()
    {
        curled = true;
        rHandAnimator.SetBool("curled", true);
    }

    void Uncurl()
    {
        curled = false;
        rHandAnimator.SetBool("curled", false);
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
