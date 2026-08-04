using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeerGrabber : MonoBehaviour
{
    public ArmLogic _arm;

    void OnTriggerEnter(Collider other) 
    {
        if (other.TryGetComponent<Beer>(out Beer beer))
        {
            beer.OnGrab();
            beer.transform.SetParent(this.transform);
            beer.transform.localRotation = Quaternion.identity;
            beer.transform.localPosition = Vector3.zero;
            _arm.holdingBeer=true;
            GameManager.Instance.PlayDialogue("Y E S!", .15f, 2f, 0);
        }
    }
}
