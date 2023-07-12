using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownMovement : MonoBehaviour
{
    [SerializeField] private Vector2 speed;

    private Rigidbody2D _rb;
    private Vector2 _targetDirection;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();  
    }

    // FixedUpdate is called once per physics update
    private void FixedUpdate()
    {
        _rb.AddForce(new Vector2(_targetDirection.x * speed.x, _targetDirection.y * speed.y), ForceMode2D.Impulse);
    }

    public void UpdateMove(Vector2 direction)
    {
        _targetDirection = direction;
    }

    public Vector2 GetMove()
    {
        return _targetDirection;
    }
}
