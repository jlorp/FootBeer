using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchPhysics : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> bodies;
    public Rigidbody body;
    public float grip = 5f;
    public float heldGravity = -.1f;

    void OnTriggerEnter(Collider other)
    {
        if(other.transform.TryGetComponent<Rigidbody>(out Rigidbody body))
        {
            if(!bodies.Contains(body))
            {
                bodies.Add(body);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.transform.TryGetComponent<Rigidbody>(out Rigidbody body))
        {
            if(bodies.Contains(body))
            {
                bodies.Remove(body);
            }
        }
    }

    void FixedUpdate()
    {
        if(bodies.Count == 2)
        {
            Vector3 averageVelocity = (bodies[1].velocity + bodies[0].velocity)/1.8f;
            body.velocity = Vector3.MoveTowards(body.velocity, averageVelocity, grip * Time.deltaTime);

            body.velocity += (Vector3.up * heldGravity* Time.deltaTime);
            
            body.useGravity = false;

            foreach (var foot in bodies)
            {
                Vector3 directionToBeer = (foot.transform.position - transform.position).normalized;
                directionToBeer.y= 0;
                //foot.velocity += directionToBeer * Time.deltaTime * -10;
            }
        }
        else
        {
            body.useGravity = true;
        }
    }
}
