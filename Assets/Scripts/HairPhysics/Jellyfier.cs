using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jellyfier : MonoBehaviour
{
    [Header("Hair Simulation")]
    public float bounceSpeed;
    public float stiffness;
    public float hairMovement = .75f;

    [Header("Hair Sim: Noise")]
    public float scale = 5;
    public float scrollSpeed = 1;
    public float positionEffect = .25f;

    public Rigidbody body;

    private MeshFilter meshFilter;
    private Mesh mesh;

    JellyVertex[] jellyVerticies;
    Vector3[] currentMeshVerticies;

    public List<Rigidbody> outsideForceList = new List<Rigidbody>();


    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        GetVertices();
    }

    private void GetVertices()
    {
        jellyVerticies = new JellyVertex[mesh.vertices.Length];
        currentMeshVerticies = new Vector3[mesh.vertices.Length];

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            jellyVerticies[i] = new JellyVertex(i, mesh.vertices[i], mesh.vertices[i], Vector3.zero, mesh.colors[i]);
            currentMeshVerticies[i] = mesh.vertices[i];
        }
    }

    private void Update()
    {
        ApplyPressureToFullForm(-body.velocity * Time.deltaTime * hairMovement);
        ApplyNoise(scale, scrollSpeed, positionEffect);
        UpdateVertices();

        foreach (Rigidbody body in outsideForceList)
        {
            ApplyPressureToPoint(body.transform.position, body.velocity * Time.deltaTime);
        }
    }

    private void UpdateVertices()
    {
        for(int i = 0; i < jellyVerticies.Length; i++)
        {
            jellyVerticies[i].UpdateVelocity(bounceSpeed);
            jellyVerticies[i].Settle(stiffness);

            jellyVerticies[i].currentVertexPosition += jellyVerticies[i].currentVelocity * Time.deltaTime;
            currentMeshVerticies[i] = jellyVerticies[i].currentVertexPosition;
        }

        mesh.vertices = currentMeshVerticies;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    public void OnCollisionEnter(Collision other)
    {
        ContactPoint[] collisionPoints = other.contacts;
        for (int i=0; i < collisionPoints.Length; i++)
        {
            ApplyPressureToFullForm(-other.relativeVelocity * .25f);
        }
    }

    public void ApplyPressureToFullForm(Vector3 _force)
    {
        Vector3 force_localized = transform.InverseTransformDirection(_force);

        for(int i = 0; i < jellyVerticies.Length; i++)
        {
            jellyVerticies[i].ApplyPressureToFullForm(force_localized);
        }
    }

    public void ApplyNoise(float _scale, float _scrollSpeed, float _positionEffect)
    {
        for(int i = 0; i < jellyVerticies.Length; i++)
        {
            jellyVerticies[i].ApplyNoise(_scale, _scrollSpeed, _positionEffect);
        }
    }   

    public void ApplyPressureToPoint(Vector3 _point, Vector3 _pressure)
    {
        Vector3 pressure_localized = transform.InverseTransformDirection(_pressure);

        for(int i = 0; i < jellyVerticies.Length; i++)
        {
            jellyVerticies[i].ApplyPressureToVertex(transform, _point, pressure_localized * 200);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HairEffector>(out HairEffector effector))
        {
            if(effector.body != body) outsideForceList.Add(effector.body); 
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<HairEffector>(out HairEffector effector))
        {
            if (outsideForceList.Contains(effector.body)) outsideForceList.Remove(effector.body);
        }
    }
}
