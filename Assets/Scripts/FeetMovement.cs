using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetMovement : MonoBehaviour
{
    [Header("Feet Movement")]
    public float acceleration;
    public float maxSpeed;
    public float dragNoInput;
    public FootCode leftFoot,rightFoot;
    public Transform rKneeBone,lKneeBone;

    [Header("Crotch Rotation")]
    public Transform crotch;
    public Transform rightLegBase, leftLegBase;

    float crotchRotateInput;

    [Header("Crotch Movement")]
    Vector3 crotchMovementInput;
    public float crotchFallspeed = .1f;

    [Header("Foot Rotation")]
    public Transform rFootBone;
    public Transform lFootBone;

    public Vector3 lFootIdleRot, rFootIdleRot;
    public Vector3 rFootGroundedRotation,lFootGroundedRotation;

    [Header("Dependencies")]
    public bool grounded;
    public Rigidbody rightFootRB,leftFootRB, crotchRB;
    public ArmLogic arm;

    //Input
    Vector2 rightFootInput, leftFootInput;

    //reset
    Vector3 rFootStartPosition, lFootStartPosition, crotchStartPosition;

    public bool sceneActive = true;

    void Start()
    {
        lFootIdleRot = lFootBone.localEulerAngles;
        rFootIdleRot = rFootBone.localEulerAngles;
        
        lFootStartPosition = leftFootRB.transform.position;
        rFootStartPosition = rightFootRB.transform.position;
        crotchStartPosition = crotchRB.transform.position;
    }

    void Update()
    {
        if(!sceneActive) return;
        UpdateInputs();
        RotateFeet(5f);
    }

    void FixedUpdate()
    {
        if(!sceneActive) return;
        MoveFeet();
        RotateCrotch(5f, 2f);
        UpdateGrounded();
        MoveCrotch();
    }
    
    void RotateFoot(Vector3 idleRotationVector, bool isGrounded, Vector3 footVelocity, Transform foot, Transform knee, float rotateSpeed, Vector3 footGroundedRotation, float rightLeft)
    {
        Vector3 desiredRotationVector = idleRotationVector;
        float footExtensionAmount = Mathf.Clamp((Mathf.Abs(knee.localEulerAngles.z)-260)/100,0,1);
        

        if(isGrounded)
        {
            foot.rotation = Quaternion.Lerp(foot.rotation, Quaternion.Euler(footGroundedRotation), rotateSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 footPointRotation = new Vector3(19.2f, 33f, 64.8f);
            footPointRotation *= footExtensionAmount;
            desiredRotationVector += footPointRotation;

            desiredRotationVector.z += footVelocity.y * 50f;
            desiredRotationVector.y += footVelocity.x * 50f * rightLeft;

            foot.localRotation = Quaternion.Lerp(foot.localRotation, Quaternion.Euler(desiredRotationVector), rotateSpeed * Time.deltaTime);
        }
    }

    void RotateFeet(float rotateSpeed)
    {
        RotateFoot(rFootIdleRot, rightFoot.OnGround, leftFootRB.velocity, rFootBone, rKneeBone, rotateSpeed, rFootGroundedRotation, 1);
        RotateFoot(lFootIdleRot, leftFoot.OnGround, rightFootRB.velocity, lFootBone, lKneeBone, rotateSpeed, lFootGroundedRotation, -1);
    }

    void RotateCrotch(float maxRotation, float rotateSpeed)
    {
        Quaternion desiredRotation = Quaternion.Euler(0,0,maxRotation * crotchRotateInput);
        Quaternion newRotation = Quaternion.Lerp(crotch.rotation, desiredRotation, rotateSpeed * Time.deltaTime);
        crotchRB.MoveRotation(newRotation);
    }

    void MoveCrotch()
    {
        //add gravity if not grounded
        if(!grounded)
        {
            crotchMovementInput +=(Vector3.up * -crotchFallspeed);
        }
        else if(crotchMovementInput.y == 0)
        {
            crotchMovementInput.y = (-Mathf.Sign(crotchRB.velocity.y) * 0.1f);
        }

        UpdateCrotchVelocity(crotchMovementInput.x, crotchMovementInput.y);
    }

    void UpdateCrotchVelocity(float x, float y)
    {
        Vector3 crotchVelocity = crotchRB.velocity;
        crotchVelocity.y += Time.deltaTime * y;
        crotchVelocity.x += Time.deltaTime * x;
        crotchRB.velocity = crotchVelocity;
    }

    void MoveFeet()
    {
        AdjustBodyVelocity(rightFootRB, new Vector2(rightFootInput.x, rightFootInput.y *.5f));
        AdjustBodyVelocity(leftFootRB, new Vector2(leftFootInput.x, leftFootInput.y * .5f));
        AddLegTension(leftFootRB, -1);
        AddLegTension(rightFootRB, 1);
    }

    void AddLegTension(Rigidbody body, float xDirection)
    {
        Vector3 footVelocity = body.velocity;

        float distanceToCrotchY = crotch.position.y - body.position.y;
        float minForceDistance = .6f;
        float maxForceDistance = 0f;
        float downForce = 9f;

        distanceToCrotchY= Mathf.Clamp(distanceToCrotchY, maxForceDistance, minForceDistance);
        float percentForce = 1- (distanceToCrotchY/minForceDistance);

        footVelocity.y -= (percentForce * downForce * Time.deltaTime);

        float distanceToCrotchX = (crotch.position.x + .6f * xDirection) - body.position.x;
        float minForceDistanceX = .3f * xDirection;
        float maxForceDistanceX = 0f;
        float sideForce = 15f * -xDirection;
        if(minForceDistanceX < maxForceDistanceX)
        {
            maxForceDistanceX = minForceDistanceX;
            minForceDistanceX = 0;
        }

        distanceToCrotchX = Mathf.Clamp(distanceToCrotchX, maxForceDistanceX, minForceDistanceX);
        
        float percentForceX = 1- (Mathf.Abs(distanceToCrotchX)/Mathf.Abs(maxForceDistanceX - minForceDistanceX));
        float xForceCalculated = (percentForceX * sideForce * Time.deltaTime);
        footVelocity.x += xForceCalculated;

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
        float leftY = Input.GetAxis("Left Vertical");
        float rightX = -Input.GetAxis("Right Horizontal");
        float rightY = Input.GetAxis("Right Vertical");
        

        leftFootInput = new Vector2(leftX, leftY);
        rightFootInput = new Vector2(rightX, rightY);

        if(arm.holdingBeer) leftFootInput = rightFootInput = -Vector3.up;

        //Crotch Raising
        bool leftRaise = leftFoot.OnGround && rightY < -0.3f && !leftFoot.atLimit;
        bool rightRaise = rightFoot.OnGround && leftY < -0.3f && !rightFoot.atLimit;

        //crotch move input
        float crotchMoveSpeedLimit =.2f;

        float leftFootLimitDot = Mathf.Clamp(Vector3.Dot(-leftFoot.directionToAnchor, rightFootInput), 0, 1);
        Vector3 leftFootCrotchInput = -leftFoot.directionToAnchor * leftFootLimitDot * crotchMoveSpeedLimit * 2f;

        if(!leftFoot.atLimit) leftFootCrotchInput = Vector3.zero;
        if(leftRaise) leftFootCrotchInput = (leftFoot.directionToAnchor + (Vector3.up*.5f))* crotchMoveSpeedLimit;


        float rightFootLimitDot = Mathf.Clamp(Vector3.Dot(-rightFoot.directionToAnchor, leftFootInput), 0, 1);
        Vector3 rightFootCrotchInput = -rightFoot.directionToAnchor * rightFootLimitDot * crotchMoveSpeedLimit  * 2f;
 
        if(!rightFoot.atLimit) rightFootCrotchInput = Vector3.zero;
        if(rightRaise) rightFootCrotchInput = (rightFoot.directionToAnchor + Vector3.up*.5f) * crotchMoveSpeedLimit;

        crotchMovementInput =  leftFootCrotchInput + rightFootCrotchInput;
        crotchRotateInput = (rightFootCrotchInput.y - leftFootCrotchInput.y) / crotchMoveSpeedLimit;

        if(arm.holdingBeer) crotchMovementInput = Vector3.up * .75f;
    }

    public void ResetPlayerPosition()
    {
        leftFootRB.MovePosition(lFootStartPosition);
        rightFootRB.MovePosition(rFootStartPosition);
        crotchRB.MovePosition(crotchStartPosition);

        leftFootRB.velocity = rightFootRB.velocity = crotchRB.velocity = Vector3.zero;
    }
}