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
    [SerializeField] private Vector2 _respawnPoint;
    [SerializeField] private float maxSpeed = 15f;
    private float currentMaxSpeed;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float friction = 60f;
    private float currentFriction;
    [SerializeField] private float jumpForce = 25f;
    [SerializeField] private float Gravity = 7;
    private float xAxis;
    private float yAxis;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckY = 0.3f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float boost = 20f;
    [SerializeField] private bool _active = true;

    [SerializeField] private Transform leftWallCheck;
    [SerializeField] private Transform rightWallCheck;
    [SerializeField] private float wallCheckX = 0.2f;
    [SerializeField] private float wallCheckY = 0.35f;
    [SerializeField] private LayerMask wallLayer;
    

    private PlayerStateList playerStateList;


    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction sprintAction;
    [SerializeField] private float dashTimeMS = 200f;
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashCooldownMS = 200f;
    private bool canDash = true;
    [SerializeField] private int maxDashes = 1;
    private int dashes;
    [SerializeField] private float sprintJumpFriction;
    [SerializeField] private float sprintTimeMS = 300f;
    [SerializeField] private float sprintSpeed = 50f;
    [SerializeField] private float sprintCooldownMS = 200f;

    private bool canSprint = true;
    private int sprints = 1;

    [SerializeField] private int jumpFrameTolerance = 8;
    private int jumpFrameCounter = 0;
    [SerializeField] private int coyoteFrameTolerance = 8;
    private int coyoteFrameCounter = 0;
    [SerializeField] private bool allowDoubleJump = true;
    private bool canDoubleJump = true;
    [SerializeField] private float maxFallSpeed = 40f;
    [SerializeField] private float wallDragFallSpeed = 10f;
    private float currentMaxFallSpeed;
    private Collider2D _collider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStateList = GetComponent<PlayerStateList>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        dashAction = InputSystem.actions.FindAction("Dash");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        _collider = GetComponent<Collider2D>();

        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        dashAction.Enable();
        SetRespawnPoint(transform.position);

        currentMaxFallSpeed = maxFallSpeed;
    }

    void Update()
    {
        
        if (!_active)
        {
            return;
        }
        GetInput();
        UpdateState();
        Move();
        StartSprint();
        StartDash();
        Jump();

        if (!_active)
        {
            return;
        }
    }

    float round(float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }

    void GetInput()
    {
        xAxis = 1 * Math.Sign(moveAction.ReadValue<Vector2>().x);
        yAxis = 1 * Math.Sign(moveAction.ReadValue<Vector2>().y);
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
            //if not in contact with ground, decrement jump frame counter
            if (jumpFrameCounter > 0)
                jumpFrameCounter--;
            else
                playerStateList.Jumping = false;
        }

        resetGroundedStates();
        resetSprintState();
        resetStatesOnWall();

        //ensure dash maintains speed while dashing
        if (playerStateList.Dashing)
        {
            currentMaxSpeed = dashSpeed;
        }

        //reset sprint speed in the air, ensure sprint doesn't get interrupted if not dashing
        else if (!playerStateList.Sprinting)
        {
            currentMaxSpeed = maxSpeed;
        }
    }

    void resetStatesOnWall()
    {
        //reset dash and double jump on wall contact
        if (isTouchingWall())
        {
            resetDashes();
            resetDoubleJump();
        }
    }

    void resetGroundedStates()
    {
        if (IsGrounded())
        {
            coyoteFrameCounter = coyoteFrameTolerance;
            resetDashes();
            resetDoubleJump();
            if (!playerStateList.Sprinting && !playerStateList.Dashing)
            {
                currentFriction = friction;
                currentMaxSpeed = maxSpeed;
            }
        }
        else if (coyoteFrameCounter > 0)
        {
            coyoteFrameCounter--;
        }
    }
    void resetSprintState()
    {
        if (!sprintAction.IsPressed())
        {
            playerStateList.Sprinting = false;
            if (IsGrounded()) canSprint = true;
            sprints = 1;
        }
    }
    void resetDoubleJump()
    {
        canDoubleJump = true;
    }
    void resetDashes()
    {
        dashes = maxDashes;
    }

    void Move()
    {
        if (Mathf.Abs(rb.linearVelocity.x) < 0.5f)
            rb.linearVelocity = new Vector2(round(Mathf.Lerp(rb.linearVelocity.x, 0, currentFriction /*/ 4 /*smoother deceleration*/ * Time.deltaTime)), rb.linearVelocity.y);
        if (Mathf.Abs(rb.linearVelocity.x) > currentMaxSpeed || xAxis == 0)
        {
            decelerate();
        }
        else
        {
            rb.linearVelocityX = round(rb.linearVelocity.x + acceleration * xAxis * Time.deltaTime);
        }
        fall();
    }

    void fall()
    {   
        //wall drag
        if(isTouchingLeftWall() && xAxis < 0 || isTouchingRightWall() && xAxis > 0)
        {
            rb.linearVelocityY = round(Mathf.Clamp(rb.linearVelocity.y, -wallDragFallSpeed, maxFallSpeed));
        }
        
        rb.linearVelocityY = round(Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, maxFallSpeed));
    }

    void decelerate()
    {
        if (rb.linearVelocity.x > 0)
            rb.linearVelocity = new Vector2(Mathf.Max(0, round(rb.linearVelocity.x - currentFriction * Time.deltaTime)), rb.linearVelocity.y);
        else if (rb.linearVelocity.x < 0)
            rb.linearVelocity = new Vector2(Mathf.Min(0, round(rb.linearVelocity.x + currentFriction * Time.deltaTime)), rb.linearVelocity.y);
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckY, groundLayer)
            || Physics2D.Raycast(groundCheck.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, groundLayer)
            || Physics2D.Raycast(groundCheck.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, groundLayer);
    }
   
   //Separate wall checks for left and right to allow for dragging along walls
    public bool isTouchingLeftWall()
    {
        return Physics2D.Raycast(leftWallCheck.position, Vector2.left, wallCheckX, wallLayer)
            || Physics2D.Raycast(leftWallCheck.position + new Vector3(0, wallCheckY, 0), Vector2.left, wallCheckX, wallLayer)
            || Physics2D.Raycast(leftWallCheck.position + new Vector3(0, -wallCheckY, 0), Vector2.left, wallCheckX, wallLayer);
    }
    public bool isTouchingRightWall()
    {
        return Physics2D.Raycast(rightWallCheck.position, Vector2.right, wallCheckX, wallLayer)
            || Physics2D.Raycast(rightWallCheck.position + new Vector3(0, wallCheckY, 0), Vector2.right, wallCheckX, wallLayer)
            || Physics2D.Raycast(rightWallCheck.position + new Vector3(0, -wallCheckY, 0), Vector2.right, wallCheckX, wallLayer);
    }
    public bool isTouchingWall()
    {
        return isTouchingLeftWall() || isTouchingRightWall();
    }


    void Jump()
    {
        if (playerStateList.Dashing) return;

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
        else if (isTouchingWall() && !IsGrounded() && playerStateList.Jumping)
        {
            wallJump();
        }
        else if (playerStateList.Jumping && allowDoubleJump && canDoubleJump)
        {
            playerStateList.Jumping = false;
            canDoubleJump = false;
            applyJumpForces();
        }
    }

    void wallJump()
    {
        float wallDir = isTouchingLeftWall() ? 1 : -1;
        rb.linearVelocity = new Vector2(0, 0);
        rb.linearVelocity = new Vector2(wallDir * boost * 1.7f, jumpForce);
    }

    void applyJumpForces()
    {
        float boostToAdd = 0;
        if  (xAxis != 0)
        {
            boostToAdd = Math.Sign(xAxis) * boost;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x + boostToAdd, jumpForce, 0);
    }

    public void StartDash()
    {
        if (canDash && dashAction.triggered && dashes > 0 && !playerStateList.Sprinting)
        {
            dashes--;
            rb.linearVelocity = new Vector2(0, 0);
            bool temp = canDoubleJump;
            canDoubleJump = false;
            StartCoroutine(Dash());
            canDoubleJump = temp;
        }
    }

    public IEnumerator Dash()
    {
        canDash = false;
        playerStateList.Dashing = true;
        currentMaxSpeed = dashSpeed;
        currentMaxFallSpeed = dashSpeed;
        rb.gravityScale = 0;

        //direction multipliers
        float xDir = xAxis;
        float yDir = yAxis;
        if( xDir == 0 && yDir == 0)
        {
            xDir = Math.Sign(transform.localScale.x);
        }
        else if (yDir != 0 && xDir != 0)
        {
            yDir *= 0.7f;
            xDir *= 0.7f;
        }
        rb.linearVelocity = new Vector2(dashSpeed * xDir, dashSpeed * yDir);

        yield return new WaitForSeconds(dashTimeMS / 1000);
        playerStateList.Dashing = false;
        if (!playerStateList.Respawning)
        {
            rb.gravityScale = Gravity;
            currentMaxSpeed = maxSpeed;
            currentMaxFallSpeed = maxFallSpeed;
        }
        yield return new WaitForSeconds(dashCooldownMS / 1000);
        canDash = true;
    }

    void StartSprint()
    {
        if (canSprint && sprintAction.triggered && sprints > 0 && !playerStateList.Dashing)
        {
            sprints = 0;
            StartCoroutine(Sprint());
        }
    }

    public IEnumerator Sprint()
    {
        canSprint = false;
        playerStateList.Sprinting = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        currentMaxSpeed = sprintSpeed;
        rb.linearVelocity = new Vector2(sprintSpeed * Math.Sign(xAxis == 0 ? transform.localScale.x : xAxis), 0);
        yield return new WaitForSeconds(sprintTimeMS / 1000);
        rb.gravityScale = originalGravity;
        yield return new WaitForSeconds(sprintCooldownMS / 1000);
        if(!IsGrounded()) playerStateList.Sprinting = false;
        canSprint = true;
    }
    public void Die()
    {
        _active = false;
        Freeze();
        StartCoroutine(Respawn());
        
    }

    public void SetRespawnPoint(Vector2 position)
    {
        _respawnPoint = position;
    }

    private IEnumerator Respawn()
    {
        playerStateList.Respawning = true;
        rb.gravityScale = 0;
        yield return PlayDeathAnimation();
        rb.gravityScale = Gravity;
        _collider.enabled = true;
        transform.position = _respawnPoint;
        _active = true;
        playerStateList.Respawning = false;
    }
    private void Freeze()
    {
        rb.linearVelocity = new Vector2(0, 0);
    }
    private void setVelocity(float a, float b)
    {
        rb.linearVelocity = new Vector2(a, b);
    }
    private IEnumerator PlayDeathAnimation()
    {
        _collider.enabled = false;
        setVelocity(0, 3f);
        yield return new WaitForSeconds(0.75f);
        Freeze();
        yield return new WaitForSeconds(0.5f);
        yield break;
    }
}