using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLogic : MonoBehaviour
{
    Vector3 startPosition, downPosition, handStartPosition;
    public Transform armFinalPosition;

    public bool canInRange;

    public float rotationTime, rotationRange;
    public Transform beercan;

    Quaternion startRotation, upperArmStartRotation;
    public bool armInPosition;

    bool wristFired = false;
    bool wristExtending, wristReturning;
    public Transform elbow;
    public float armSpeed;

    public float minCanGrabPosition, maxCanGrabPosition;
    [HideInInspector]public bool allowArmDrop = false;

    float armRotationTime = 0f;

    //solving IK
    public float forearmLength, upperArmLength;
    public Transform handPosition;

    float handToShoulderDistance;
    public Transform handPositionTarget;


    //stretchyHand;
    Vector3 elbowStartLocalPosition, handStartLocalPosition;
    public Transform forearmScaler, armScaler;

    [HideInInspector]public bool holdingBeer = false;

    void Start()
    {
        startPosition = transform.position;
        downPosition = armFinalPosition.position;
        startRotation = elbow.rotation;
        upperArmStartRotation = transform.localRotation;

        forearmLength = handPosition.localPosition.magnitude;
        upperArmLength = elbow.localPosition.magnitude;
        handToShoulderDistance = Vector3.Distance(handPosition.position, transform.position);

        elbowStartLocalPosition = elbow.localPosition;
        handStartLocalPosition = handPosition.localPosition;
    }

    void Update()
    {
        //ExtendArm(handPositionTarget.position);
        CheckCanPosition();
        SetElbowPosition();
        RotationPingPong();
        if(Input.GetKeyDown(KeyCode.Space)) FireWrist();
        HandleWristMovement();
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

        armScaler.localScale = new Vector3(upperArmSquash, upperArmStretch, upperArmSquash);
        forearmScaler.localScale = new Vector3(forearmSquash, forearmSquash, forearmStretch);
        
        elbow.localPosition = elbowStartLocalPosition - Vector3.up * (amountOverClamp/2);
        handPosition.localPosition = handStartLocalPosition + Vector3.forward * (amountOverClamp/2);

        float a = handToShoulder;
        float b = forearmLength;
        float c = upperArmLength;

        float elbowAngle = CosAngle(b,c,a);

        a = forearmLength;
        b = upperArmLength;
        c=  handToShoulder;

        float shoulderAngle = CosAngle(b,c,a);

        elbow.localEulerAngles = new Vector3(elbowAngle - 90, 90 , 0 );

        Vector3 directionToHand = desiredHandPosition - transform.position;
        float shoulderToBeerAngle = Mathf.Atan2(directionToHand.y, directionToHand.x) * Mathf.Rad2Deg;

        transform.localEulerAngles = new Vector3(0,0,- shoulderAngle + (shoulderToBeerAngle) + 90);

        handPosition.rotation = handPositionTarget.rotation;
    }

    void CheckCanPosition()
    {
        if(!allowArmDrop) return;

        if(beercan.position.y > minCanGrabPosition) canInRange = true;
        if(beercan.position.y < maxCanGrabPosition) canInRange = false;

        //canInRange = true;
    } 

    void RotationPingPong()
    {
        if(wristFired) return;

        armRotationTime += Time.deltaTime;

        Quaternion targetRotation;
        if(armInPosition && !holdingBeer)
        {
            float lerpPostion = Mathf.PingPong(armRotationTime, rotationTime)/rotationTime;
            Vector3 directionToBeer = beercan.position - elbow.position;
            directionToBeer.z=0;
            Quaternion lookRotation = Quaternion.LookRotation((directionToBeer).normalized , Vector3.right);
            Quaternion upRotation = lookRotation * Quaternion.Euler(Vector3.right * rotationRange);
            Quaternion downRotation = lookRotation * Quaternion.Euler(Vector3.right * -rotationRange);
            targetRotation = Quaternion.Slerp(upRotation, downRotation, lerpPostion);
        }
        else
        {
            targetRotation = startRotation;
        }

        elbow.rotation = Quaternion.Lerp(elbow.rotation, targetRotation, Time.deltaTime * 4f);
    }
    
    void HandleWristMovement()
    {
        if(!wristFired) return;

        ExtendArm(handPositionTarget.position);
      

        if(wristExtending)
        {
            handPositionTarget.position += handPositionTarget.forward * Time.deltaTime * armSpeed;
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
        if(wristFired) return;
        Vector3 targetPosition = canInRange ? downPosition : startPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
        armInPosition = (Vector3.Distance(transform.position, downPosition)<.1f);
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
