using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;
public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement")]
    private Rigidbody2D rb;
    [SerializeField] private float maxSpeed = 15f;
    private float currentMaxSpeed;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float friction = 60f;
    private float currentFriction;
    [SerializeField] private float jumpForce = 25f;
    private float xAxis;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckY = 0.3f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float boost = 20f;

    

    private PlayerStateList playerStateList;


    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    [SerializeField] private float dashTimeMS = 200f;
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashCooldownMS = 200f;
    private bool canDash = true;
    [SerializeField] private int maxDashes = 1;
    private int dashes;
    [SerializeField] private float jumpDashFriction;

    [SerializeField] private int jumpFrameTolerance = 8;
    private int jumpFrameCounter = 0;
    [SerializeField] private int coyoteFrameTolerance = 8;
    private int coyoteFrameCounter = 0;
    [SerializeField] private bool allowDoubleJump = true;
    private bool canDoubleJump = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStateList = GetComponent<PlayerStateList>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        dashAction = InputSystem.actions.FindAction("Dash");

        moveAction.Enable();
        jumpAction.Enable();
        dashAction.Enable();
    }

    void Update()
    {
        GetInput();
        UpdateState();
        Move();
        Jump();
        StartDash();
    }

    void GetInput()
    {
        xAxis = 1 * Math.Sign(moveAction.ReadValue<Vector2>().x);
    }

    void UpdateState()
    {
        if (jumpAction.triggered)
        {
            playerStateList.Jumping = true;
            jumpFrameCounter = jumpFrameTolerance;
        }
        else
        {
            if (jumpFrameCounter > 0)
                jumpFrameCounter--;
            else
                playerStateList.Jumping = false;
        }

        if (IsGrounded())
        {
            coyoteFrameCounter = coyoteFrameTolerance;
            dashes = maxDashes;
            canDoubleJump = true;
            currentFriction = friction;
            currentMaxSpeed = maxSpeed;
        }
        else if (coyoteFrameCounter > 0)
        {
            coyoteFrameCounter--;
        }

        if (playerStateList.Dashing)
        {
            currentMaxSpeed = dashSpeed;
            if (jumpAction.IsPressed() && IsGrounded())
                currentFriction = jumpDashFriction;
        }
        else
        {
            currentMaxSpeed = maxSpeed;
        }
    }

    void Move()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > currentMaxSpeed || xAxis == 0)
            decelerate();
        else rb.linearVelocity = new Vector2(rb.linearVelocity.x + acceleration * xAxis * Time.deltaTime, rb.linearVelocity.y);
        if (rb.linearVelocity.x < 0.5f && rb.linearVelocity.x > -0.5f)
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, currentFriction * Time.deltaTime), rb.linearVelocity.y);
    }

    void decelerate()
    {
        if (rb.linearVelocity.x > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x - (currentFriction + Mathf.Pow(currentFriction, (rb.linearVelocity.x - 15) / 55f)) * Time.deltaTime, rb.linearVelocity.y);
        else if (rb.linearVelocity.x < 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x + (currentFriction + Mathf.Pow(currentFriction, (-rb.linearVelocity.x - 15) / 55f)) * Time.deltaTime, rb.linearVelocity.y);
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
            playerStateList.Jumping = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.67f, 0);
        }

        if (playerStateList.Jumping && (IsGrounded() || coyoteFrameCounter > 0))
        {
            playerStateList.Jumping = false;
            applyJumpForces();
        }
        else if (playerStateList.Jumping && allowDoubleJump && canDoubleJump)
        {
            playerStateList.Jumping = false;
            canDoubleJump = false;
            applyJumpForces();
        }
    }

    void applyJumpForces()
    {
        float boostToAdd = 0;
        if (xAxis != 0)
        {
            boostToAdd = Math.Sign(xAxis) * boost;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x + boostToAdd, jumpForce, 0);
    }
    
    void StartDash()
    {
        if (canDash && dashAction.triggered && dashes > 0)
        {
            dashes--;
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        playerStateList.Dashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(dashSpeed * Math.Sign(xAxis == 0 ? transform.localScale.x : xAxis), 0);
        yield return new WaitForSeconds(dashTimeMS / 1000);
        rb.gravityScale = originalGravity;
        playerStateList.Dashing = false;
        yield return new WaitForSeconds(dashCooldownMS / 1000);
        canDash = true;
    }
}