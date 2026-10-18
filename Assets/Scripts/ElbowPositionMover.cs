using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElbowPositionMover : MonoBehaviour
{
    public Transform beer;
    public float minDistancetoBeer = 0.25f;
    
    Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Vector3 targetPosition = startPosition;
        float yDistanceToBeer = Mathf.Abs(beer.position.y - transform.position.y);
        if(yDistanceToBeer < minDistancetoBeer)
        {
            if(beer.position.y + minDistancetoBeer > startPosition.y)
            targetPosition.y = beer.position.y + minDistancetoBeer;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }
}
