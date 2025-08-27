using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float attackRadius = 2f;
    public LayerMask buildingMask;
    public LayerMask antMask;
    private Transform targetPoint;

    public void SetTarget(Transform target)
    {
        targetPoint = target;
    }

    void FixedUpdate()
    {
        if (targetPoint == null) return;

        // Move towards target, avoiding obstacles
        Vector2 dir = (targetPoint.position - transform.position).normalized;
        Vector2 move = dir * moveSpeed * Time.fixedDeltaTime;

        // Simple obstacle avoidance
        Collider2D obstacle = Physics2D.OverlapCircle(transform.position + (Vector3)dir, 0.5f, buildingMask);
        if (obstacle != null)
        {
            // Try to steer around
            move += (Vector2)Vector3.Cross(dir, Vector3.forward) * moveSpeed * 0.5f * Time.fixedDeltaTime;
        }

        transform.position += (Vector3)move;

        // Attack buildings in radius
        Collider2D building = Physics2D.OverlapCircle(transform.position, attackRadius, buildingMask);
        if (building != null)
        {
            // Attack logic here
            var health = building.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(1); // Example damage
        }

        // Fight ants in radius
        Collider2D ant = Physics2D.OverlapCircle(transform.position, attackRadius, antMask);
        if (ant != null)
        {
            // Fight logic here
            var health = ant.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(1); // Example damage
        }
    }
}