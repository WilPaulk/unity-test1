using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;

    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private int extraJumpNum = 2;
    private int extraJumpsRemaining;

    [SerializeField] bool isGrounded;
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
        if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")) ||
          (Physics2D.Linecast(transform.position, groundCheckL.position, 1 << LayerMask.NameToLayer("Ground"))) ||
          (Physics2D.Linecast(transform.position, groundCheckR.position, 1 << LayerMask.NameToLayer("Ground"))))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && isGrounded)
        {
            this.transform.parent = other.gameObject.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && isGrounded == false)
        {
            this.transform.parent = null;
        }
    }
}