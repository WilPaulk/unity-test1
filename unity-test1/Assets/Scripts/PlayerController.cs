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


    [SerializeField] bool isGrounded;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform groundCheckL;
    [SerializeField] Transform groundCheckR;

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

    }

    private void FixedUpdate()
    {
        if(Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")) ||
         (Physics2D.Linecast(transform.position, groundCheckL.position, 1 << LayerMask.NameToLayer("Ground"))) ||
         (Physics2D.Linecast(transform.position, groundCheckR.position, 1 << LayerMask.NameToLayer("Ground"))))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
            if(rb2d.velocity.y >0)
            {
                animator.Play("Player_rise");
            }
            else
            {
                animator.Play("Player_fall");
            }
        }

        if(Input.GetKey("d") || Input.GetKey("right"))
        {
            rb2d.velocity = new Vector2(movementSpeed, rb2d.velocity.y);
            if(isGrounded)
                animator.Play("Player_run");
            spriteRenderer.flipX = false;
        }
        else if(Input.GetKey("a") || Input.GetKey("left"))
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

        if(Input.GetKey("space") && isGrounded)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpHeight);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Platform") && isGrounded)
        {
            this.transform.parent = other.gameObject.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Platform") && isGrounded == false)
        {
            this.transform.parent = null;
        }
    }
}