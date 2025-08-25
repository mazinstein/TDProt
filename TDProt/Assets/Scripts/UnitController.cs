using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject selectionHighlight; // Assign in Inspector

    private Queue<Vector3> moveQueue = new Queue<Vector3>();
    private Vector3 targetPos;
    private bool isMoving = false;

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
