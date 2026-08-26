using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LiquidWobble : MonoBehaviour
{
    public Rigidbody relevantBody;
    public float velocityEffect = 1;

    public float stiffness, damping;

    Vector3 velocity, rotation;

    void Update()
    {
        //get rigidbody force
        velocity += (-relevantBody.velocity * Time.deltaTime * velocityEffect);

        ApplySpring();

        Vector3 rotationAdjusted = new Vector3(rotation.x, 0, rotation.z);

        transform.localRotation =  Quaternion.Euler(rotationAdjusted);
    }

    void ApplySpring()
    {
        Vector2 velRotX = ApplySpring(velocity.x, rotation.x);
        Vector2 velRotY = ApplySpring(velocity.y, rotation.y);
        Vector2 velRotZ = ApplySpring(velocity.z, rotation.z);

        velocity = new Vector3(velRotX.x,velRotY.x,velRotZ.x);
        rotation = new Vector3(velRotX.y,velRotY.y,velRotZ.y);
    }

    Vector2 ApplySpring(float _velocity, float _rotation)
    {
        float displacement = _rotation;
        float springForce = -stiffness * displacement;
        float dampingForce = damping * _velocity;
        
        float totalAcceleration = springForce - dampingForce;

        _velocity += totalAcceleration * Time.deltaTime;
        _rotation += _velocity * Time.deltaTime;

        return new Vector2(_velocity, _rotation);
    }

}
