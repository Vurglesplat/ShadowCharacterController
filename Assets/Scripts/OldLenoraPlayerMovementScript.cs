using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldLenoraPlayerMovementScript : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] float jumpHeight = 3f;
    [SerializeField] float baseMoveSpeed = 12f;
    [SerializeField] float baseMaxMoveSpeed;
    [SerializeField] float baseGravity;
    [SerializeField] float groundCheckDistance = 0.4f;
    [SerializeField] float playerTurningSpeedToCamera;

    [Header("Modifier rates")]
    [SerializeField] [Range(0.0f,1.0f)] float tuckMoveRate = 9f;
    [SerializeField] [Range(0.0f, 1.0f)] float crouchMoveRate = 6f;
    [SerializeField] float hatTuckFallSpeedMultiplier = 2.0f;
    [SerializeField] float sprintSpeedMultiplier = 2.0f;
    [SerializeField] float sprintMaxSpeedMultiplier = 2.0f;
        
    [Header("Required Objects for the Script")]
    [SerializeField] Transform groundChecker;
    [SerializeField] GameObject hat;
    [SerializeField] GameObject body;
    [SerializeField] LayerMask groundMask;
    [SerializeField] GameObject bodyCollider;
    Vector3 bodyColliderPosOffset = new Vector3(0, -0.1f, 0);
    [SerializeField] GameObject hatCollider;
    Vector3 hatColliderRotationAmount = new Vector3(25,0,0);
    Vector3 hatColliderPosOffset = new Vector3(0, -0.85f, 0);


    // many of these will get removed once we have actual animations, but they can be good references for where they will be called
    float basePlayerScale = 1.0f;
    [SerializeField] float tuckedHatScale = 0.6f;
    float untuckedHatScale;
    [SerializeField] float crouchedScale = 0.4f;

    float currentAdditionalGravity;
    float currentMaxMoveSpeed;

    [HideInInspector]
    public bool isPlayerGrounded;
    [HideInInspector]
    public bool isTucking = false;
    [HideInInspector]
    public bool isCrouching = false;
    bool isSprinting = false;
    [HideInInspector]
    public bool isJumping = false;
    bool isAbleToJump = true;

    Rigidbody rb;

    Vector3 totalMove = new Vector3();

    [HideInInspector] public bool isAutoMoving = false;


    private void Start()
    {

        tuckMoveRate = tuckMoveRate * baseMaxMoveSpeed;
        crouchMoveRate = crouchMoveRate * baseMaxMoveSpeed;
        hatTuckFallSpeedMultiplier =  hatTuckFallSpeedMultiplier * baseGravity;

        untuckedHatScale = hat.transform.localScale.x;
        tuckedHatScale = untuckedHatScale * tuckedHatScale;
        crouchedScale = basePlayerScale * crouchedScale;
        basePlayerScale = this.transform.localScale.y;

        rb = this.GetComponent<Rigidbody>();

        currentAdditionalGravity = baseGravity;
        currentMaxMoveSpeed = baseMaxMoveSpeed;

        totalMove.Set(0.0f, 0.0f, 0.0f);
    }

    private void Update()
    {
        if(!isAutoMoving)
        {
            if (Input.GetButtonDown("Jump") && isAbleToJump)
            {
                isJumping = true;
                StartCoroutine("MakeSolid");
            }

            GetMovement();

            CrouchHandling();
            TuckHandling();
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
    {d
        isAbleToJump = false;
        yield return new WaitForSeconds(0.3f);
        isAbleToJump = true;
    }

void DoGravity()
    {
        isPlayerGrounded = Physics.CheckSphere(groundChecker.position, groundCheckDistance, groundMask);

        if (isTucking && !isPlayerGrounded && rb.velocity.y < 0.0f) // make it slow only when they are falling
        {
            currentAdditionalGravity = hatTuckFallSpeedMultiplier;
        }
        else
            currentAdditionalGravity = baseGravity;


        //additional gravity
        rb.AddForce(currentAdditionalGravity * Vector3.down);
    }

    void GetMovement()
    {

        totalMove.Set(0.0f, 0.0f, 0.0f);

        isSprinting = (Input.GetKey(KeyCode.LeftShift) && isPlayerGrounded && !isTucking && !isCrouching);

        if (rb.velocity.magnitude < (currentMaxMoveSpeed * (isSprinting ? sprintMaxSpeedMultiplier:1.0f) ))
        {
            if (Input.GetKey(KeyCode.W))
            {
                totalMove = rb.transform.forward;
            }

            if (Input.GetKey(KeyCode.A))
            {
                totalMove += -rb.transform.right;
            }

            if (Input.GetKey(KeyCode.S))
            {
                totalMove += -rb.transform.forward;
            }

            if (Input.GetKey(KeyCode.D))
            {
                totalMove += rb.transform.right;
            }
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
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (!isCrouching && !isTucking && isPlayerGrounded) // you are not able to use any other action while tucking
            {
                isCrouching = true;

                Debug.Log("Player is now crouching");

                //Vector3 temp = body.transform.localScale; // maybe shift this and tuckhandling to use a permanent vector instead of generating it every time
                //temp.y = crouchedScale;
                //body.transform.localScale = temp;

                bodyCollider.GetComponent<CapsuleCollider>().height *= crouchedScale;
                bodyCollider.GetComponent<CapsuleCollider>().center += bodyColliderPosOffset;

                hatCollider.transform.Rotate(hatColliderRotationAmount);
                hatCollider.transform.position += hatColliderPosOffset;


                //temp = hat.transform.localScale;
                //temp.y /= crouchedScale;
                //hat.transform.localScale = temp;

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


                hatCollider.transform.Rotate(-hatColliderRotationAmount);
                hatCollider.transform.position -= hatColliderPosOffset;

                ////temp = hat.transform.localScale;
                ////temp.y *= crouchedScale;
                ////hat.transform.localScale = temp;

                currentMaxMoveSpeed = baseMaxMoveSpeed;
            }
        }
    }

    void TuckHandling()
    {


        if (Input.GetKey(KeyCode.Mouse0))
        {
            if(!isTucking && !isCrouching)
            {
                isTucking = true;
                Vector3 temp = hat.transform.localScale;
                temp.z = temp.x = tuckedHatScale;
                hat.transform.localScale = temp;

                currentMaxMoveSpeed = tuckMoveRate;
            }
        }
        else
        {
            if(isTucking)
            {
                isTucking = false;
                Vector3 temp = hat.transform.localScale;
                temp.z = temp.x = untuckedHatScale;
                hat.transform.localScale = temp;

                currentMaxMoveSpeed = baseMaxMoveSpeed;
            }
        }
    }

    void FaceCamera()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        // Only allow aligning of player's direction when there is a movement.
        if (v > 0.1 || v < -0.1 || h > 0.1 || h < -0.1)
        {
            // rotate player towards the camera forward.
            Vector3 eu = Camera.main.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.Euler(0.0f, eu.y, 0.0f),
                playerTurningSpeedToCamera * Time.deltaTime);
        }
    }
}
