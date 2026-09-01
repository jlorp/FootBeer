using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleArm : MonoBehaviour
{
    public Transform shoulderTargetPosition,armUpPosition;
    public Transform elbowTargetPosition;
    public Vector3 desiredElbowAngle;

    public Transform shoulder, armScaler, elbow;

    void Update()
    {
        SetElbowPosition();
    }
    void SetElbowPosition()
    {
        //set position of shoulder
        transform.position = shoulderTargetPosition.position;

        Vector3 targetPosition = armUpPosition.position;
        float movespeed = 3f;
        
        // set upper arm rotation
        Vector3 directionElbowTarget = elbowTargetPosition.position - transform.position;
        directionElbowTarget.z=0;
        Quaternion lookRotation = Quaternion.Euler(0,Vector3.SignedAngle(Vector3.up, directionElbowTarget, Vector3.forward) + 90, 0);
        shoulder.localRotation = Quaternion.Lerp(shoulder.localRotation, lookRotation, Time.deltaTime * 30f);

        // set lower arm rotation;
        Quaternion elbowAngle = Quaternion.Euler(0, Vector3.SignedAngle(armScaler.right, desiredElbowAngle, Vector3.forward), 0);
        elbow.localRotation = Quaternion.Lerp(elbow.localRotation, elbowAngle, Time.deltaTime * 2f);
    }
}
