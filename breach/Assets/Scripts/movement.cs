using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class movement : MonoBehaviour
{

    //globals
    readonly float MaxSpeed = 10.0f;
    readonly float Acceleration = 20.0f;
    readonly float AirFriction = 20.0f;
    readonly float GroundFriction = 30.0f;

    public GameObject player;
    Vector2 velocity;

    InputAction moveAction;
    InputAction jumpAction;
    Vector3 startPos;

    private void Start()
    {
        // 3. Find the references to the "Move" and "Jump" actions
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        // 4. Read the "Move" action value, which is a 2D vector
        // and the "Jump" action state, which is a boolean value

        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        bool isGrounded = player.GetComponent<CapsuleCollider2D>().IsTouchingLayers(LayerMask.GetMask("Ground"));

        if (moveValue != Vector2.zero)
        {
            velocity = new Vector2(moveValue.x * MaxSpeed, player.GetComponent<Rigidbody2D>().linearVelocity.y);
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.Lerp(player.GetComponent<Rigidbody2D>().linearVelocity, velocity, Acceleration * Time.deltaTime);
        } else
        {
            // Apply friction when no input is given
            float friction = isGrounded ? GroundFriction : AirFriction;
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.Lerp(player.GetComponent<Rigidbody2D>().linearVelocity, new Vector2(0, player.GetComponent<Rigidbody2D>().linearVelocity.y), friction * Time.deltaTime);
        }


        if (jumpAction.IsPressed())
        {
            if (isGrounded)
            {
                startPos = player.transform.position;

            }
        }
    }
}
