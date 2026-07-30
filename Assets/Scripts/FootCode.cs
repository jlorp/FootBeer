using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCode : MonoBehaviour
{
    [SerializeField, Range(0f, 90f)] 
	float maxGroundAngle = 25f;
    ConfigurableJoint joint;

    public bool atLimit;
    public Vector3 directionToAnchor;

    float minGroundDotProduct;
    int groundContactCount;
    public bool OnGround;
    float stepsSinceLastGrounded=0;

    public LayerMask groundMask;

    void Start()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        joint = GetComponent<ConfigurableJoint>();
    }



	void OnCollisionStay (Collision collision) 
    {
		EvaluateCollision(collision);
	}

    void EvaluateCollision (Collision collision) 
    {
        float minDot = minGroundDotProduct;
        for (int i = 0; i < collision.contactCount; i++)
        {
            bool isOnGroundLayer = (((1 << collision.collider.gameObject.layer) & groundMask) != 0);
            
			Vector3 normal = collision.GetContact(i).normal;
			float upDot = Vector3.Dot(Vector3.up, normal);
            if (upDot >= minDot && isOnGroundLayer) 
            {
				groundContactCount += 1;
			}
		}
    }

    void FixedUpdate()
    {
        stepsSinceLastGrounded += 1;
        if(OnGround) stepsSinceLastGrounded = 0;

        OnGround = (groundContactCount > 0);

        Vector3 footPosition = transform.TransformPoint(joint.anchor);
        Vector3 anchorPosition = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);
        float distanceToAnchor = Vector3.Distance(footPosition,anchorPosition);
        atLimit =  (distanceToAnchor) >= joint.linearLimit.limit;

        directionToAnchor = (anchorPosition-footPosition).normalized;
        
        ClearState();
    }

    void ClearState()
    {
        groundContactCount=0;
    }
}
