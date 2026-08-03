using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beer : MonoBehaviour
{
    Rigidbody body;
    public BoxCollider collider;
    public CapsuleCollider collider2;
    bool isGrabbed = false;

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }
    public void OnGrab()
    {
        body.isKinematic = true;
        collider.enabled = false;
        collider2.enabled = false;
        isGrabbed = true;
    }

    void FixedUpdate()
    {
        if(!isGrabbed) return;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
