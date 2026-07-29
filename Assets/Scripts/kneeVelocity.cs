using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kneeVelocity : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 angularVelocity;
    Rigidbody rb;
    public Transform targetPosition;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.position = targetPosition.position;
        transform.rotation = targetPosition.rotation;
    }

    void FixedUpdate()
    {
        rb.MovePosition(targetPosition.position);
        rb.MoveRotation(targetPosition.rotation);
        velocity = rb.velocity;
        angularVelocity = rb.angularVelocity;
    }
}
