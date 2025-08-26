using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(UnitController))]
public class Combat : MonoBehaviour
{
    public int damage = 10;
    public float attackRange = 1.5f;
    public float visionRange = 5f;
    public float attackRate = 1f;

    private float nextAttackTime = 0f;
    private Transform target;
    private Health targetHealth;
    private UnitController unitController;

    // статический словарь: кто атакует какого врага
    private static Dictionary<Transform, List<Combat>> attackers = new Dictionary<Transform, List<Combat>>();

    void Start()
    {
        unitController = GetComponent<UnitController>();
    }

    void Update()
    {
        if (target == null)
        {
            FindTarget();
        }
        else
        {
            float dist = Vector3.Distance(transform.position, target.position);

            if (targetHealth == null || targetHealth.IsDead() || dist > visionRange)
            {
                ReleaseTarget();
                return;
            }

            if (dist > attackRange)
            {
                // получаем "свою" позицию вокруг врага
                Vector3 attackPos = GetFormationPosition();
                unitController.MoveTo(attackPos, false);
            }
            else
            {
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + 1f / attackRate;
                }
            }
        }
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange);

        float closestDist = Mathf.Infinity;
        Transform closestTarget = null;
        Health closestHealth = null;

        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            if ((gameObject.CompareTag("Unit") && hit.CompareTag("Enemy")) ||
                (gameObject.CompareTag("Enemy") && hit.CompareTag("Unit")))
            {
                float d = Vector3.Distance(transform.position, hit.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    closestTarget = hit.transform;
                    closestHealth = hit.GetComponent<Health>();
                }
            }
        }

        if (closestTarget != null)
        {
            target = closestTarget;
            targetHealth = closestHealth;

            // регистрируемся как атакующий
            if (!attackers.ContainsKey(target))
                attackers[target] = new List<Combat>();
            attackers[target].Add(this);
        }
    }

    void ReleaseTarget()
    {
        if (target != null && attackers.ContainsKey(target))
        {
            attackers[target].Remove(this);
            if (attackers[target].Count == 0)
                attackers.Remove(target);
        }

        target = null;
        targetHealth = null;
    }

    Vector3 GetFormationPosition()
    {
        if (target == null) return transform.position;

        List<Combat> group = attackers[target];
        int index = group.IndexOf(this);
        int count = group.Count;

        // угол на окружности
        float angle = (360f / count) * index;
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * (attackRange * 0.8f);
        return target.position + offset;
    }

    void Attack()
    {
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }
    }

    void OnDestroy()
    {
        ReleaseTarget();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
