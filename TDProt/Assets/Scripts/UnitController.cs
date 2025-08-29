using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject selectionHighlight; // Assign in Inspector

    public GameObject healthBarPrefab; // Префаб полоски здоровья (назначьте в инспекторе)
    public HealthBar healthBar;

    private Queue<Vector3> moveQueue = new Queue<Vector3>();
    private Vector3 targetPos;
    private bool isMoving = false;

    private Collider2D selfCollider;

    void Start()
    {
        selfCollider = GetComponent<Collider2D>();

        if (healthBarPrefab != null)
        {
            var bar = Instantiate(healthBarPrefab, transform);
            bar.transform.localPosition = new Vector3(0, 1.5f, 0); // над головой
            healthBar = bar.GetComponent<HealthBar>();
            var health = GetComponent<Health>();
            if (healthBar != null && health != null)
                healthBar.health = health;
        }
    }

    void FixedUpdate()
    {
        if (!isMoving)
            return;

        Vector3 direction = (targetPos - transform.position).normalized;
        float distance = moveSpeed * Time.fixedDeltaTime;

        // Проверяем препятствие на пути (игнорируем свой коллайдер)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance);
        if (hit.collider != null && hit.collider != selfCollider)
        {
            transform.position = hit.point;
            isMoving = false;
            moveQueue.Clear();
            return;
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.MovePosition(rb.position + (Vector2)direction * distance);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, distance);
        }

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if (moveQueue.Count > 0)
                targetPos = moveQueue.Dequeue();
            else
                isMoving = false;
        }
    }

    public void MoveTo(Vector3 position, bool queue = false)
    {
        // Проверяем, не внутри ли препятствия целевая точка (игнорируем свой коллайдер)
        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit != selfCollider)
        {
            Debug.Log("Target position is inside a collider! Movement cancelled.");
            return;
        }

        if (queue)
        {
            moveQueue.Enqueue(position);
        }
        else
        {
            moveQueue.Clear();
            moveQueue.Enqueue(position);
        }

        if (!isMoving && moveQueue.Count > 0)
        {
            targetPos = moveQueue.Dequeue();
            isMoving = true;
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(selected);
    }
}