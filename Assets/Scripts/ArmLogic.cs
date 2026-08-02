using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLogic : MonoBehaviour
{
    Vector3 startPosition, downPosition;
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

    void Start()
    {
        startPosition = transform.position;
        downPosition = armFinalPosition.position;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        CheckCanPosition();
        SetElbowPosition();
        RotationPingPong();
        if(Input.GetKeyDown(KeyCode.Space)) FireWrist();
        HandleWristMovement();
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

        Quaternion targetRotation;
        if(armInPosition)
        {
            float lerpPostion = Mathf.PingPong(Time.time, rotationTime)/rotationTime;
            Vector3 directionToBeer = beercan.position - transform.position;
            directionToBeer.z=0;
            Quaternion lookRotation = Quaternion.LookRotation((directionToBeer).normalized , Vector3.up);
            Quaternion upRotation = lookRotation * Quaternion.Euler(Vector3.right * rotationRange);
            Quaternion downRotation = lookRotation * Quaternion.Euler(Vector3.right * -rotationRange);
            targetRotation = Quaternion.Slerp(upRotation, downRotation, lerpPostion);
        }
        else
        {
            targetRotation = startRotation;
        }

        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 5f);
    }
    
    void HandleWristMovement()
    {
        if(!wristFired) return;

        if(wristExtending)
        {
            elbow.localPosition += Vector3.forward * Time.deltaTime * armSpeed;
            if(elbow.localPosition.z > maxDistanceWristFire)
            {
                wristExtending = false;
                wristReturning = true;
            }
        }
        if (wristReturning)
        {
            elbow.localPosition = Vector3.MoveTowards(elbow.localPosition, Vector3.zero, Time.deltaTime * armSpeed);
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
