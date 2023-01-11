using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    private Vector3 movementDirection;
    private Rigidbody rb;

    [SerializeField] int speed = 0;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        // replace y input axe to z to make it unity scene fit
        movementDirection = context.ReadValue<Vector2>();
        movementDirection.z = movementDirection.y;
        movementDirection.y = 0;
    }

    public void Awake()
    {
       rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        rb.velocity += movementDirection * Time.deltaTime * speed ;

    }
}
