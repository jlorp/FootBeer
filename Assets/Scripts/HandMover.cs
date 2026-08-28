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
    public Animator rHandAnimator,lHandAnimator;
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
    public GameObject beerDroppable;
    bool beerDropped = false;
    public Transform rHandYOffset;
    public float curlHeightOffset = 0.3f;
    public BeerSensor _beerSensor;

    //Input
    Vector2 rightHandInput, leftHandInput;
    bool pullingTab;
    float framesPullingTab;
    float rTargetHeight;

    public bool sceneActive;
    Vector3 rHandStartPosition;
    bool tabSet = false;

    void Start()
    {
        tabTipStartPositionZ = beerCan.InverseTransformPoint(canTabTip.position).z;
        parentPositionZ = tabGrabParent.localPosition.z;
        rHandTargetTransform = rightHandRB.transform;
        originalRightHandParent = rHandTargetTransform.parent;
        originalRightHandRotation = rHandTargetTransform.localRotation;
        tabStartPosition = cantab.localPosition;
        lHandAnimator.SetBool("holdBeer", true);

        rHandStartPosition=rightHandRB.transform.position;
    }

    void Update()
    {
        if(!sceneActive) return;

        UpdateInputs();
        VibrateCan();
        PullTab();

        if(framesPullingTab > canOpenTime) OpenCan();
        
        UpdateFingerPosition();
        if(Input.GetKeyDown(KeyCode.Space) && canOpen) GameManager.Instance.TakeDrink();
    }

    void FixedUpdate()
    {
        if(!sceneActive)return;
        MoveHands();
        UpdateCurl();   
        AdjustRightHandHeight();
    }

    void AdjustRightHandHeight()
    {
        if(tabGrabbed || canOpen) return;

        if(rHandYOffset.localPosition.y == rTargetHeight) return;
        Vector3 targetHeight=  new Vector3(0, rTargetHeight, 0);
        rHandYOffset.localPosition = Vector3.Lerp(rHandYOffset.localPosition, targetHeight, 10f * Time.deltaTime);

        if(rightHandRB.transform.position.y == rHandStartPosition.y) return;

        Vector3 targetHeight2 = rightHandRB.transform.position;
        targetHeight2.y = rHandStartPosition.y;
        float positiondifference = targetHeight2.y - rightHandRB.transform.position.y;
        rightHandRB.transform.position= targetHeight2;

        rHandYOffset.localPosition -= Vector3.up * positiondifference;
    }

    void UpdateFingerPosition() 
    {
        float tabTipZ = beerCan.InverseTransformPoint(canTabTip.position).z;
        float tabOffset = Mathf.Abs(tabTipZ - tabTipStartPositionZ);

        Vector3 tabGrabTargetPosition = tabGrabParent.localPosition;
        tabGrabTargetPosition.z = parentPositionZ + tabOffset;
        tabGrabParent.localPosition = tabGrabTargetPosition;
    }

    public void OnPoke(bool dropOnPoke, float bumpForce = 2.5f, bool normalizeX = true)
    {
        if((tabGrabbed || curled || canOpen) && dropOnPoke) return;
    

        Vector3 handDirection = handDirection = -(rightHandRB.velocity - leftHandRB.velocity).normalized;
        if(normalizeX) handDirection.x = -Mathf.Abs(handDirection.x);
        leftHandRB.velocity = -maxSpeed * handDirection * bumpForce;
        rightHandRB.velocity = maxSpeed * handDirection * bumpForce;

        float _pitch = UnityEngine.Random.Range(1.5f, 1.6f);
        AudioManager.Instance.PlaySound(AudioManager.Instance.canKickSounds[1], 0.5f, _pitch, fingerTipPosition.position);

        if(dropOnPoke) DropBeer(handDirection);
    }

    void DropBeer(Vector3 _handDirection)
    {
        beerDropped =true;
        lHandAnimator.SetBool("open", true);
        beerCan.gameObject.SetActive(false);
        var droppedBeer = Instantiate(beerDroppable, beerCan.position, beerCan.rotation);
        DroppableBeer _beer = droppedBeer.GetComponent<DroppableBeer>();
        _beer.SetAngularVelocity(_handDirection);
    }

    void PullTab()
    {
        if(!pullingTab || canOpen) return;
        float pullPercent = framesPullingTab/canOpenTime;
        float tabRotationCurrent = Mathf.Lerp(tabMinRotate, tabMaxRotate, pullPercent);
        cantab.localRotation = Quaternion.Euler(tabRotationCurrent,0,0);
    }

    public bool SetTab()
    {
        if(!canOpen && !curled) return false;

        Vector3 tapVelocity = -(rightHandRB.velocity - leftHandRB.velocity).normalized;

        if(Vector3.Dot(tapVelocity, Vector3.left) >.6f)
        {
            StartCoroutine(SetTabAnimation(0.1f));
            tabSet = true;
            return true;
        }
        else
        {
            OnPoke(false, 1f, false);
            return false;
        }
    }

    IEnumerator SetTabAnimation(float duration)
    {
        float elapsedTime = 0;
        Quaternion startRotTab = cantab.localRotation;
        Quaternion targetRotTab = Quaternion.Euler(0,0,0);

        while(elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            cantab.localRotation = Quaternion.Lerp(startRotTab,targetRotTab, t);
            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }

    void OpenCan()
    {
        if(canOpen) return;
        StartCoroutine(OpenCanAnimation(.3f));

        canOpen = true;
        DropTab();
        Uncurl();
        rightHandRB.velocity = Vector3.right * -4f;
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
        Vector3 _offsetStartPosition = rHandYOffset.localPosition;

        while(elapsedTime < duration && tabGrabbed)
        {
            float t = elapsedTime / duration;

            _transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, t);
            _transform.localRotation= Quaternion.Lerp(_startRotation, Quaternion.identity, t);

            rHandYOffset.localPosition = Vector3.Lerp(_offsetStartPosition, _endPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if(tabGrabbed)
        {
            _transform.localPosition = rHandYOffset.localPosition = _endPosition;
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
        if(tabGrabbed || canOpen || beerDropped) return;

        float tipDistance = fingerTipPosition.position.x - tabPosition.position.x;
        float tipDistanceY = fingerTipPosition.position.z - tabPosition.position.z;

        float curlYMinimum = -0.35f;
        float curlYMaximum = 0.4f;

        bool inCurlXRange = tipDistance > curlThreshold;
        bool inUncurlXRange = tipDistance < uncurlThreshold;
        bool inCurlYRange =  tipDistanceY < curlYMinimum;
        bool inUncurlYrange = tipDistanceY > curlYMaximum;

        bool inGrabXRange = tipDistance > 0f && tipDistance < .35f;
        

        if(!curled && (inCurlYRange && inCurlXRange)) Curl();

        if(curled && (inUncurlXRange || inUncurlYrange)) Uncurl();

        if(curled && tipDistanceY > -.3f && inGrabXRange) GrabTab();
    }

    void Curl()
    {
        curled = true;
        rHandAnimator.SetBool("curled", true);
        rTargetHeight = curlHeightOffset;
    }

    void Uncurl()
    {
        curled = false;
        rHandAnimator.SetBool("curled", false);
        rTargetHeight = 0;
        _beerSensor.Reset();
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
            if(rightHandInput.y < 0 || rightHandInput.x > 0 || leftHandInput.x < 0 || leftHandInput.y > 0) DropTab();
            
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

    public void ResetHandScene()
    {
        lHandAnimator.SetBool("holdBeer", true);
        lHandAnimator.SetBool("open", false);
        beerCan.gameObject.SetActive(true);
        
        rightHandRB.MovePosition(rHandStartPosition);
        rightHandRB.velocity=Vector3.zero;
        beerDropped = false;

        _beerSensor.Reset();
    }
}
