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
    //private float tempGravity = 0;
    private float tempGrounded = 0;
    private float tempJumpPress = 0;
    private int extraAmountOfJumps;
    private bool isGrounded;
    private InputOptions inputOptions;

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
    [SerializeField] bool enableSprint = false;
    [SerializeField] float speedMultipier = 1.25f;

    [Space]
    [SerializeField] bool enableJump = false;
    [SerializeField] bool enableDoubleJump = false;
    [SerializeField] float jumpForce = 10f;
    [Space]
    [SerializeField] bool enableShortJump = false;
    [SerializeField] float shortJump = 5f;
    [Space]
    [SerializeField] bool enableCoyoteTimer;
    [SerializeField] float coyoteTimer = 0.1f;
    [Space]
    [SerializeField] bool enableJumpBuffer;
    [SerializeField] float JumpBeforeGroundTimer = 0.2f;



    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputOptions = new InputOptions();

        //tempGravity = Physics.gravity.y;

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
        isGrounded = Physics.SphereCast(transform.position, 0.2f, -transform.up, out _, 1, groundLayer);

        //coyote Time set whenever player is on ground
        if (enableCoyoteTimer)
        {
            tempGrounded -= Time.deltaTime;
            if (isGrounded)
            {
                tempGrounded = coyoteTimer;
            }
        }

        //let player jump before hiting the ground
        if (enableJumpBuffer)
        {
            tempJumpPress -= Time.deltaTime;
        }

        if (enableDoubleJump && isGrounded)
        {
            extraAmountOfJumps = 1;
        }


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
        movementDirection = transform.TransformDirection(movementDirection);

        //add basic velocity each frame
        horizontalVelocity = rb.velocity.x;
        verticalVelocity = rb.velocity.z;

        horizontalVelocity = CalculateVelocity(horizontalVelocity, movementDirection.x);
        verticalVelocity = CalculateVelocity(verticalVelocity, movementDirection.z);

        //Debug.Log(horizontalVelocity);
        //Debug.Log(verticalVelocity);
    }

    public void Rotate(Vector2 context)
    {
        int pitchAxis;

        //xAxis from pitch perspective
        //check if pitch axis is invert
        if (invertYAxis == true)
        {
            pitchAxis = 1;
        }
        else
        {
            pitchAxis = -1;
        }

        //positive or negative speed of pitch axis
        xAxis += pitchAxis * (context.y * xAxisSpeed * Time.deltaTime);

        //example for inline
        //xAxis += (invertYAxis ? 1 : -1) * (context.y * xAxisSpeed * Time.deltaTime);

        //set range limit
        xAxis = Mathf.Clamp(xAxis, minMaxAxis.x, minMaxAxis.y);
        
        //moves only camera
        eyes.transform.localEulerAngles = Vector3.right * xAxis;

        yAxis += context.x * yAxisSpeed * Time.deltaTime;

        //rotate player
        transform.eulerAngles = Vector3.up * yAxis;
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
            if (context.started)
            {
                if (enableJumpBuffer)
                {
                    tempJumpPress = JumpBeforeGroundTimer;
                }

                if (tempJumpPress > 0 && tempGrounded > 0 || isGrounded || enableDoubleJump && extraAmountOfJumps > 0)
                {
                    tempJumpPress = 0;
                    tempGrounded = 0;

                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                    extraAmountOfJumps--;
                }

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

