using System;
using UnityEngine;

public class PlayerFreeMoveRigidbodyView : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D _rb;
    
    public void SetVelocity(Vector2 velocity)
    {
        _rb.linearVelocity = velocity;
    }

    public void SetWeight(float weight)
    {
        _rb.mass = weight;
    }

    public Vector2 GetVelocity()
    {
        return _rb.linearVelocity;
    }

    public void AddForce(Vector2 force, ForceMode2D forceMode)
    {
        _rb.AddForce(force, forceMode);
    }

}