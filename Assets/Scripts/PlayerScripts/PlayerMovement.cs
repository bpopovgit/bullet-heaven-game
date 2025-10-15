using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementInput;
    public float moveSpeed = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        var moveAction = playerInput.actions["Move"];

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        playerInput.actions.Enable(); // Important
    }

    private void OnDisable()
    {
        var playerInput = GetComponent<PlayerInput>();
        var moveAction = playerInput.actions["Move"];

        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        Debug.Log("Move input: " + movementInput); // For debugging
    }

    private void FixedUpdate()
    {
        rb.velocity = movementInput * moveSpeed;
    }
}