using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyVertex : MonoBehaviour
{
    public int verticeIndex;
    public Vector3 initialVertexPosition;
    public Vector3 currentVertexPosition;
    public float vertRedAmount;

    public Vector3 currentVelocity;

    public JellyVertex(int _verticeIndex, Vector3 _initialVertexPosition, Vector3 _currentVertexPosition, Vector3 _currentVelocity, Color _vertRedAmount)
    {
        verticeIndex = _verticeIndex;
        initialVertexPosition = _initialVertexPosition;
        currentVertexPosition = _currentVertexPosition;
        currentVelocity = _currentVelocity;
        vertRedAmount = _vertRedAmount.r;
    }

    public Vector3 GetCurrentDisplacement()
    {
        return currentVertexPosition - initialVertexPosition;
    }

    public void UpdateVelocity(float _bounceSpeed)
    {
        currentVelocity = currentVelocity - GetCurrentDisplacement() * _bounceSpeed * Time.deltaTime;
    }

    public void Settle(float _stiffness)
    {
        currentVelocity *= 1f - _stiffness * Time.deltaTime;
    }
    
    public void ApplyPressureToFullForm(Vector3 _force)
    {
        currentVelocity += _force * vertRedAmount;
    }
    public void ApplyAngularPressureToFullForm(Rigidbody _body)
    {
        Vector3 velocityAtPosition = _body.GetPointVelocity(_body.transform.TransformPoint(initialVertexPosition)) - _body.velocity;
        currentVelocity += velocityAtPosition * vertRedAmount * Time.deltaTime * -2f;
    }

    public void ApplyPressureToVertex(Transform _transform, Vector3 _position, float _pressure)
    {
        Vector3 distanceVertexPoint = currentVertexPosition - _transform.InverseTransformPoint(_position);
        float adaptedPressure = _pressure/(1f + distanceVertexPoint.sqrMagnitude);
        float velocity = adaptedPressure * Time.deltaTime;
        currentVelocity += distanceVertexPoint.normalized * velocity;
    }
}
