using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject selectionHighlight; // Assign in Inspector

    // --- Добавьте эти строки ---
    public GameObject healthBarPrefab; // Префаб полоски здоровья (назначьте в инспекторе)
    private HealthBar healthBar;

    private Queue<Vector3> moveQueue = new Queue<Vector3>();
    private Vector3 targetPos;
    private bool isMoving = false;

    void Start()
    {
        // --- Добавьте этот блок ---
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
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                if (moveQueue.Count > 0)
                {
                    targetPos = moveQueue.Dequeue();
                }
                else
                {
                    isMoving = false;
                }
            }
        }
    }

    // Если queue=true → добавляем к текущей очереди
    public void MoveTo(Vector3 position, bool queue = false)
    {
        // Проверяем, свободна ли точка назначения
        RaycastHit2D hit = Physics2D.Raycast(transform.position, position - transform.position, Vector3.Distance(transform.position, position));
        if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
        {
            Debug.Log("Cannot move to the target position, obstacle detected!");
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