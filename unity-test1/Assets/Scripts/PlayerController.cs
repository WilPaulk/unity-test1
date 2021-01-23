using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Component references
    Animator animator;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider2D;

    // Grounded checks & physics variables
    [SerializeField] bool isGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform groundCheckL;
    [SerializeField] private Transform groundCheckR;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] LayerMask groundedLayerMask;
    
    [SerializeField] private float slopeCheckDistance;
    private float slopeDownAngle;
    private float slopeDownAngleOld;
    private Vector2 slopeNormalPerp;
    private bool isOnSlope;
    private float slopeSideAngle;
    
    // Non-binary movement variables
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private int extraJumpNum = 2;
    private float horizontalSpeed;
    private int extraJumpsRemaining;

    // Binary movement variables
    private bool rightMovement;
    private bool leftMovement;
    private bool groundJump;
    private bool extraJump;
    private bool posYVelocity;
    private bool dropThrough;
    private bool dropThroughCoroutineIsRunning;

    private float xInput;
    string airAnimation;

    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Initialize variables
        horizontalSpeed = movementSpeed;
        dropThroughCoroutineIsRunning = false;
    }

    void Update()
    {
        CheckInput();
    }

    private void FixedUpdate()
    {
        GroundedCheck();
        // SlopeCheck();
        BaseDirectionalMovement();
        Jump();    
    }

    private void LateUpdate()
    {
        AnimationHandling();
    }

    private void CheckInput() // Check for movement input
    {
        // Standard movement
        xInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown("space") && isGrounded) 
            groundJump = true;
        if (Input.GetKeyDown("space") && !isGrounded && extraJumpsRemaining > 0)
            extraJump = true;

        // Sprint logic
        if (Input.GetKeyDown("left shift") && isGrounded)
            horizontalSpeed = sprintSpeed;
        if (Input.GetKeyUp("left shift"))
            horizontalSpeed = movementSpeed;
        
        // Platform handling
        dropThrough = Input.GetKey("s") ? true : false;
    }

    private void GroundedCheck() // Check if player is grounded
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundedLayerMask) ||
            Physics2D.OverlapCircle(groundCheckL.position, groundCheckRadius, groundedLayerMask) ||
            Physics2D.OverlapCircle(groundCheckR.position, groundCheckRadius, groundedLayerMask);
        posYVelocity = rb2d.velocity.y >= 0.0f ? true : false;
        // if ((rightMovement || leftMovement) && OnSlope())
        //     rb2d.velocity = new Vector2(rb2d.velocity.x, -slopeForce);
    }

    void OnDrawGizmosSelected() // Debugging vizualization for GroundedCheck()
    {
        Gizmos.color = (isGrounded) ? Color.green : Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawSphere(groundCheckL.position, groundCheckRadius);
        Gizmos.DrawSphere(groundCheckR.position, groundCheckRadius);
    }

    // private void SlopeCheck()
    // {
    //     Vector2 checkPos = transform.position - new Vector3(0.0f, boxCollider2D.size.y / 2);
        
    //     SlopeCheckVertical(checkPos);
    // }

    // private void SlopeCheckHorizontal(Vector2 checkPos)
    // {
    //     RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, 
    //     slopeCheckDistance, groundedLayerMask);
    //     RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, 
    //     slopeCheckDistance, groundedLayerMask);

    //     if (slopeHitFront)
    //     {
    //         isOnSlope = true;
    //         slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
    //     }
    //     else if (slopeHitBack)
    //     {
    //         isOnSlope = true;
    //         slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
    //     }
    //     else
    //     {
    //         slopeSideAngle = 0.0f;
    //         isOnSlope = false;
    //     }
    // }

    // private void SlopeCheckVertical(Vector2 checkPos)
    // {
    //     RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, groundedLayerMask);

    //     if (hit)
    //     {
    //         slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

    //         slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

    //         if (slopeDownAngle != slopeDownAngleOld)
    //             isOnSlope = true;
            
    //         slopeDownAngleOld = slopeDownAngle;

    //         Debug.DrawRay(hit.point, slopeNormalPerp, Color.red);
    //         Debug.DrawRay(hit.point, hit.normal, Color.green);
    //         Debug.Log(isOnSlope);
    //     }
    // }

    // Standard directional movement physics & animation handling
    private void BaseDirectionalMovement()
    {
        if (isGrounded /* && !isOnSlope */)
        {
            rb2d.velocity = new Vector2(xInput * movementSpeed, rb2d.velocity.y);
            
        }
        // else if (isGrounded && isOnSlope)
        // {
        //     rb2d.velocity = new Vector2(movementSpeed * slopeNormalPerp.x * -xInput, movementSpeed * slopeNormalPerp.y * -xInput);
        //     if (xInput != 0)
        //         animator.Play("Player_run");
        //     else
        //         animator.Play("Player_idle");
        // } 
        else if (!isGrounded)
        {
            rb2d.velocity = new Vector2(xInput * movementSpeed, rb2d.velocity.y);
            airAnimation = rb2d.velocity.y > 0 ? "Player_rise" : "Player_fall";
        }
    }

    private void Jump() // Jump movement physics
    {
        if (groundJump)
        {
            rb2d.velocity = new Vector2(xInput, jumpHeight);
            extraJumpsRemaining = extraJumpNum;
            groundJump = false;
        }
        else if (extraJump)
        {
            rb2d.velocity = new Vector2(xInput, jumpHeight);
            extraJumpsRemaining--;
            extraJump = false;
        }
    }

    // Create relationship between player/platforms for natural platform movement
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && !posYVelocity && isGrounded)
        {
            this.transform.parent = other.gameObject.transform;
        }
    }

    // Intialize DropThrough() coroutine if called while on platform
    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && isGrounded)
        {
            if (dropThrough && !dropThroughCoroutineIsRunning)
                StartCoroutine("DropThrough");
        }
    }

    // Coroutine to temporarily ignore collision between player and platforms
    IEnumerator DropThrough()
    {
        dropThroughCoroutineIsRunning = true;
        GameObject[] platformArray = GameObject.FindGameObjectsWithTag("Platform");
        foreach (GameObject platform in platformArray)
        {
            Physics2D.IgnoreCollision(boxCollider2D,platform.GetComponent<BoxCollider2D>());
        }
        yield return new WaitForSeconds(0.5f);
        foreach (GameObject platform in platformArray)
        {
            Physics2D.IgnoreCollision(boxCollider2D,platform.GetComponent<BoxCollider2D>(),false);
        }
        dropThroughCoroutineIsRunning = false;
    }

    // Decouple player and platform upon exiting collision
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            this.transform.parent = null;
        }
    }

    private void AnimationHandling()
    {
        if (xInput == 1)
            spriteRenderer.flipX = false;
        else if (xInput == -1)
            spriteRenderer.flipX = true;

        if (isGrounded)
        {
            if (xInput != 0)
                animator.Play("Player_run");
            else
                animator.Play("Player_idle");
        }
        else
            animator.Play(airAnimation);
    }
}