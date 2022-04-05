using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] float jumpHeight = 3f;
    [SerializeField] float baseMoveSpeed = 12f;
    [SerializeField] float baseMaxMoveSpeed;
    [SerializeField] float baseGravity;
    [SerializeField] float groundCheckDistance = 0.4f;
    [SerializeField] float playerTurningSpeedToCamera;

    [Header("Modifier rates")]
    [SerializeField] [Range(0.0f, 1.0f)] float tuckMoveRate = 9f;
    [SerializeField] [Range(0.0f, 1.0f)] float crouchMoveRate = 6f;
    [SerializeField] float sprintSpeedMultiplier = 2.0f;
    [SerializeField] float sprintMaxSpeedMultiplier = 2.0f;

    [Header("Required Objects for the Script")]
    [SerializeField] Transform groundChecker;
    [SerializeField] LayerMask groundMask;
    [SerializeField] GameObject bodyCollider;
    Vector3 bodyColliderPosOffset = new Vector3(0, -0.1f, 0);


    float basePlayerScale = 1.0f;
    [SerializeField] float crouchedScale = 0.4f;

    float currentAdditionalGravity;
    float currentMaxMoveSpeed;

    [HideInInspector]
    public bool isPlayerGrounded;
    [HideInInspector]
    public bool isCrouching = false;
    bool isSprinting = false;
    [HideInInspector]
    public bool isJumping = false;
    bool isAbleToJump = true;

    Rigidbody rb;

    Vector3 totalMove = new Vector3();

    [HideInInspector] public bool isAutoMoving = false;

    PlayerInput pInput;

    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction crouchAction;


    private void Start()
    {

        tuckMoveRate = tuckMoveRate * baseMaxMoveSpeed;
        crouchMoveRate = crouchMoveRate * baseMaxMoveSpeed;

        crouchedScale = basePlayerScale * crouchedScale;
        basePlayerScale = this.transform.localScale.y;

        rb = this.GetComponent<Rigidbody>();

        currentAdditionalGravity = baseGravity;
        currentMaxMoveSpeed = baseMaxMoveSpeed;

        totalMove.Set(0.0f, 0.0f, 0.0f);


        // Input setup
        pInput = this.GetComponent<PlayerInput>();
        moveAction = pInput.actions["Move"];
        lookAction = pInput.actions["Look"];
        jumpAction = pInput.actions["Jump"];
        sprintAction = pInput.actions["Sprint"];
        crouchAction = pInput.actions["Crouch"];

    }

    private void Update()
    {
        if(!isAutoMoving)
        {
            if (jumpAction.triggered && isAbleToJump)
            {
                isJumping = true;
                StartCoroutine("MakeSolid");
            }

            GetMovement();

            CrouchHandling();
        }
        else
        {
            ;
        }
    }
    void FixedUpdate()
    {
        //do movement
        rb.AddForce(totalMove * baseMoveSpeed * (isSprinting?sprintSpeedMultiplier:1.0f));

        if(!isAutoMoving)
            FaceCamera();
        
        JumpHandling();
        DoGravity();
    }


    IEnumerator MakeSolid()
    {
        isAbleToJump = false;
        yield return new WaitForSeconds(0.3f);
        isAbleToJump = true;
    }

void DoGravity()
    {
        isPlayerGrounded = Physics.CheckSphere(groundChecker.position, groundCheckDistance, groundMask);

        
        currentAdditionalGravity = baseGravity;


        //additional gravity
        rb.AddForce(currentAdditionalGravity * Vector3.down);
    }

    void GetMovement()
    {

        totalMove.Set(0.0f, 0.0f, 0.0f);

        isSprinting = (sprintAction.IsPressed() && isPlayerGrounded && !isCrouching);

        if (rb.velocity.magnitude < (currentMaxMoveSpeed * (isSprinting ? sprintMaxSpeedMultiplier:1.0f) ))
        {
            Vector2 move = moveAction.ReadValue<Vector2>();

            totalMove  = move.y * rb.transform.forward;
            totalMove += move.x * rb.transform.right;

            //if (Input.GetKey(KeyCode.W))
            //{
            //    totalMove =  rb.transform.forward;
            //}

            //if (Input.GetKey(KeyCode.A))
            //{
            //    totalMove += -rb.transform.right;
            //}

            //if (Input.GetKey(KeyCode.S))
            //{
            //    totalMove += -rb.transform.forward;
            //}

            //if (Input.GetKey(KeyCode.D))
            //{
            //    totalMove += rb.transform.right;
            //}
        }
    }

    void JumpHandling()
    {
        if (isJumping && isPlayerGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
            rb.AddForce(jumpHeight * rb.transform.up);
            isJumping = false;
        }
    }

    void CrouchHandling()
    {
        if (crouchAction.IsPressed())
        {
            if (!isCrouching && isPlayerGrounded)
            {
                isCrouching = true;

                Debug.Log("Player is now crouching");

                //Vector3 temp = body.transform.localScale; // maybe shift this and tuckhandling to use a permanent vector instead of generating it every time
                //temp.y = crouchedScale;
                //body.transform.localScale = temp;

                bodyCollider.GetComponent<CapsuleCollider>().height *= crouchedScale;
                bodyCollider.GetComponent<CapsuleCollider>().center += bodyColliderPosOffset;

                currentMaxMoveSpeed = crouchMoveRate;
            }
        }
        else
        {
            if (isCrouching)
            {
                Debug.Log("Player now not crouching");

                isCrouching = false;
                //Vector3 temp = body.transform.localScale;
                //temp.y = basePlayerScale;
                //body.transform.localScale = temp;
                
                bodyCollider.GetComponent<CapsuleCollider>().height /= crouchedScale;
                bodyCollider.GetComponent<CapsuleCollider>().center -= bodyColliderPosOffset;

                currentMaxMoveSpeed = baseMaxMoveSpeed;
            }
        }
    }

    void FaceCamera()
    {
        Vector2 lookAxes = lookAction.ReadValue<Vector2>();



        float horizontal = lookAxes.x;
        float vertical   = lookAxes.y;

        //old


        // eally old
        //float h = Input.GetAxis("Horizontal");
        //float v = Input.GetAxis("Vertical");


        // Only allow aligning of player's direction when there is a movement.
        if (vertical > 0.1 || vertical < -0.1 || horizontal > 0.1 || horizontal < -0.1)
        {

            // rotate player towards the camera forward.
            Vector3 eu = Camera.main.transform.rotation.eulerAngles;
            this.transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.Euler(0.0f, eu.y, 0.0f),
                playerTurningSpeedToCamera * Time.deltaTime);
        }
    }
}
