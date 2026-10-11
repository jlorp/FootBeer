using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedCheck : MonoBehaviour
{
    [SerializeField, Range(0f, 90f)] 
	float maxGroundAngle = 25f;

    float minGroundDotProduct;
    int groundContactCount;
    public bool OnGround;
    public bool visuallyGrounded;
    float stepsSinceLastGrounded=0;

    public LayerMask groundMask;

    private Rigidbody rb;

    public Collider collider;
    public float skinHeight;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
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
            normal.z = 0;
            normal = normal.normalized;
			float upDot = Vector3.Dot(Vector3.up, normal);
            if (upDot >= minDot && isOnGroundLayer) 
            {
				groundContactCount += 1;
			}
		}
    }

    void VisualGroundCheck()
    {
        Transform _camera= CameraManager.Instance.legCam.transform;

        Vector3 lowestY = collider.bounds.min;
        lowestY.x = transform.position.x;
        lowestY.z = transform.position.z;
        lowestY += _camera.forward * -.5f;
        lowestY += skinHeight * _camera.up;

        RaycastHit hit;

        if (Physics.Raycast(lowestY , _camera.forward, out hit, Mathf.Infinity, groundMask))
        {
            visuallyGrounded = true;
            Debug.DrawRay(lowestY, _camera.forward, Color.green);
        }
        else
        {
            visuallyGrounded = false;
            Debug.DrawRay(lowestY, _camera.forward, Color.red);
        }
    }

    void FixedUpdate()
    {
        stepsSinceLastGrounded += 1;
        if(OnGround) stepsSinceLastGrounded = 0;

        if (rb.IsSleeping()) 
        {
            groundContactCount = 1;
        }

        OnGround = (groundContactCount > 0);
    
        ClearState();
        VisualGroundCheck();
    }

    void ClearState()
    {
        groundContactCount=0;
    }
}
