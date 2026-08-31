using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeerGrabber : MonoBehaviour
{
    public ArmLogic arm;

    void OnTriggerEnter(Collider other) 
    {
        if (other.TryGetComponent<Beer>(out Beer beer))
        {
            beer.OnGrab();
            beer.transform.SetParent(this.transform);
            beer.transform.localRotation = Quaternion.identity;
            beer.transform.localPosition = Vector3.zero;
            arm.holdingBeer = true;
            arm.armRenderer.SetBlendShapeWeight(0, 100f);
            AudioManager.Instance.PlaySound(AudioManager.Instance.canGrabSounds, 1, Random.Range(0.95f,1.05f), transform.position);
            //GameManager.Instance.PlayDialogue("YES!", .15f, 2f, 0);
        }
    }
}
