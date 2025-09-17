using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement")]
    private Rigidbody2D rb;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 70f;
    [SerializeField] private float friction = 70f;
    [SerializeField] private float jumpForce = 30f;
    private float xAxis;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float boost = 140f;


    private InputAction moveAction;
    private InputAction jumpAction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        moveAction.Enable();
        jumpAction.Enable();
    }

    void Update()
    {
        GetInput();
        Move();
        Jump();
    }

    void GetInput()
    {
        xAxis = moveAction.ReadValue<Vector2>().x;
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(Math.Min(Mathf.Lerp(rb.linearVelocity.x, xAxis * maxSpeed, acceleration * Time.deltaTime), maxSpeed), rb.linearVelocity.y);
        if (xAxis == 0 || rb.linearVelocity.x > maxSpeed || rb.linearVelocity.x < -maxSpeed)
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, friction * Time.deltaTime), rb.linearVelocity.y);
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckY, groundLayer)
            || Physics2D.Raycast(groundCheck.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, groundLayer)
            || Physics2D.Raycast(groundCheck.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, groundLayer);
    }

    void Jump()
    {
        if (!jumpAction.IsPressed() && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, 0);
        }

        if (jumpAction.triggered && IsGrounded())
        {
            float boostToAdd = 0;
            if(xAxis != 0)
            {
                boostToAdd = Math.Sign(xAxis) * boost;
            }

            rb.linearVelocity = new Vector3(rb.linearVelocity.x + boostToAdd, jumpForce, 0);
        }
    }
}