using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLogic_Simplified : MonoBehaviour
{
    Vector3 handStartPosition;

    public bool canInRange;

    public Transform beercan;
    
    public float outRangeY, inRangeY;
    public float xMinPositionArm, XMaxPositionArm;

    Quaternion startRotation, upperArmStartRotation;
    public bool armInPosition;

    bool wristFired = false;
    bool wristExtending, wristReturning;
    public Transform elbow,shoulder;
    public float armSpeed;

    public float minCanGrabPosition, maxCanGrabPosition;
    [HideInInspector]public bool allowArmDrop = false;

    //solving IK
    float forearmLength, upperArmLength;
    public Transform handPosition;

    float handToShoulderDistance;
    public Transform handPositionTarget;


    //stretchyHand;
    Vector3 elbowStartLocalPosition, handStartLocalPosition;
    public Transform forearmScaler, armScaler;

    [HideInInspector]public bool holdingBeer = false;

    void Start()
    {
        startRotation = elbow.localRotation;
        upperArmStartRotation = transform.localRotation;

        forearmLength = (handPosition.localPosition.magnitude + forearmScaler.localPosition.magnitude);
        upperArmLength = (elbow.localPosition.magnitude + armScaler.localPosition.magnitude);
        handToShoulderDistance = Vector3.Distance(handPosition.position, transform.position);

        elbowStartLocalPosition = armScaler.localPosition;
        handStartLocalPosition = forearmScaler.localPosition;
    }

    void Update()
    {
        CheckCanPosition();
        SetElbowPosition();
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

        if(holdingBeer) canInRange = true;
    } 

    void SetElbowPosition()
    {
        if(wristFired) return;
        if(!allowArmDrop) return;

        float xPosition = Mathf.Clamp(beercan.position.x + upperArmLength, xMinPositionArm, XMaxPositionArm);

        float yOffset = Mathf.PingPong(Time.time/40, 0.05f);

        Vector3 outRangePosition = new Vector3(xPosition, outRangeY + yOffset, transform.position.z);
        Vector3 inRangePosition = new Vector3(xPosition, inRangeY + yOffset, transform.position.z);

        Vector3 targetPosition = canInRange ? inRangePosition: outRangePosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
        armInPosition = (Vector3.Distance(transform.position, inRangePosition)<.1f);
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