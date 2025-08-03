using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using ThreeLines.IOT.Arduino;
using UnityEngine;

public class CubeMovementWithJoystick : MonoBehaviour
{
    [SerializeField]
    float speed = 5f;

    [SerializeField]
    float jumpForce = 5f;

    [SerializeField]
    ArduinoButton joyStickUp; //6
    [SerializeField]
    ArduinoButton joyStickDown;//5
    [SerializeField]
    ArduinoButton joyStickLeft;//7
    [SerializeField]
    ArduinoButton joyStickRight;//4
    [SerializeField]
    ArduinoButton jumpButton; // Add your jump button here

    [ShowInInspector]
    [ReadOnly]
    Vector3 movement;

    [ShowInInspector]
    [ReadOnly]
    bool isGrounded = true;

    private Rigidbody rb;
    private float groundCheckDistance = 0.1f;

    private void Start()
    {
        movement = Vector2.zero;
        rb = GetComponent<Rigidbody>();

        // Add Rigidbody if it doesn't exist
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Handle horizontal movement
        movement = new(
            joyStickLeft.IsPressed ? -1 : joyStickRight.IsPressed ? 1 : 0,
            0,
            joyStickDown.IsPressed ? -1 : joyStickUp.IsPressed ? 1 : 0);

        // Apply horizontal movement
        transform.Translate(movement * Time.deltaTime * speed, Space.World);

        // Ground check using raycast
        CheckGrounded();

        // Handle jump
        if (jumpButton.IsPressed && isGrounded)
        {
            Jump();
        }
    }

    private void CheckGrounded()
    {
        // Cast a ray downward to check if we're touching the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.5f);
    }

    private void Jump()
    {
        // Apply upward force for jump
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    private void OnDrawGizmos()
    {
        // Visualize ground check ray in Scene view
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * (groundCheckDistance + 0.5f));
    }
}