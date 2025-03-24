using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    [SerializeField] private bool isGrounded;
    PlayerAttack attackScript;

    int jumpCount;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Dash")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashLength;
    [SerializeField] private float dashCooldown;
    private bool canDash;
    private bool isInvincible;
    [SerializeField] private TrailRenderer tr;

    [Header("Bomb Placement")]
    public GameObject bombPrefab;
    public float bombCountMax = 2;
    private float bombCount = 0;
    private float bombPlacementOffset = 1.0f;

    private void Start()
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();
        anim = gameObject.GetComponent<Animator>();
        attackScript = GetComponent<PlayerAttack>();
        isGrounded = true;
        canDash = true;
        jumpCount = 0;
    }

    //private void FixedUpdate()
    //{
    //    HandleInput();
    //}

    // Update method to check input
    void Update()
    {
        HandleInput();

        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        bombCount = GameObject.FindGameObjectsWithTag("PlayerBomb").Length;

    }

    private void HandleInput()          
    {
        // Horizontal Movement - Allow movement only if grounded or not near a wall
        float horizontal = 0f;

        horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        //Movemwnt
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // Flip player sprite based on movement direction
        if (horizontal > 0.1f)
        {
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
            anim.SetFloat("xVelocity", 1f);
        }
        else if (horizontal < -0.1f)
        {
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
            anim.SetFloat("xVelocity", 1f);
        }
        else
        {
            anim.SetFloat("xVelocity", 0f);
        }

        // JUMPING
        if (Input.GetKeyDown(KeyCode.W) && isGrounded || Input.GetKeyDown(KeyCode.W) && jumpCount <= 1)
        {
            Jump();
            jumpCount ++;
            anim.SetBool("isJumping", true);
        }

        // DASH
        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            Vector2 dashDirection = new Vector2(horizontal, 0.0f);

            if (dashDirection != Vector2.zero)
            {
                StartCoroutine(Dash(dashDirection));
            }
        }

        // ATTACK
        if (Input.GetKeyDown(KeyCode.E))
        {
            attackScript.Attack();
            anim.SetBool("isJumping", isGrounded);

        }

        // PLACE BOMB
        if (Input.GetKeyDown(KeyCode.Q) && bombCount < bombCountMax) { 
            attackScript.PlaceBomb();
        }

    }

    private void Jump()
    {
        // Apply force upwards to simulate a jump
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireCube(attackPoint.position, new Vector2(attackRange/2, attackRange));
        //Gizmos.DrawSphere(attackPoint.position, attackRange);
    }

    private IEnumerator Dash(Vector2 direction)
    {
        anim.SetTrigger("DashTrigger");

        canDash = false;
        isInvincible = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashTimer = 0f;
        Vector2 initialVelocity = rb.linearVelocity; // Store the initial velocity
        Vector2 targetVelocity = direction.normalized * dashForce; // Calculate target velocity

        // Enable dash trail
        tr.emitting = true;

        // Gradually apply velocity over the dash duration
        while (dashTimer < dashLength)
        {
            dashTimer += Time.deltaTime;
            rb.linearVelocity = Vector2.Lerp(initialVelocity, targetVelocity, dashTimer / dashLength); // Smoothly interpolate velocity
            yield return null; // Wait for the next frame
        }

        // Ensure the final velocity is applied
        rb.linearVelocity = targetVelocity;

        // Wait for the dash duration to end
        yield return new WaitForSeconds(dashLength - dashTimer);

        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isInvincible = false;

        // Wait for dash cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        isGrounded = true;
        jumpCount = 0;
        anim.SetBool("isJumping", false);
    }

}
