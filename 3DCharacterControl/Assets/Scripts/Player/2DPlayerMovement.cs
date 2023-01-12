using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;

#endif

using UnityEngine;
using UnityEngine.InputSystem;

public class AdvancedPlayerMovement : MonoBehaviour
{
    //set a selectbox
    enum ItemType { movement, jumping, timer, colissionMarkpoints }

    [Header("Presets")]
    [SerializeField] ItemType setup;
    readonly List<GameObject> movements = new();

    bool showDashMovement = false;
    bool showBasicMovement = false;
    bool showJumpExtra = false;
    bool showBasicJump = false;
    bool showBasicJumpTimer = false;
    bool showTimerExtra = false;

    #region
#if UNITY_EDITOR

    [CustomEditor(typeof(AdvancedPlayerMovement))]
    public class AdvancedPlayerMovementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AdvancedPlayerMovement advancedPlayerMovement = (AdvancedPlayerMovement)target;

            //shows diffrence setup depending the what was selected
            if (advancedPlayerMovement.setup == ItemType.movement)
            {
                EditorGUILayout.Space();
                advancedPlayerMovement.showBasicMovement = EditorGUILayout.Foldout(advancedPlayerMovement.showBasicMovement, "Basic Movement", true);

                //toggle drop down settings
                if (advancedPlayerMovement.showBasicMovement)
                {
                    advancedPlayerMovement.acceleration = EditorGUILayout.FloatField("Acceleration", advancedPlayerMovement.acceleration);
                    advancedPlayerMovement.dampingMovingForward = EditorGUILayout.Slider("Damping Moving Forward", advancedPlayerMovement.dampingMovingForward, 0, 1f);
                    advancedPlayerMovement.dampingWhenStopping = EditorGUILayout.Slider("Damping When Stopping", advancedPlayerMovement.dampingWhenStopping, 0, 1f);
                    advancedPlayerMovement.dampingWhenTurning = EditorGUILayout.Slider("Damping When Turning", advancedPlayerMovement.dampingWhenTurning, 0, 1f);
                }

                EditorGUILayout.Space();

                advancedPlayerMovement.showDashMovement = EditorGUILayout.Foldout(advancedPlayerMovement.showDashMovement, "Dash Movement", true);
                if (advancedPlayerMovement.showDashMovement)
                {
                    EditorGUILayout.Space();
                    advancedPlayerMovement.enableDash = EditorGUILayout.Toggle("Enable Dash", advancedPlayerMovement.enableDash);
                    if (advancedPlayerMovement.enableDash)
                    {
                        advancedPlayerMovement.speedMultipier = EditorGUILayout.FloatField("Speed Multiplier", advancedPlayerMovement.speedMultipier);
                    }
                }
            }
            else if (advancedPlayerMovement.setup == ItemType.jumping)
            {
                EditorGUILayout.Space();
                advancedPlayerMovement.showBasicJump = EditorGUILayout.Foldout(advancedPlayerMovement.showBasicJump, "Basic Jumps", true);
                if (advancedPlayerMovement.showBasicJump)
                {
                    advancedPlayerMovement.shortJump = EditorGUILayout.FloatField("Short Jump", advancedPlayerMovement.shortJump);
                    advancedPlayerMovement.jumpForce = EditorGUILayout.FloatField("Jump Force", advancedPlayerMovement.jumpForce);
                }

                EditorGUILayout.Space();

                advancedPlayerMovement.showJumpExtra = EditorGUILayout.Foldout(advancedPlayerMovement.showJumpExtra, "Extras", true);
                if (advancedPlayerMovement.showJumpExtra)
                {
                    advancedPlayerMovement.enableDoubleJump = EditorGUILayout.Toggle("Enable Double Jump", advancedPlayerMovement.enableDoubleJump);
                    advancedPlayerMovement.enableWallJump = EditorGUILayout.Toggle("Enable Wall Jump", advancedPlayerMovement.enableWallJump);
                    if (advancedPlayerMovement.enableWallJump)
                    {
                        advancedPlayerMovement.horizontalWallJumpForce = EditorGUILayout.FloatField("Horizontal Wall Jump Force", advancedPlayerMovement.horizontalWallJumpForce);
                        advancedPlayerMovement.verticalWallJumpForce = EditorGUILayout.FloatField("Vertical Wall Jump Force", advancedPlayerMovement.verticalWallJumpForce);
                    }
                }
            }
            else if (advancedPlayerMovement.setup == ItemType.timer)
            {
                EditorGUILayout.Space();
                advancedPlayerMovement.showBasicJumpTimer = EditorGUILayout.Foldout(advancedPlayerMovement.showBasicJumpTimer, "Jump Timer", true);
                if (advancedPlayerMovement.showBasicJumpTimer)
                {
                    advancedPlayerMovement.coyoteTimer = EditorGUILayout.FloatField("Coyote Timer", advancedPlayerMovement.coyoteTimer);
                    advancedPlayerMovement.JumpBeforeGroundTimer = EditorGUILayout.FloatField("Jump Before Ground Timer", advancedPlayerMovement.JumpBeforeGroundTimer);
                }

                EditorGUILayout.Space();

                advancedPlayerMovement.showTimerExtra = EditorGUILayout.Foldout(advancedPlayerMovement.showTimerExtra, "Extras", true);
                if (advancedPlayerMovement.showTimerExtra)
                {
                    if (advancedPlayerMovement.enableWallJump)
                    {
                        advancedPlayerMovement.wallGrabTimer = EditorGUILayout.FloatField("Wall Grab Timer", advancedPlayerMovement.wallGrabTimer);
                        advancedPlayerMovement.wallJumpTimer = EditorGUILayout.FloatField("Wall Jump Timer", advancedPlayerMovement.wallJumpTimer);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Wall Jump is disabled");
                    }
                }
            }
            else if (advancedPlayerMovement.setup == ItemType.colissionMarkpoints)
            {

                EditorGUILayout.Space();
                advancedPlayerMovement.groundLayer = EditorGUILayout.MaskField("Ground Layer", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(advancedPlayerMovement.groundLayer), InternalEditorUtility.layers);
                advancedPlayerMovement.groundCenter = EditorGUILayout.ObjectField("Ground Center", advancedPlayerMovement.groundCenter, typeof(Transform), true) as Transform; //get the System.Type object for a type and allow to assign object
                advancedPlayerMovement.wallCenterRight = EditorGUILayout.ObjectField("Wall Center Right", advancedPlayerMovement.wallCenterRight, typeof(Transform), true) as Transform;
                advancedPlayerMovement.wallCenterLeft = EditorGUILayout.ObjectField("Wall Center Left", advancedPlayerMovement.wallCenterLeft, typeof(Transform), true) as Transform;
            }



        }
    }
#endif
    #endregion

    private Vector2 movementDirection;
    private bool isGround;
    private bool isWallRight;
    private bool isWallLeft;
    private bool isGrabbing = false;
    private bool didWallJump = false;
    private Rigidbody2D rb;
    private float tempGrounded = 0;
    private float tempJumpPress = 0;
    private float horizontalVelocity = 0;
    private int extraAmountOfJumps;
    private float wallGrabCounter = 0;
    private float tempGravity = 0;


    [HideInInspector][SerializeField] bool enableDash = false;
    [HideInInspector][SerializeField] float acceleration = 1;
    [HideInInspector][SerializeField] float speedMultipier = 1.25f;
    [HideInInspector][SerializeField] float dampingMovingForward = 0.8f;
    [HideInInspector][SerializeField] float dampingWhenStopping = 0.5f;
    [HideInInspector][SerializeField] float dampingWhenTurning = 0.8f;
    [HideInInspector][SerializeField] bool enableWallJump = false;
    [HideInInspector][SerializeField] bool enableDoubleJump = false;
    [HideInInspector][SerializeField] float shortJump = 9f;
    [HideInInspector][SerializeField] float jumpForce = 18f;
    [HideInInspector][SerializeField] float horizontalWallJumpForce = 10f;
    [HideInInspector][SerializeField] float verticalWallJumpForce = 12f;
    [HideInInspector][SerializeField] LayerMask groundLayer;
    [HideInInspector][SerializeField] Transform groundCenter;
    [HideInInspector][SerializeField] Transform wallCenterRight;
    [HideInInspector][SerializeField] Transform wallCenterLeft;
    [HideInInspector][SerializeField] float coyoteTimer = 0.05f;
    [HideInInspector][SerializeField] float JumpBeforeGroundTimer = 0.2f;
    [HideInInspector][SerializeField] float wallGrabTimer = 0.2f;
    [HideInInspector][SerializeField] float wallJumpTimer = 0.2f;

    public void OnMove(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<Vector2>();
    }

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        tempGravity = rb.gravityScale;
    }

    private void Update()
    {
        //verify if wall or ground is touched
        isGround = Physics2D.OverlapCircle(groundCenter.position, 0.2f, groundLayer);
        isWallRight = Physics2D.OverlapCircle(wallCenterRight.position, 0.2f, groundLayer);
        isWallLeft = Physics2D.OverlapCircle(wallCenterLeft.position, 0.2f, groundLayer);

        //coyote Time set whenever player is on ground
        tempGrounded -= Time.deltaTime;
        if (isGround)
        {
            tempGrounded = coyoteTimer;
        }

        //let player jump before hiting the ground
        tempJumpPress -= Time.deltaTime;
        if (tempJumpPress > 0 && tempGrounded > 0)
        {
            tempJumpPress = 0;
            tempGrounded = 0;
            extraAmountOfJumps = 1;

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (enableWallJump)
        {
            WallJumpControler();
        }

        Movement();

        //apply speed to the player movement 
        if (!didWallJump)
        {
            rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {


        if (context.started)
        {
            tempJumpPress = JumpBeforeGroundTimer;

            if (enableDoubleJump && extraAmountOfJumps > 0 && !isGrabbing)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                extraAmountOfJumps--;
            }
        }

        if (context.started && isGrabbing)
        {
            didWallJump = true;
        }



        if (context.canceled)
        {
            if (rb.velocity.y > 0f && isGround)
            {
                rb.velocity = new Vector2(rb.velocity.x, shortJump);
            }

        }
    }

    private void WallJumpControler()
    {
        if (!isGround)
        {
            if (isWallLeft && wallGrabCounter >= 0 || isWallRight && wallGrabCounter >= 0)
            {
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;
                wallGrabCounter -= Time.deltaTime;
                isGrabbing = true;

                //listens to jump event
                if (didWallJump)
                {
                    if (isWallLeft)
                    {
                        rb.velocity = new Vector2(horizontalWallJumpForce, verticalWallJumpForce);
                    }
                    else
                    {
                        rb.velocity = new Vector2(-horizontalWallJumpForce, verticalWallJumpForce);
                    }
                    //reset grabtime and set recovery walljump after sucessful jump

                    Invoke(nameof(SetWallJumpToFalse), wallJumpTimer);
                }

            }
            else
            {
                rb.gravityScale = tempGravity;
                isGrabbing = false;

            }
        }
        else
        {
            //reset grabtime after touching the ground
            SetWallJumpToFalse();
        }


    }

    public void OnDash(InputAction.CallbackContext context)
    {
        //cut acceleration in half to multiply speed
        if (enableDash)
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

    public void SetWallJumpToFalse()
    {
        didWallJump = false;
        wallGrabCounter = wallGrabTimer;

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

    }
}
