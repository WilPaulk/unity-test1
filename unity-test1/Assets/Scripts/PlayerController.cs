using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider2D;

    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private int extraJumpNum = 2;
    private float horizontalSpeed;
    private int extraJumpsRemaining;

// Grounded checks
    [SerializeField] bool isGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform groundCheckL;
    [SerializeField] private Transform groundCheckR;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] LayerMask groundedLayerMask;

// Binary movement variables
    private bool rightMovement;
    private bool leftMovement;
    private bool groundJump;
    private bool extraJump;
    private bool posVelocity;
    [SerializeField] private bool dropThrough;
    [SerializeField] private bool dropThroughCoroutineIsRunning;

    void Start()
    {
        // Initialize components
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

    private void Move()
    {
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
        dropThrough = Input.GetKey("s") ? true : false;
    }

    private void GroundedCheck()
    {
        // float extraHeightTest = 0.1f;
        // RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, extraHeightTest, groundedLayerMask);
        // Color rayColor;
        // if (raycastHit.collider != null)
        // {
        //     rayColor = Color.green;
        // }
        // else
        // {
        //     rayColor = Color.red;
        // }
        // Debug.DrawRay(boxCollider2D.bounds.center + new Vector3(boxCollider2D.bounds.extents.x,0), Vector2.down * (boxCollider2D.bounds.extents.y + extraHeightTest), rayColor);
        // Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x,0), Vector2.down * (boxCollider2D.bounds.extents.y + extraHeightTest), rayColor);
        // Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, boxCollider2D.bounds.extents.y + extraHeightTest), Vector2.right * (boxCollider2D.bounds.extents.x) * 2, rayColor);
        // Debug.Log(raycastHit.collider);
        // isGrounded = raycastHit.collider != null;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundedLayerMask) ||
            Physics2D.OverlapCircle(groundCheckL.position, groundCheckRadius, groundedLayerMask) ||
            Physics2D.OverlapCircle(groundCheckR.position, groundCheckRadius, groundedLayerMask);
        posVelocity = rb2d.velocity.y >= 0.0f ? true : false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = (isGrounded) ? Color.green : Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawSphere(groundCheckL.position, groundCheckRadius);
        Gizmos.DrawSphere(groundCheckR.position, groundCheckRadius);
    }

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

    private void Jump()
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
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && !posVelocity && isGrounded)
        {
            this.transform.parent = other.gameObject.transform;
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && isGrounded)
        {
            if (dropThrough && !dropThroughCoroutineIsRunning)
                StartCoroutine("DropThrough");
        }
    }

    IEnumerator DropThrough()
    {
        Debug.Log("start");
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
        Debug.Log("end");
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            this.transform.parent = null;
        }
    }
}

// Debug logs for groundedCheck:
        // Color rayColor;
        // if (raycastHit.collider != null)
        // {
        //     rayColor = Color.green;
        // }
        // else
        // {
        //     rayColor = Color.red;
        // }
        // Debug.DrawRay(boxCollider2D.bounds.center + new Vector3(boxCollider2D.bounds.extents.x,0), Vector2.down * (boxCollider2D.bounds.extents.y + extraHeightTest), rayColor);
        // Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x,0), Vector2.down * (boxCollider2D.bounds.extents.y + extraHeightTest), rayColor);
        // Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, boxCollider2D.bounds.extents.y + extraHeightTest), Vector2.right * (boxCollider2D.bounds.extents.x), rayColor);
        // Debug.Log(raycastHit.collider);