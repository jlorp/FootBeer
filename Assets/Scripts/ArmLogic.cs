using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLogic : MonoBehaviour
{
    Vector3 handStartPosition;
    public Transform armFinalPosition,armStartPosition;

    public bool canInRange;

    public float rotationTime, rotationRange;
    public Transform beercan;

    Quaternion startRotation, upperArmStartRotation;
    public bool armInPosition;

    bool wristFired = false;
    bool wristExtending, wristReturning;
    public Transform elbow,shoulder;
    public float armSpeed;

    public float minCanGrabPosition, maxCanGrabPosition;
    [HideInInspector]public bool allowArmDrop = false;

    float armRotationTime = 0f;

    //solving IK
    float forearmLength, upperArmLength;
    public Transform handPosition;

    float handToShoulderDistance;
    public Transform handPositionTarget;


    //stretchyHand;
    Vector3 elbowStartLocalPosition, handStartLocalPosition;
    public Transform forearmScaler, armScaler;

    [HideInInspector]public bool holdingBeer = false;

    [HideInInspector] public bool sceneActive = true;

    public SkinnedMeshRenderer armRenderer;
    
    //connecting the arm to the body
    public Transform shoulderTargetPosition;
    public Vector3 desiredElbowAngle;
    Vector3 elbowTargetPosition;

    void Start()
    {
        startRotation = elbow.localRotation;
        upperArmStartRotation = transform.localRotation;

        forearmLength = (handPosition.localPosition.magnitude + forearmScaler.localPosition.magnitude);
        upperArmLength = (elbow.localPosition.magnitude + armScaler.localPosition.magnitude);
        handToShoulderDistance = Vector3.Distance(handPosition.position, transform.position);

        elbowStartLocalPosition = armScaler.localPosition;
        handStartLocalPosition = forearmScaler.localPosition;

        elbowTargetPosition = armStartPosition.position;
    }

    void Update()
    {
        if(!sceneActive) return;

        if(holdingBeer && beercan.position.y > 1.05f) SwitchScene();
        if(Input.GetKeyDown(KeyCode.Alpha5)) SwitchScene();

        CheckCanPosition();
        SetElbowPosition();
        RotationPingPong();
        if(Input.GetKeyDown(KeyCode.Space)) FireWrist();
        HandleWristMovement();
    }

    void SwitchScene()
    {
        sceneActive = false;
        CameraManager.Instance.SwitchCamera(2,true);
        AudioManager.Instance.ExitWater();
        GameManager.Instance.StartArmRaise();
    }

    public void ForceArmUp()
    {
        elbowTargetPosition = armStartPosition.position;
        armRenderer.SetBlendShapeWeight(0, 0f);
    }
    
    void ExtendArm(Vector3 desiredHandPosition)
    {
        
        float handToShoulder = Vector3.Distance(desiredHandPosition, transform.position);
        float maxExtension = upperArmLength + forearmLength - .0000001f;
        float amountOverClamp = Mathf.Clamp( handToShoulder - maxExtension, 0,5);

        handToShoulder = Mathf.Clamp(handToShoulder, 0.05f, maxExtension);

        float forearmStretch = (forearmLength + (amountOverClamp/2)) / forearmLength;
        float forearmSquash = 2-forearmStretch;

        float upperArmStretch =  (upperArmLength + (amountOverClamp/2)) / upperArmLength;
        float upperArmSquash = 2-upperArmStretch;

        //stretchy arm
        //armScaler.localScale = new Vector3(upperArmSquash, upperArmStretch, upperArmSquash);
        //forearmScaler.localScale = new Vector3(forearmSquash, forearmSquash, forearmStretch);
       
        armScaler.localPosition = elbowStartLocalPosition - Vector3.right * (amountOverClamp/2);
        forearmScaler.localPosition = handStartLocalPosition - Vector3.right * (amountOverClamp/2);

        float a = handToShoulder;
        float b = forearmLength;
        float c = upperArmLength;

        float elbowAngle = CosAngle(b,c,a);

        a = forearmLength;
        b = upperArmLength;
        c=  handToShoulder;

        float shoulderAngle = CosAngle(b,c,a);

        elbow.localEulerAngles = new Vector3(0, elbowAngle + 180, 0 );

        Vector3 directionToHandGlobal = desiredHandPosition - transform.position;
        Vector3 directionToHand = new Vector3(directionToHandGlobal.x, directionToHandGlobal.y, directionToHandGlobal.z);
        float shoulderToBeerAngle = Mathf.Atan2(directionToHand.y, directionToHand.x) * Mathf.Rad2Deg;

        shoulder.localEulerAngles = new Vector3(0, shoulderAngle - (shoulderToBeerAngle) + 180, 0);

        //handPosition.rotation = handPositionTarget.rotation;
    }

    void CheckCanPosition()
    {
        if(!allowArmDrop) return;

        if(beercan.position.y > minCanGrabPosition) canInRange = true;
        if(beercan.position.y < maxCanGrabPosition) canInRange = false;

        if(holdingBeer)
        {
            canInRange = wristFired;
        } 
    } 

    void RotationPingPong()
    {
        if(wristFired || holdingBeer) return;

        armRotationTime += Time.deltaTime;

        Quaternion targetRotation;
        if(armInPosition && !holdingBeer)
        {
            float lerpPostion = Mathf.PingPong(armRotationTime, rotationTime)/rotationTime;
            Vector3 directionToBeer = beercan.position - elbow.position;
            directionToBeer.z=0;

            Quaternion lookRotation = Quaternion.Euler(0,-Vector3.SignedAngle(shoulder.right, directionToBeer, Vector3.forward) + 180,0);
            Quaternion elbowAngle = Quaternion.Euler(0,-Vector3.SignedAngle(elbow.right, desiredElbowAngle, Vector3.forward) + shoulder.localEulerAngles.y, 0);

            Quaternion upRotation = lookRotation * Quaternion.Euler(Vector3.up * rotationRange);
            Quaternion downRotation = lookRotation * Quaternion.Euler(Vector3.up * -rotationRange);
            targetRotation = Quaternion.Slerp(upRotation, downRotation, lerpPostion);
        }
        else
        {
            targetRotation = startRotation;
        }

        elbow.localRotation = Quaternion.Lerp(elbow.localRotation, targetRotation, Time.deltaTime * 2f);
    }
    
    void HandleWristMovement()
    {
        if(!wristFired) return;

        ExtendArm(handPositionTarget.position);
      
        if(wristExtending)
        {
            handPositionTarget.position -= handPositionTarget.right * Time.deltaTime * armSpeed;
            float armExtension = Vector3.Distance(handPositionTarget.position, handStartPosition);
            if(armExtension >= .5 || holdingBeer)
            {
                wristExtending = false;
                wristReturning = true;
            }
        }
        if (wristReturning)
        {
            handPositionTarget.position= Vector3.MoveTowards( handPositionTarget.position, handStartPosition, Time.deltaTime * armSpeed);
            if(handPositionTarget.position == handStartPosition)
            {
                transform.rotation = upperArmStartRotation;
                handPosition.localRotation = Quaternion.identity;
                wristReturning = false;
                wristFired = false;
            }
        }
    }

    void FireWrist()
    {
        if(wristFired || !canInRange) return;

        handPositionTarget.position = handPosition.position;
        handStartPosition = handPosition.position;
        handPositionTarget.rotation = handPosition.rotation;
        
        wristFired = true;
        wristExtending =true;
        wristReturning = false;
    }

    void SetElbowPosition()
    {
        transform.position = shoulderTargetPosition.position;

        if(wristFired) return;

        Vector3 targetPosition = canInRange ? armFinalPosition.position : armStartPosition.position;
        float movespeed = canInRange ? 3f : 3f;
        
        // set upper arm rotation
        Vector3 directionElbowTarget = elbowTargetPosition - transform.position;
        directionElbowTarget.z=0;
        Quaternion lookRotation = Quaternion.Euler(0,-Vector3.SignedAngle(Vector3.up, directionElbowTarget, Vector3.forward) + 90, 0);
        shoulder.localRotation = Quaternion.Lerp(shoulder.localRotation, lookRotation, Time.deltaTime * 30f);

        // set upper arm stretch
        armScaler.localPosition = Vector3.Lerp(armScaler.localPosition,-Vector3.right * Vector3.Magnitude(directionElbowTarget), 50f * Time.deltaTime);

        // set lower arm rotation
        elbowTargetPosition = Vector3.Lerp(elbowTargetPosition, targetPosition, Time.deltaTime * 3f);

        armInPosition = (Vector3.Distance(elbowTargetPosition, armFinalPosition.position) <.1f);

        if(armInPosition && !holdingBeer) return;

        Quaternion elbowAngle = Quaternion.Euler(0, -Vector3.SignedAngle(-armScaler.right, desiredElbowAngle, Vector3.forward), 0);
        elbow.localRotation = Quaternion.Lerp(elbow.localRotation, elbowAngle, Time.deltaTime * 30f);
    }

    float CosAngle(float a, float b, float c) 
    {
        if ( !float.IsNaN(Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (-2 * a * b)) * Mathf.Rad2Deg))
        {
            return Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (2 * a * b)) * Mathf.Rad2Deg;
        }
        else
        {
            return 1;
        }
    }
}
