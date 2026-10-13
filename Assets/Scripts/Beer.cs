using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beer : MonoBehaviour
{
    public Rigidbody body;
    public BoxCollider collider;

    bool isGrabbed = false;
    public bool OnGround;
    public GroundedCheck groundCheck;

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public void OnDrop()
    {
        body.isKinematic = false;
        collider.enabled = true;
        isGrabbed = false;
    }
    
    public void OnGrab()
    {
        body.isKinematic = true;
        collider.enabled = false;
        isGrabbed = true;
    }

    void FixedUpdate()
    {
        OnGround = groundCheck.OnGround;

        if(!isGrabbed) return;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

}
