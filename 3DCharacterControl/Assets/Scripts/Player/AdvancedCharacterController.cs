using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AdvancedCharacterController : MonoBehaviour
{
    private Vector3 movementDirection;
    private Rigidbody rb;
    private float horizontalVelocity = 0;
    private float verticalVelocity = 0;
    private InputOptions inputOptions;

    [SerializeField] float acceleration = 1;
    [SerializeField] float dampingMovingForward = 0.6f;
    [SerializeField] float dampingWhenStopping = 0.5f;
    [SerializeField] float dampingWhenTurning = 0.8f;
    [SerializeField] float speedMultipier = 1.25f;
    [SerializeField] float jumpHeight = 1f;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputOptions = new InputOptions();
        inputOptions.Player.Jump.started += Jump;
        inputOptions.Player.Dash.performed += Dash;
        inputOptions.Player.Dash.canceled += Dash;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void FixedUpdate()
    {
        Move(inputOptions.Player.Move.ReadValue<Vector2>());
        rb.velocity = new Vector3(horizontalVelocity, rb.velocity.y, verticalVelocity);

    }

    private void OnEnable()
    {
        inputOptions.Enable();
    }

    private void OnDisable()
    {
        inputOptions.Disable();
    }

    public void Move(Vector2 context)
    {
        // replace y input axe to z to make it unity scene fit
        //movementDirection = context;
        movementDirection = context;
        movementDirection.z = movementDirection.y;
        movementDirection.y = 0;
        Movement();
    }

    public void Movement()
    {
        //add basic velocity each frame
        horizontalVelocity = rb.velocity.x;
        verticalVelocity = rb.velocity.z;

        horizontalVelocity = CalculateVelocity(horizontalVelocity, movementDirection.x);
        verticalVelocity = CalculateVelocity(verticalVelocity, movementDirection.z);

        //Debug.Log(horizontalVelocity);
        //Debug.Log(verticalVelocity);
    }

    private float CalculateVelocity(float directionVelocity, float direction)
    {

        directionVelocity += direction;
        //flip negaive number into positive, verify movement direction and on 0 damp speed
        //damp speed when direction is switched
        //damp while moving forward
        if (Mathf.Abs(direction) < 0.01f) //Abs flips negative to positive
        {
            directionVelocity *= Mathf.Pow(acceleration - dampingWhenStopping, Time.deltaTime * 10f);
        }
        else if (Mathf.Sign(direction) != Mathf.Sign(directionVelocity))// Sign turns positive and 0 to 1 and negative to -1
        {
            directionVelocity *= Mathf.Pow(acceleration - dampingWhenTurning, Time.deltaTime * 10f);
        }
        else
        {
            directionVelocity *= Mathf.Pow(acceleration - dampingMovingForward, Time.deltaTime * 10f);//x^y
        }

        return directionVelocity;

    }

    public void Jump(InputAction.CallbackContext context)
    {
        Debug.Log("jump");
    }
    public void Dash(InputAction.CallbackContext context)
    {
        //dampingMovingForward /= speedMultipier;

        /*  if (context.performed)
          {
              dampingMovingForward /= speedMultipier;
          }

          if (context.canceled)
          {
              dampingMovingForward *= speedMultipier;
          }*/
        Debug.Log("dash");
    }
}

