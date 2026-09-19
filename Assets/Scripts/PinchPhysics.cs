using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchPhysics : MonoBehaviour
{
    [SerializeField] private List<FootCode> Feet;
    public Rigidbody body;
    public float grip = 5f;
    public float heldGravity = -.1f;

    float foot1Pinch, foot2Pinch;

    Vector3 directionToFoot1, directionToFoot2;

    void OnTriggerEnter(Collider other)
    {
        if(other.transform.TryGetComponent<FootCode>(out FootCode foot))
        {
            Feet.Add(foot);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.transform.TryGetComponent<FootCode>(out FootCode foot))
        {
            Feet.Remove(foot);
        }
    }

    void CheckFeetPinching()
    {
        directionToFoot2 = (Feet[0].transform.position - transform.position);
        directionToFoot2.z=0;

        foot2Pinch = Vector3.Dot(directionToFoot2, Feet[0].movementIntent);

        directionToFoot1 = (Feet[1].transform.position - transform.position);
        directionToFoot1.z=0;

        foot1Pinch = Vector3.Dot(directionToFoot1, Feet[1].movementIntent);
    }

    void DampenVelocityTowardsBeer()
    {
        DampenVelocity(Feet[0].body, directionToFoot1);
        DampenVelocity(Feet[1].body, directionToFoot2);
    }

    void DampenVelocity(Rigidbody body, Vector3 direction)
    {
        Vector3 _velocity = body.velocity;   
        direction = direction.normalized;

        if (direction.x < 0 && _velocity.x < 0)
        {
            _velocity.x = _velocity.x * Mathf.Abs(direction.x/2);
        }
        else if (direction.x > 0 && _velocity.x > 0)
        {
            _velocity.x = _velocity.x * Mathf.Abs(direction.x/2);
        }

        body.velocity = _velocity;
    }

    void FixedUpdate()
    {
        if(Feet.Count == 2)
        {
            CheckFeetPinching();

            body.velocity += (Vector3.up * heldGravity* Time.deltaTime);

            float pinchAmount = (foot2Pinch + foot1Pinch)/2;

            Vector3 averageVelocity = (Feet[0].body.velocity + Feet[1].body.velocity)/1.8f;
            body.velocity = Vector3.MoveTowards(body.velocity, averageVelocity, grip * Time.deltaTime * pinchAmount);

            DampenVelocityTowardsBeer();
            
            body.useGravity = false;
        }
        else
        {
            body.useGravity = true;
        }
    }
}
