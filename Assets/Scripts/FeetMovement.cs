using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetMovement : MonoBehaviour
{
    //Input
    Vector2 rightFootInput, leftFootInput;
    bool desiresRaiseCrotch, desiresLowerCrotch;
    float crotchRotateInput;
    public Transform crotch;

    public Rigidbody rightFootRB,leftFootRB, crotchRB;
    public Transform rightLegBase, leftLegBase;

    public float acceleration, maxSpeed, dragNoInput;

    public bool grounded;

    public FootCode leftFoot,rightFoot;
    float leftLegExtension,rightLegExtension;


    void Update()
    {
        UpdateInputs();
        RotateCrotch(5, 2);
    }

    void FixedUpdate()
    {
        MoveFeet();
        UpdateGrounded();
        MoveCrotch();
    }

    void RotateCrotch(float maxRotation, float rotateSpeed)
    {
        Quaternion desiredRotation = Quaternion.Euler(0,0,maxRotation * crotchRotateInput);
        crotch.rotation = Quaternion.Lerp(crotch.rotation, desiredRotation, rotateSpeed * Time.deltaTime);
    }

    void MoveCrotch()
    {
        if(!grounded)
        {
           UpdateCrotchVelocity(0,-.15f);
        }
        else
        {
            if(desiresRaiseCrotch)
            {
                UpdateCrotchVelocity(0, .4f);
            }
            else if(crotchRB.velocity.y > 0f || desiresLowerCrotch)
            {
                UpdateCrotchVelocity(0, -.25f);
            }
            else if(crotchRB.velocity.y < 0)
            {
                UpdateCrotchVelocity(0, .25f);
            }
        }
    }

    void UpdateCrotchVelocity(float x, float y)
    {
        Vector3 crotchVelocity = crotchRB.velocity;
        crotchVelocity.y += Time.deltaTime * y;
        crotchVelocity.x += Time.deltaTime * x;
        crotchRB.velocity= crotchVelocity;
    }

    void MoveFeet()
    {
        AdjustBodyVelocity(rightFootRB, rightFootInput);
        AdjustBodyVelocity(leftFootRB, leftFootInput);
        AddLegTension(leftFootRB);
        AddLegTension(rightFootRB);

        rightLegExtension = Vector3.Distance(rightLegBase.position, rightFoot.gameObject.transform.position);
        leftLegExtension = Vector3.Distance(leftLegBase.position, leftFoot.gameObject.transform.position);
    }

    void AddLegTension(Rigidbody body)
    {
        float distanceToCrotch = crotch.position.y - body.position.y;
        float minForceDistance = .6f;
        float maxForceDistance = 0f;
        float downForce = 9f;

        distanceToCrotch= Mathf.Clamp(distanceToCrotch, maxForceDistance, minForceDistance);
        float percentForce = 1- (distanceToCrotch/minForceDistance);
        Vector3 footVelocity = body.velocity;
        footVelocity.y -= (percentForce * downForce * Time.deltaTime);
        body.velocity = footVelocity;
    }

    void UpdateGrounded()
    {
        grounded = rightFoot.OnGround || leftFoot.OnGround;
    }

    void AdjustBodyVelocity(Rigidbody body, Vector2 desiredVelocity)
    {
        float localacceleration = desiredVelocity.magnitude > .1 ? acceleration : dragNoInput;
        body.velocity = Vector3.MoveTowards(body.velocity, desiredVelocity * maxSpeed, localacceleration * Time.deltaTime);
    }

    void  UpdateInputs()
    {
        float leftX = -Input.GetAxis("Left Horizontal");
        float leftY = Input.GetAxis("Left Vertical") * .5f;
        float rightX = -Input.GetAxis("Right Horizontal");
        float rightY = Input.GetAxis("Right Vertical") * .5f;

        leftFootInput = new Vector2(leftX, leftY);
        rightFootInput = new Vector2(rightX, rightY);

        //Crotch Raising
        bool leftRaise = leftFoot.OnGround && rightY < -0.3f && leftLegExtension <.81f;
        bool rightRaise = rightFoot.OnGround && leftY < -0.3f && rightLegExtension <.81f;
        desiresRaiseCrotch = (leftRaise || rightRaise);

        //Crotch Lowering
        bool leftTooTall= leftFoot.OnGround && leftLegExtension > .815f;
        bool leftStretchDown= !leftFoot.OnGround && (rightY < 0 || rightX < 0 ) && leftLegExtension >.78;
        bool rightTooTall= rightFoot.OnGround && rightLegExtension > .815f;
        bool rightStretchDown = !rightFoot.OnGround && (leftY < 0 || leftX > 0 ) && rightLegExtension >.77;;
        
        bool leftLower = leftStretchDown || leftTooTall;
        bool rightLower = rightStretchDown || rightTooTall;

        desiresLowerCrotch = leftLower || rightLower;

        crotchRotateInput = 0;
        if(leftRaise || rightStretchDown) crotchRotateInput -= 1;
        if(rightRaise || leftStretchDown) crotchRotateInput +=1;
    }
}