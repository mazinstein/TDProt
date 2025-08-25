using UnityEngine;

public class CameraEdgeMove2D : MonoBehaviour
{
    public float moveSpeed = 20f;       // скорость движения камеры
    public float borderThickness = 20f; // зона у краёв экрана
    public float smoothness = 5f;       // плавность движения

    [Header("Ограничения карты (X/Y)")]
    public bool useBounds = true;
    public Vector2 xLimits = new Vector2(-50, 50);
    public Vector2 yLimits = new Vector2(-50, 50);

    private Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        Vector3 move = Vector3.zero;

        // --- edge panning (движение по краям экрана)
        if (Input.mousePosition.x >= Screen.width - borderThickness)
            move.x += moveSpeed * Time.deltaTime;
        if (Input.mousePosition.x <= borderThickness)
            move.x -= moveSpeed * Time.deltaTime;
        if (Input.mousePosition.y >= Screen.height - borderThickness)
            move.y += moveSpeed * Time.deltaTime;
        if (Input.mousePosition.y <= borderThickness)
            move.y -= moveSpeed * Time.deltaTime;

        // новая позиция
        targetPos += move;

        // --- ограничения карты
        if (useBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, xLimits.x, xLimits.y);
            targetPos.y = Mathf.Clamp(targetPos.y, yLimits.x, yLimits.y);
        }
        targetPos.z = -10f; // фиксированная глубина для 2D камеры

        // --- плавное движение
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothness);
    }
}
