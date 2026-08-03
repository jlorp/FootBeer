using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeerGrabber : MonoBehaviour
{
    void OnTriggerEnter(Collider other) 
    {
        if (other.TryGetComponent<Beer>(out Beer beer))
        {
            beer.OnGrab();
            beer.transform.SetParent(this.transform);
            beer.transform.localRotation = Quaternion.identity;
            beer.transform.localPosition = Vector3.zero;
        }
    }
}
