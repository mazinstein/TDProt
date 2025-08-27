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
<<<<<<< Updated upstream
        selfCollider = GetComponent<Collider2D>();

=======
>>>>>>> Stashed changes
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

    void Update()
    {
        // Здесь может быть код для Update, если он нужен
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
<<<<<<< Updated upstream
            Vector3 direction = (targetPos - transform.position).normalized;
            float distance = moveSpeed * Time.deltaTime;

            // Проверяем препятствие на пути (игнорируем свой коллайдер)
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance);
            if (hit.collider != null && hit.collider != selfCollider)
            {
                // Останавливаемся у препятствия
                transform.position = hit.point;
                isMoving = false;
                moveQueue.Clear();
                return;
            }

            // Двигаемся к цели, если препятствий нет
            transform.position = Vector3.MoveTowards(transform.position, targetPos, distance);

            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
=======
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (targetPos - transform.position).normalized;
                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            }

            if (Vector2.Distance(transform.position, targetPos) < 0.1f)
>>>>>>> Stashed changes
            {
                if (moveQueue.Count > 0)
                    targetPos = moveQueue.Dequeue();
                else
                    isMoving = false;
            }
        }
    }

    public void MoveTo(Vector3 position, bool queue = false)
    {
<<<<<<< Updated upstream
        // Проверяем, не внутри ли препятствия целевая точка (игнорируем свой коллайдер)
        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit != selfCollider)
=======
        RaycastHit2D hit = Physics2D.Raycast(transform.position, position - transform.position, Vector3.Distance(transform.position, position));
        if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
>>>>>>> Stashed changes
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

        if (!isMoving)
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