using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OrientationConstraint : MonoBehaviour
{
    public Transform bone;
    public Vector3 offset;
    public float rotationSpeed;

    void LateUpdate()
    {
        bone.rotation = Quaternion.Lerp(bone.rotation, transform.rotation * Quaternion.Euler(offset), Time.deltaTime * rotationSpeed);
    }
}
