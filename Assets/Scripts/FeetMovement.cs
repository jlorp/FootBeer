using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetMovement : MonoBehaviour
{
    [Header("Feet Movement")]
    public float acceleration;
    public float maxSpeed;
    public float dragNoInput;
    public FootCode leftFoot, rightFoot;
    public Transform rKneeBone, lKneeBone;
    public float footGravity;

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

    [Header("IK Handles")]
    public Transform rightIK;
    public Transform leftIK;
    public float restDistanceIK;
    public Transform upperLegL, upperLegR;

    [Header("Dependencies")]
    public bool grounded;
    public int feetGrounded= 0;
    public Rigidbody rightFootRB, leftFootRB, crotchRB;
    public ArmLogic arm;

    //Input
    float hitstunInputMultiplier = 1;
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
        KneeOrient();
    }

    void FixedUpdate()
    {
        if(!sceneActive) return;
        MoveFeet();
        RotateCrotch(5f, 2f);
        UpdateGrounded();
        MoveCrotch();
    }

    void KneeOrient()
    {
        Vector3 rKneeDirection = (rightFoot.transform.position - upperLegR.position).normalized;
        rKneeDirection.z=0;
        rKneeDirection = Quaternion.Euler(0, 0, 90) * rKneeDirection;

        Vector3 middlePointR = (rightFoot.transform.position + upperLegR.position)/2;
        rightIK.position = middlePointR + (rKneeDirection * restDistanceIK);

        Vector3 lKneeDirection = (leftFoot.transform.position - upperLegL.position).normalized;
        lKneeDirection.z=0;
        lKneeDirection = Quaternion.Euler(0, 0, -90) * lKneeDirection;

        Vector3 middlePointL = (leftFoot.transform.position + upperLegL.position)/2;
        leftIK.position = middlePointL + (lKneeDirection * restDistanceIK);
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
        if(feetGrounded == 0)
        {
            crotchRB.velocity += (-Vector3.up * crotchFallspeed * Time.deltaTime);
        }
        else if (feetGrounded == 1)
        {
            crotchRB.velocity += (-Vector3.up * crotchFallspeed * Time.deltaTime * 0.25f);
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
        AdjustBodyVelocity(rightFootRB, new Vector2(rightFootInput.x, Mathf.Clamp(rightFootInput.y, -1f, 0.66f)));
        AdjustBodyVelocity(leftFootRB, new Vector2(leftFootInput.x, Mathf.Clamp(leftFootInput.y, -1f, 0.66f)));


        leftFootRB.velocity += (footGravity + crotchRB.velocity.y) * Vector3.up * Time.deltaTime;
        rightFootRB.velocity += (footGravity + crotchRB.velocity.y) * Vector3.up * Time.deltaTime;

        rightFoot.movementIntent = rightFootInput;
        leftFoot.movementIntent = leftFootInput;

        AddLegTension(leftFootRB, -1);
        AddLegTension(rightFootRB, 1);
    }

    void AddLegTension(Rigidbody body, float xDirection)
    {
        Vector3 footVelocity = body.velocity;

        float distanceToCrotchY = crotch.position.y - body.position.y;
        float minForceDistance = -0.1f;
        float maxForceDistance = 0.1f;
        float downForce = 9f;

        distanceToCrotchY= Mathf.Clamp(distanceToCrotchY, minForceDistance, maxForceDistance);
        float fullRange = maxForceDistance - minForceDistance;
        float percentForce =  1- ((distanceToCrotchY - minForceDistance) / fullRange);

        footVelocity.y -= (percentForce * downForce * Time.deltaTime);
        body.velocity = footVelocity;
    }

    void UpdateGrounded()
    {
        grounded = rightFoot.OnGround || leftFoot.OnGround;
        feetGrounded=0;
        if (rightFoot.OnGround) feetGrounded +=1;
        if (leftFoot.OnGround) feetGrounded +=1;
    }

    void AdjustBodyVelocity(Rigidbody body, Vector2 desiredVelocity)
    {
        float localaccelerationX = Mathf.Abs(desiredVelocity.x) > 0.1f ? acceleration : dragNoInput;
        float localaccelerationY = Mathf.Abs(desiredVelocity.y) > 0.1f ? acceleration : dragNoInput;
        
        if(hitstunInputMultiplier < 1) 
        {
            localaccelerationX = acceleration * hitstunInputMultiplier;
            localaccelerationY = acceleration * hitstunInputMultiplier;
        }

        Vector3 _velocity = new Vector3(
            Mathf.MoveTowards(body.velocity.x, desiredVelocity.x * maxSpeed, localaccelerationX * Time.deltaTime),
            Mathf.MoveTowards(body.velocity.y, desiredVelocity.y * maxSpeed, localaccelerationY * Time.deltaTime),
            0);
        
        body.velocity = _velocity;
    }

    public void OnHitStun(float stunDuration, AnimationCurve _curve)
    {
        StartCoroutine(InputDelay(stunDuration, _curve));
    }

    IEnumerator InputDelay(float duration, AnimationCurve _curve)
    {
        float elapsedTime = 0f;
        hitstunInputMultiplier = 0;

        while (elapsedTime < duration)
        {
            float percentComplete = elapsedTime/duration;
            percentComplete = _curve.Evaluate(percentComplete);
            hitstunInputMultiplier = percentComplete;

            elapsedTime += Time.deltaTime;
            yield return null; 
        }
        hitstunInputMultiplier = 1;
    }

    void  UpdateInputs()
    {
        float leftX = -Input.GetAxis("Left Horizontal") * hitstunInputMultiplier;
        float leftY = Input.GetAxis("Left Vertical") * hitstunInputMultiplier;
        float rightX = -Input.GetAxis("Right Horizontal") * hitstunInputMultiplier;
        float rightY = Input.GetAxis("Right Vertical") * hitstunInputMultiplier;
        
        if(arm.holdingBeer) leftY = rightY = -2f;

        leftFootInput = new Vector2(leftX, leftY);
        rightFootInput = new Vector2(rightX, rightY);

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

        if(arm.holdingBeer && crotchMovementInput.y < 0.3f) crotchMovementInput = Vector3.up * 0.3f;
        if(arm.holdingBeer) crotchRotateInput = 0;
    }

    public void ResetPlayerPosition()
    {
        leftFootRB.MovePosition(lFootStartPosition);
        rightFootRB.MovePosition(rFootStartPosition);
        crotchRB.MovePosition(crotchStartPosition);

        leftFootRB.velocity = rightFootRB.velocity = crotchRB.velocity = Vector3.zero;
    }
}