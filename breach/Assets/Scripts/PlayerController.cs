using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement")]
    private Rigidbody2D rb;
    private Vector3 _respawnPoint;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float friction = 60f;
    [SerializeField] private float jumpForce = 25f;
    private float xAxis;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckY = 0.3f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float boost = 20f;
    [SerializeField] private bool _active = true;

    private InputAction moveAction;
    private InputAction jumpAction;
    private Collider2D _collider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        _collider = GetComponent<Collider2D>();

        moveAction.Enable();
        jumpAction.Enable();
    }

    void Update()
    {
        GetInput();
        Move();
        Jump();
        if (!_active)
        {
            return;
        }
    }

    void GetInput()
    {
        xAxis = 1 * Math.Sign(moveAction.ReadValue<Vector2>().x);
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
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.67f, 0);
        }

        if (jumpAction.triggered && IsGrounded())
        {


            float boostToAdd = 0;
            if (xAxis != 0)
            {
                boostToAdd = Math.Sign(xAxis) * boost;
            }

            rb.linearVelocity = new Vector3(rb.linearVelocity.x + boostToAdd, jumpForce, 0);
        }
    }
    public IEnumerator die()
    {
        _active = false;
        _collider.enabled = false;
        yield return new WaitForSeconds(0.75f);
        transform.position = _respawnPoint;
        _active = true;
        _collider.enabled = true;
    }

    public void SetRespawnPoint(Vector3 newPoint)
    {
        _respawnPoint = newPoint;
    }
}