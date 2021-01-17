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
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private int extraJumpNum = 2;
    private int extraJumpsRemaining;

// Grounded checks
    [SerializeField] bool isGrounded;
    [SerializeField] LayerMask groundedLayerMask;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform groundCheckL;
    [SerializeField] Transform groundCheckR;

// Binary movement variables
    private bool rightMovement;
    private bool leftMovement;
    private bool groundJump;
    private bool extraJump;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rightMovement = Input.GetKey("d") || Input.GetKey("right") ? true : false;
        leftMovement = Input.GetKey("a") || Input.GetKey("left") ? true : false;
        if (Input.GetKeyDown("space") && isGrounded) 
            groundJump = true;
        if (Input.GetKeyDown("space") && isGrounded == false && extraJumpsRemaining > 0)
            extraJump = true;
    }

    private void FixedUpdate()
    {
        GroundedCheck();

        if (isGrounded == false)
        {
            string airAnimation = rb2d.velocity.y > 0 ? "Player_rise" : "Player_fall";
            animator.Play(airAnimation);
        }

        if (rightMovement)
        {
            rb2d.velocity = new Vector2(movementSpeed, rb2d.velocity.y);
            if(isGrounded)
                animator.Play("Player_run");
            spriteRenderer.flipX = false;
        }
        else if (leftMovement)
        {
            rb2d.velocity = new Vector2(-movementSpeed, rb2d.velocity.y);
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

    private void GroundedCheck()
    {
        float extraHeightTest = 0.3f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, extraHeightTest, groundedLayerMask);
        Color rayColor;
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(boxCollider2D.bounds.center + new Vector3(boxCollider2D.bounds.extents.x,0), Vector2.down * (boxCollider2D.bounds.extents.y + extraHeightTest), rayColor);
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x,0), Vector2.down * (boxCollider2D.bounds.extents.y + extraHeightTest), rayColor);
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, boxCollider2D.bounds.extents.y + extraHeightTest), Vector2.right * (boxCollider2D.bounds.extents.x), rayColor);
        Debug.Log(raycastHit.collider);
        isGrounded = raycastHit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && isGrounded)
        {
            this.transform.parent = other.gameObject.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            this.transform.parent = null;
        }
    }
}