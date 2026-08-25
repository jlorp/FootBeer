using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppableBeer : MonoBehaviour
{
    public float gravity;
    public Rigidbody body;
    public Vector3 handDirection;

    public float sceneChangeHeightThreshold = 0.5f;

    public float initialYVelociy;

    void FixedUpdate()
    {
        Vector3 _velocity = body.velocity;
        _velocity.y += gravity * Time.deltaTime;
        body.velocity = _velocity;

        if(transform.position.y < sceneChangeHeightThreshold) ChangeScene();
    }

    public void SetAngularVelocity(Vector3 _angularVelocity)
    {
        body.angularVelocity = new Vector3(-_angularVelocity.z, 0.25f, _angularVelocity.x) * 5f;
    }

    void Awake()
    {
        Vector3 startVelocity = Vector3.zero;
        startVelocity.y = initialYVelociy;
        body.velocity = startVelocity;
    }

    void ChangeScene()
    {
        CameraManager.Instance.SwitchCamera(1, true);
        Destroy(this.gameObject);
        GameManager.Instance.PlayDialogue("oh come on", .15f, 2.5f, 0f);
    }
}
