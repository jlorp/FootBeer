using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeerSensor : MonoBehaviour
{
    public HandMover _hands;
    public bool fired = false;

    public void Reset()
    {
        fired = false;
    }

    void OnTriggerEnter(Collider other) 
    {
        if (fired) return;
        _hands.OnPoke(true);
        fired = true;
    }
}
