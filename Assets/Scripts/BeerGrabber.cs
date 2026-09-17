using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeerGrabber : MonoBehaviour
{
    public ArmLogic arm;
    public float maxGrabDistance;
    public Transform childthing;

    float timeSinceTouch;

    public Rigidbody rFoot, lFoot;

    void FixedUpdate()
    {
        timeSinceTouch +=Time.deltaTime;
    }

    void OnTriggerEnter(Collider other) 
    {
        if(CameraManager.Instance.timeInScene < 1) return;

        if (other.TryGetComponent<Beer>(out Beer beer))
        {
            Vector3 beerLocalPosition = childthing.InverseTransformPoint(other.transform.position);

            bool beerinRange = Mathf.Abs(beerLocalPosition.y) <= maxGrabDistance;

            //Debug.Log(beerLocalPosition.y);
            if(beerinRange && arm.wristExtending)
            {
                GrabBeer(beer);
            }
            else
            {
                FlickBeer(beer, beerLocalPosition.y);
            }
        }
    }

    void FlickBeer(Beer beer, float beerDirectionY)
    {
        if(timeSinceTouch < 1) return;

        AudioManager.Instance.PlaySound(AudioManager.Instance.canKickSounds, 0.5f, 1f, transform.position);

        SpreadFeet(beer);

        if(arm.wristExtending)
        {
            beer.body.velocity = -arm.handPositionTarget.right * 0.25f;
            beer.body.angularVelocity= new Vector3(0,0,Mathf.Sign(beerDirectionY) * -6f);
        }
        else
        {
           beer.body.velocity = arm.handPositionTarget.right *0.25f;
           beer.body.angularVelocity= new Vector3(0,0,Mathf.Sign(beerDirectionY) * 6f);
        }
        arm.StartReturnWrist();
        timeSinceTouch = 0;

    }

    void SpreadFeet(Beer beer)
    {
        Vector3 beerDirectionR = (rFoot.transform.position - beer.transform.position).normalized;
        Vector3 beerDirectionL = (lFoot.transform.position - beer.transform.position).normalized;

        float pokeVelocity = 0.85f;

        rFoot.velocity = beerDirectionR * pokeVelocity;
        lFoot.velocity = beerDirectionL * pokeVelocity;
    }

    void GrabBeer(Beer beer)
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
