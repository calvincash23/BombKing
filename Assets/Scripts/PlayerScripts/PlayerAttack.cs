using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [System.Serializable]
    public class AttackColliderData
    {
        public PolygonCollider2D collider;
        // This vector represents the direction the bomb should be sent when hit by this collider.
        public Vector2 smackDirection;
    }

    public Rigidbody2D rb;
    private Animator anim;

    [Header("Attack")]
    public Transform attackPoint;
    [SerializeField] private float attackSpeed;
    [SerializeField] public float attackPower;
    public LayerMask enemyLayer;
    private ContactFilter2D contactFilter;

    // Instead of separate fields for high, mid, and low, we use a list.
    [Header("Attack Colliders")]
    public List<AttackColliderData> attackColliders = new List<AttackColliderData>();

    [Header("Bomb Placement")]
    public GameObject bombPrefab;
    public float bombCountMax = 2;
    private float bombCount = 0;
    private float bombPlacementOffset = 1.0f;

    private void Start()
    {
        anim = GetComponent<Animator>();

        // Disable all attack colliders by default.
        foreach (var ac in attackColliders)
        {
            if (ac.collider != null)
                ac.collider.enabled = false;
        }
    }

    void Update()
    {
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        bombCount = GameObject.FindGameObjectsWithTag("PlayerBomb").Length;
    }

    public void Attack()
    {
        Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        anim.SetTrigger("AttackTrigger");

        // Set up the contact filter for hit detection.
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Hitable"));
        contactFilter.useLayerMask = true;

        // Iterate over each attack collider.
        foreach (var ac in attackColliders)
        {
            if (ac.collider == null) continue;

            ac.collider.enabled = true;
            Collider2D[] hitResults = new Collider2D[10];

            // Use OverlapCollider on this specific collider.
            Physics2D.OverlapCollider(ac.collider, contactFilter, hitResults);

            foreach (Collider2D enemy in hitResults)
            {
                if (enemy != null && (enemy.CompareTag("Bomb") || enemy.CompareTag("PlayerBomb")))
                {
                    Bomb bomb = enemy.GetComponent<Bomb>();
                    // Use the smackDirection specified for this collider.
                    bomb.smack(ac.smackDirection* facingDirection, attackPower);

                    Debug.DrawLine(
                        bomb.transform.position,
                        bomb.transform.position + new Vector3(ac.smackDirection.normalized.x * facingDirection.x, ac.smackDirection.normalized.y, 0) * 2f,
                        Color.red,
                        2f
                    );
                }
            }

            ac.collider.enabled = false;
        }
    }

    public void PlaceBomb()
    {
        Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 bombPosition = (Vector2)transform.position + (facingDirection * bombPlacementOffset);
        Instantiate(bombPrefab, bombPosition, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        // Optional: Add Gizmos to visualize attack range or collider positions.
    }
}
