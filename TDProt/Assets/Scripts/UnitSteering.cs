using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class UnitSteering : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float avoidDistance = 1f;
    public LayerMask obstacleMask;

    private Vector2 targetPos;
    private bool isMoving = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveTo(Vector3 position)
    {
        targetPos = new Vector2(position.x, position.y);
        isMoving = true;
    }

    void FixedUpdate()
    {
        if (!isMoving) return;

        Vector2 currentPos = rb.position;
        Vector2 desiredVelocity = (targetPos - currentPos).normalized * moveSpeed;

        // Obstacle avoidance using a simple forward raycast
        RaycastHit2D hit = Physics2D.Raycast(currentPos, desiredVelocity.normalized, avoidDistance, obstacleMask);
        if (hit.collider != null)
        {
            // Steer away from obstacle
            Vector2 hitNormal = hit.normal;
            desiredVelocity += hitNormal * moveSpeed;
        }

        rb.linearVelocity = desiredVelocity;

        if (Vector2.Distance(currentPos, targetPos) < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            isMoving = false;
        }
    }
}