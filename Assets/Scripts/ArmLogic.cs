using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLogic : MonoBehaviour
{
    Vector3 startPosition, downPosition, elbowStartPosition;
    public Transform armFinalPosition;

    public bool canInRange;

    public float rotationTime, rotationRange;
    public Transform beercan;

    Quaternion startRotation;
    public bool armInPosition;

    bool wristFired = false;
    bool wristExtending, wristReturning;
    public float maxDistanceWristFire = 1;
    public Transform elbow;
    public float armSpeed;

    public float minCanGrabPosition, maxCanGrabPosition;
    [HideInInspector]public bool allowArmDrop = false;

    float armRotationTime = .25f;

    //solving IK
    public float forearmLength, upperArmLength;
    public Transform handPosition;

    float handToShoulderDistance;
    public Transform handPositionTarget;

    void Start()
    {
        startPosition = transform.position;
        downPosition = armFinalPosition.position;
        startRotation = elbow.rotation;
        elbowStartPosition = elbow.localPosition;

        forearmLength = handPosition.localPosition.magnitude;
        upperArmLength = elbow.localPosition.magnitude;
        handToShoulderDistance = Vector3.Distance(handPosition.position, transform.position);
    }

    void Update()
    {
        ExtendArm(handPositionTarget.position);
        //CheckCanPosition();
        //SetElbowPosition();
        //RotationPingPong();
        //if(Input.GetKeyDown(KeyCode.Space)) FireWrist();
        //HandleWristMovement();
    }

    void ExtendArm(Vector3 desiredHandPosition)
    {
        
        float handToShoulder = Vector3.Distance(desiredHandPosition, transform.position);
        handToShoulder = Mathf.Clamp(handToShoulder, 0.05f,  upperArmLength + forearmLength - .0000001f);

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

    }

    float CosAngle(float a, float b, float c) {
        if ( !float.IsNaN(Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (-2 * a * b)) * Mathf.Rad2Deg))
        {
            return Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (2 * a * b)) * Mathf.Rad2Deg;
        }
        else
        {
            return 1;
        }
    }

    void CheckCanPosition()
    {
        if(!allowArmDrop) return;

        if(beercan.position.y > minCanGrabPosition) canInRange = true;
        if(beercan.position.y < maxCanGrabPosition) canInRange = false;
    } 

    void RotationPingPong()
    {
        if(wristFired) return;

        armRotationTime += Time.deltaTime;

        Quaternion targetRotation;
        if(armInPosition)
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

        if(wristExtending)
        {
            elbow.localPosition += elbow.forward * Time.deltaTime * armSpeed;
            if(elbow.localPosition.magnitude > maxDistanceWristFire)
            {
                wristExtending = false;
                wristReturning = true;
            }
        }
        if (wristReturning)
        {
            elbow.localPosition = Vector3.MoveTowards(elbow.localPosition, elbowStartPosition, Time.deltaTime * armSpeed);
            if(elbow.localPosition == Vector3.zero)
            {
                wristReturning = false;
                wristFired = false;
            }
        }
    }

    void FireWrist()
    {
        if(wristFired || !canInRange) return;
        Debug.Log("Fired wrist");
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
}
