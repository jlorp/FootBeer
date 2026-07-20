using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jellyfier : MonoBehaviour
{
    public float bounceSpeed;
    public float fallForce;
    public float stiffness;

    private MeshFilter meshFilter;
    private Mesh mesh;

    JellyVertex[] jellyVerticies;
    Vector3[] currentMeshVerticies;

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
            jellyVerticies[i] = new JellyVertex(i, mesh.vertices[i], mesh.vertices[i], Vector3.zero);
            currentMeshVerticies[i] = mesh.vertices[i];
        }
    }

    private void Update()
    {
        UpdateVertices();
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
            Vector3 inputPoint = collisionPoints[i].point + (collisionPoints[i].point * .1f);
            ApplyPressureToPoint(inputPoint, fallForce);
        }
    }

    public void ApplyPressureToPoint(Vector3 _point, float _pressure)
    {
        for(int i = 0; i < jellyVerticies.Length; i++)
        {
            jellyVerticies[i].ApplyPressureToVertex(transform, _point, _pressure);
        }
    }
}
