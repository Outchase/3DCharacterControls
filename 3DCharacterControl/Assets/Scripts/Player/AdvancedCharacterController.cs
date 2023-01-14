using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AdvancedCharacterController : MonoBehaviour
{
    private Vector3 movementDirection;
    private Rigidbody rb;
    private float horizontalVelocity = 0;
    private float verticalVelocity = 0;
    private float xAxis;
    private float yAxis;
    private InputOptions inputOptions;
    private bool isGrounded;

    [Header("Movement")]
    [SerializeField] float acceleration = 1;
    [SerializeField] float dampingMovingForward = 0.6f;
    [SerializeField] float dampingWhenStopping = 0.5f;
    [SerializeField] float dampingWhenTurning = 0.8f;

    [SerializeField] LayerMask groundLayer;

    [Header("Rotation")]
    [SerializeField] float xAxisSpeed;
    [SerializeField] float yAxisSpeed;
    [SerializeField] Vector2 minMaxAxis;
    [SerializeField] bool invertYAxis;
    [SerializeField] Transform eyes;

    [Header("Features")]
    [SerializeField] bool enableSprint;
    [SerializeField] float speedMultipier = 1.25f;

    [Space]
    [SerializeField] bool enableJump;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] bool enableShortJump;
    [SerializeField] float shortJump = 5f;


    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputOptions = new InputOptions();

        inputOptions.Player.Jump.started += Jump;

        inputOptions.Player.Jump.canceled += Jump;

        inputOptions.Player.Dash.performed += Dash;
        inputOptions.Player.Dash.canceled += Dash;


        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        Move(inputOptions.Player.Move.ReadValue<Vector2>());
        Rotate(inputOptions.Player.Look.ReadValue<Vector2>());
        rb.velocity = new Vector3(horizontalVelocity, rb.velocity.y, verticalVelocity);
        isGrounded = Physics.SphereCast(transform.position, 0.2f, -transform.up, out _, 1, groundLayer);
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
        movementDirection = transform.TransformDirection(movementDirection);
        Movement();
    }

    public void Rotate(Vector2 context)
    {

        // Debug.Log(context);
        xAxis += (invertYAxis ? 1 : -1) * (context.y * xAxisSpeed * Time.deltaTime);
        xAxis = Mathf.Clamp(xAxis, minMaxAxis.x, minMaxAxis.y);
        eyes.transform.localEulerAngles = Vector3.right * xAxis;

        yAxis += context.x * yAxisSpeed * Time.deltaTime;
        transform.eulerAngles = Vector3.up * yAxis;
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
        if (enableJump)
        {
            if (context.started && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            }

            if (enableShortJump)
            {
                if (context.canceled && isGrounded)
                {
                    rb.velocity = new Vector2(rb.velocity.x, shortJump);
                }
            }
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (enableSprint)
        {
            if (context.performed)
            {
                dampingMovingForward /= speedMultipier;
            }

            if (context.canceled)
            {
                dampingMovingForward *= speedMultipier;
            }
        }
    }
}

