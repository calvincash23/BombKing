using UnityEngine;
using System;
using System.Collections;
using System.Xml.Linq;
using Unity.VisualScripting;

public class Bomb : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;
    private SpriteRenderer sprite;
    private CapsuleCollider2D capsuleCollider;

    public LayerMask enemyLayer;
    private ContactFilter2D contactFilter;

    public bool isLit;

    [Header("Bomb Strength")]
    [SerializeField] public float bombKnockback;

    [Header("Flight path")]
    [SerializeField] public float verticalBoost = 2f;

    [Header("Animations")]
    [SerializeField] private float fuseTimer;
    public AnimationClip explodeClip;

    private void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    void Start()
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();
        //anim = gameObject.GetComponent<Animator>();
        if (!anim)
        {
            Debug.Log("No animator");
        }
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        isLit = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (anim == null)
        {
            Debug.LogError("Animator component is missing from the Bomb prefab!");
        }
    }

    public void smack(Vector2 direction, float attackPower)
    {

        rb.linearVelocity = Vector2.zero;

        Vector2 arcForce = new Vector2(direction.normalized.x * attackPower, verticalBoost * attackPower);

        rb.AddForce(arcForce, ForceMode2D.Impulse);

        // START FUSE
        StartCoroutine(LightFuse());
    }

    private IEnumerator Explode()
    {
        anim.SetTrigger("Explode");
        float animLength = explodeClip.length;
        checkCollisions();
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // freeze in position
        yield return new WaitForSeconds(0.5f); // Wait for animation to finish

        // check collisions
        //checkCollisions();

        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);

    }

    private void checkCollisions()
    {
        capsuleCollider.enabled = true;

        // Set up layer mask
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Hitable", "Player"));
        contactFilter.useLayerMask = true;

        Collider2D[] hitResults = new Collider2D[10];

        // Use OverlapCollider to detect all hits
        Physics2D.OverlapCollider(capsuleCollider, contactFilter, hitResults);

        //if (typeof(ColliderHit) == typeof(CircleCollider2D)) { Debug.Log("hit"); }

        foreach (Collider2D enemy in hitResults)
        {
            // Explode bombs away
            if (enemy != null && (enemy.CompareTag("Bomb") || enemy.CompareTag("PlayerBomb")))
            {
                Vector3 vectorBetween = (enemy.transform.position - transform.position) * bombKnockback;
                enemy.attachedRigidbody.AddForce(vectorBetween, ForceMode2D.Impulse);

                Bomb bomb = enemy.GetComponent<Bomb>(); // Get the Bomb component from the enemy GameObject
                if (bomb != null && !bomb.isLit) // Ensure the Bomb component exists and is not lit
                {
                    bomb.StartCoroutine(bomb.LightFuse()); // Start the LightFuse coroutine on the Bomb script
                }
            };

            // Detect player hit
            if (enemy != null && enemy.CompareTag("Player"))
            {
                enemy.gameObject.GetComponent<Health>()?.hit();

            }

            // Detect enemy hit
        }

        //Destroy(gameObject);
    }

    public void StartFuse()
    {
        StartCoroutine(LightFuse());
    }
    public IEnumerator LightFuse()
    {
        isLit = true;

        //Debug.Log("Timer started!");
        anim.SetBool("isLit", true);
        yield return new WaitForSeconds(fuseTimer); // Waits for 3 seconds
        //Debug.Log("3 seconds have passed!");
        anim.SetBool("isLit", false);

        // EXPLODE
        StartCoroutine(Explode());
    }
}
