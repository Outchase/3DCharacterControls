using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AdvancedCharacterController : MonoBehaviour
{
    private Vector3 movementDirection;
    private Rigidbody rb;
    private float horizontalVelocity = 0;

    [SerializeField] float acceleration = 1;
    [SerializeField] float dampingMovingForward = 0.8f;
    [SerializeField] float dampingWhenStopping = 0.5f;
    [SerializeField] float dampingWhenTurning = 0.8f;

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
        //rb.velocity += movementDirection * Time.deltaTime * speed;
        Movement();
        rb.velocity = new Vector3(horizontalVelocity, rb.velocity.y, rb.velocity.z);

    }

    public void Movement()
    {
        //add basic velocity each frame
        horizontalVelocity = rb.velocity.x;
        horizontalVelocity += movementDirection.x;
        
        //flip negaive number into positive, verify movement direction and on 0 damp speed
        //damp speed when direction is switched
        //damp while moving forward
        if (Mathf.Abs(movementDirection.x) < 0.01f) //Âbs flips negative to positive
        {
            horizontalVelocity *= Mathf.Pow(acceleration - dampingWhenStopping, Time.deltaTime * 10f);
        }
        else if (Mathf.Sign(movementDirection.x) != Mathf.Sign(horizontalVelocity))// Sign turns positive and 0 to 1 and negative to -1
        {
            horizontalVelocity *= Mathf.Pow(acceleration - dampingWhenTurning, Time.deltaTime * 10f);
        }
        else
        {
            horizontalVelocity *= Mathf.Pow(acceleration - dampingMovingForward, Time.deltaTime * 10f);//x^y
        }
        Debug.Log(horizontalVelocity);

    }
}
