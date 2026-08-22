using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMover : MonoBehaviour
{

    [Header("Feet Movement")]
    public float acceleration;
    public float maxSpeed;
    public float dragNoInput;
    public Rigidbody rightHandRB, leftHandRB;

    //Input
    Vector2 rightHandInput, leftHandInput;

    public bool sceneActive;

    void Update()
    {
        if(!sceneActive) return;
        UpdateInputs();
    }

    void FixedUpdate()
    {
        if(!sceneActive)return;
        MoveHands();
    }

    void MoveHands()
    {
        AdjustBodyVelocity(rightHandRB, rightHandInput);
        AdjustBodyVelocity(leftHandRB, leftHandInput);
    }

    void AdjustBodyVelocity(Rigidbody body, Vector2 desiredVelocity)
    {
        Vector3 adjustedAxis = new Vector3(desiredVelocity.x, 0, desiredVelocity.y);
        float localacceleration = desiredVelocity.magnitude > .1 ? acceleration : dragNoInput;
        body.velocity = Vector3.MoveTowards(body.velocity, adjustedAxis * maxSpeed, localacceleration * Time.deltaTime);
    }

    void  UpdateInputs()
    {
        float leftX = -Input.GetAxis("Left Horizontal");
        float leftY = -Input.GetAxis("Left Vertical");
        float rightX = -Input.GetAxis("Right Horizontal");
        float rightY = -Input.GetAxis("Right Vertical");

        leftHandInput = new Vector2(leftX, leftY);
        rightHandInput = new Vector2(rightX, rightY);
    }
}
