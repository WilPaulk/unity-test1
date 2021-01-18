using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Unity components
    Animator animator;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider2D;

    // Grounded checks
    [SerializeField] bool isGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform groundCheckL;
    [SerializeField] private Transform groundCheckR;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] LayerMask groundedLayerMask;
    
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
    private bool posVelocity;
    private bool dropThrough;
    private bool dropThroughCoroutineIsRunning;

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
        Move();
    }

    private void FixedUpdate()
    {
        GroundedCheck();
        BaseDirectionalMovement();
        Jump();        
    }

    private void Move() // Check for movement input
    {
        // Standard movement
        rightMovement = Input.GetKey("d") || Input.GetKey("right") ? true : false;
        leftMovement = Input.GetKey("a") || Input.GetKey("left") ? true : false;
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
        posVelocity = rb2d.velocity.y >= 0.0f ? true : false;
    }

    void OnDrawGizmosSelected() // Debugging vizualization for GroundedCheck()
    {
        Gizmos.color = (isGrounded) ? Color.green : Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawSphere(groundCheckL.position, groundCheckRadius);
        Gizmos.DrawSphere(groundCheckR.position, groundCheckRadius);
    }

    // Standard directional movement physics & animation handling
    private void BaseDirectionalMovement() 
    {
        if (!isGrounded)
        {
            string airAnimation = rb2d.velocity.y > 0 ? "Player_rise" : "Player_fall";
            animator.Play(airAnimation);
        }

        if (rightMovement)
        {
            rb2d.velocity = new Vector2(horizontalSpeed, rb2d.velocity.y);
            if(isGrounded)
                animator.Play("Player_run");
            spriteRenderer.flipX = false;
        }
        else if (leftMovement)
        {
            rb2d.velocity = new Vector2(-horizontalSpeed, rb2d.velocity.y);
            if(isGrounded)
                animator.Play("Player_run");
            spriteRenderer.flipX = true;
        }
        else
        {
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
            if(isGrounded)
                animator.Play("Player_idle");
        }
    }

    private void Jump() // Jump movement physics
    {
        if (groundJump)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpHeight);
            extraJumpsRemaining = extraJumpNum;
            groundJump = false;
        }
        else if (extraJump)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpHeight);
            extraJumpsRemaining--;
            extraJump = false;
        }
    }

    // Create relationship between player/platforms for natural platform movement
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && !posVelocity && isGrounded)
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
}