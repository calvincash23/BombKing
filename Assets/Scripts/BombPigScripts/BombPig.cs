using UnityEngine;
using UnityEngine.U2D;
using System.Collections;

public class BombPig : MonoBehaviour
{
    public Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private GameObject rangeObject;
    private GameObject bombPlacementObject;
    private GameObject playerObject;
    private LayerMask obstructionLayers;

    private bool canAttack;
    public enum State { Idle, Patrolling, Attack, Reload }
    State currentState;

    [Header("Attack")]
    [SerializeField] public float attackRange = 12f;
    [SerializeField] public float attackSpeed;

    [Header("Movement")]
    [SerializeField] public float movementSpeed;
    [SerializeField] public float patrolDistance;
    private Vector2 homePoint; // Use BombPig's own starting position as the patrol center
    private bool movingRight = true;

    [Header("Bomb Details")]
    public GameObject bombPrefab;
    [SerializeField] public float launchForce; // Adjust as needed

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerObject = GameObject.FindGameObjectWithTag("Player");

        rangeObject = transform.Find("Range")?.gameObject;
        bombPlacementObject = transform.Find("BombPlacement")?.gameObject;
        if (rangeObject != null)
        {
            rangeObject.transform.localScale = new Vector3(attackRange, attackRange, attackRange);
        }
        else
        {
            Debug.Log("No range object found");
        }

        obstructionLayers = LayerMask.GetMask("Player", "Wall");

        // Use BombPig's starting position as its home point for patrolling.
        homePoint = transform.position;

        currentState = State.Idle;
        canAttack = true;
    }

    void Update()
    {
        CheckForEnemey();
    }

    private void CheckForEnemey()
    {
        // Raycast toward the player with a set attack range.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, playerObject.transform.position - transform.position, attackRange, obstructionLayers);
        Debug.DrawRay(transform.position, (playerObject.transform.position - transform.position).normalized * attackRange, Color.red);

        float distanceToPlayer = Vector2.Distance(transform.position, playerObject.transform.position);

        // If player is detected within range, attack.
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = State.Attack;
                Attack();
            }
        }
        else
        {
            Patrol();
        }
    }

    private void Attack()
    {
        if (canAttack)
        {
            Debug.Log("attack");
            anim.SetTrigger("Attack"); // Trigger the attack animation.

            canAttack = false;
            StartCoroutine(Reload());
        }
    }

    public void ThrowStraight()
    {
        // Create bomb and determine the direction toward the player.
        Vector2 directionToPlayer = (playerObject.transform.position - transform.position).normalized;
        Vector2 bombPosition = (Vector2)bombPlacementObject.transform.position;
        GameObject bomb = Instantiate(bombPrefab, bombPosition, Quaternion.identity);

        // Temporarily disable collision between BombPig and the bomb.
        Collider2D bombCollider = bomb.GetComponent<Collider2D>();
        Collider2D enemyCollider = GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(bombCollider, enemyCollider, true);
        StartCoroutine(ReenableCollision(bombCollider, enemyCollider));

        Debug.DrawRay(bombPosition, Vector3.up * 0.5f, Color.red, 2f); // Draw a small vertical debug line.

        Rigidbody2D bombRb = bomb.GetComponent<Rigidbody2D>();
        bombRb.AddForce(directionToPlayer * launchForce, ForceMode2D.Impulse);

        // Light the fuse on the bomb.
        Bomb bombScript = bomb.GetComponent<Bomb>();
        bombScript.StartFuse();
    }

    private void ThrowArc()
    {
        // Implement arc throw if needed.
    }

    private void ThrowHighArc()
    {
        // Implement high arc throw if needed.
    }

    private IEnumerator Reload()
    {
        Debug.Log("in reload");
        yield return new WaitForSeconds(attackSpeed);
        canAttack = true;
    }

    private void Patrol()
    {
        float currentX = transform.position.x;

        // Check boundaries relative to BombPig's home point.
        if (currentX >= homePoint.x + patrolDistance)
        {
            movingRight = false;
            Debug.Log("Right limit reached, moving left");
        }
        else if (currentX <= homePoint.x - patrolDistance)
        {
            movingRight = true;
            Debug.Log("Left limit reached, moving right");
        }

        // Flip the sprite to face the correct direction.
        if (movingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            // Optionally, add movement code (e.g., transform.Translate(Vector2.right * movementSpeed * Time.deltaTime);)
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            // Optionally, add movement code (e.g., transform.Translate(Vector2.left * movementSpeed * Time.deltaTime);)
        }
    }

    private IEnumerator ReenableCollision(Collider2D bombCollider, Collider2D enemyCollider)
    {
        yield return new WaitForSeconds(0.1f); // Adjust delay as needed.
        Physics2D.IgnoreCollision(bombCollider, enemyCollider, false);
    }
}
