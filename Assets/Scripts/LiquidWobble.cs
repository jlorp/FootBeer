using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LiquidWobble : MonoBehaviour
{
    public Rigidbody relevantBody;
    public float velocityEffect = 1;

    public float stiffness, damping;

    public float yDampen, yVelocityEffect;

    Vector3 velocity, rotation;

    public Transform yChild;

    void Update()
    {
        //get rigidbody force
        velocity += (-relevantBody.velocity * Time.deltaTime * velocityEffect);
        velocity.y +=(relevantBody.velocity.x + relevantBody.velocity.z) * Time.deltaTime * yVelocityEffect;

        ApplySpring();

        Vector3 rotationAdjusted = new Vector3(rotation.x, 0, rotation.z);

        transform.localRotation =  Quaternion.Euler(rotationAdjusted);
        yChild.localRotation = Quaternion.Euler(new Vector3(0,rotation.y,0));
    }

    void ApplySpring()
    {
        Vector2 velRotX = ApplySpring(velocity.x, rotation.x);

        Vector2 velRotY = ApplyDampen(velocity.y, rotation.y, yDampen);

        Vector2 velRotZ = ApplySpring(velocity.z, rotation.z);

        velocity = new Vector3(velRotX.x,velRotY.x,velRotZ.x);
        rotation = new Vector3(velRotX.y,velRotY.y,velRotZ.y);
    }

    Vector2 ApplyDampen(float _velocity, float _rotation, float _damping)
    {
        float dampingForce = _damping * _velocity;

        _velocity -= dampingForce * Time.deltaTime;
        _rotation += _velocity * Time.deltaTime;

        return new Vector2(_velocity, _rotation);
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
