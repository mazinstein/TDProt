using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public List<UnitController> selectedUnits = new List<UnitController>();

    [Header("UI")]
    public Canvas canvas;             
    public Image selectionBoxImage;   
    private RectTransform selectionBox;

    private Vector3 startPos;
    private bool isDragging = false;

    void Start()
    {
        if (selectionBoxImage != null)
        {
            selectionBox = selectionBoxImage.GetComponent<RectTransform>();
            selectionBox.pivot = new Vector2(0f, 1f);
            selectionBoxImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // ЛКМ нажата → начинаем выделение
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;

            if (selectionBoxImage != null)
                selectionBoxImage.gameObject.SetActive(true);
        }

        // ЛКМ удерживается → обновляем рамку
        if (isDragging && selectionBox != null)
        {
            UpdateSelectionBox(Input.mousePosition);
        }

        // ЛКМ отпущена → выбираем юнитов
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                // если мышь почти не двигалась → считаем это кликом
                if ((Input.mousePosition - startPos).sqrMagnitude < 10f)
                {
                    SelectUnitByClick();
                }
                else
                {
                    SelectUnits();
                }

                isDragging = false;
                if (selectionBoxImage != null)
                    selectionBoxImage.gameObject.SetActive(false);
            }
        }

        // ПКМ → движение выделенных юнитов
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f; 

            bool queue = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            foreach (var unit in selectedUnits)
            {
                unit.MoveTo(worldPos, queue);
            }
        }
    }

    void UpdateSelectionBox(Vector3 currentMousePos)
    {
        float left = Mathf.Min(startPos.x, currentMousePos.x);
        float right = Mathf.Max(startPos.x, currentMousePos.x);
        float top = Mathf.Max(startPos.y, currentMousePos.y);
        float bottom = Mathf.Min(startPos.y, currentMousePos.y);

        selectionBox.position = new Vector3(left, top, 0f);
        selectionBox.sizeDelta = new Vector2(right - left, top - bottom);
    }

    void SelectUnits()
    {
        selectedUnits.Clear();

        Vector3 min = Vector3.Min(startPos, Input.mousePosition);
        Vector3 max = Vector3.Max(startPos, Input.mousePosition);
        Rect selectionRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

        UnitController[] allUnits = Object.FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            if (selectionRect.Contains(screenPos))
            {
                selectedUnits.Add(unit);
            }

            unit.SetSelected(selectedUnits.Contains(unit));
        }
    }

    void SelectUnitByClick()
    {
        selectedUnits.Clear();

        Vector3 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 clickPos = new Vector2(worldClick.x, worldClick.y);

        Collider2D hit = Physics2D.OverlapPoint(clickPos);
        if (hit != null)
        {
            UnitController unit = hit.GetComponent<UnitController>();
            if (unit != null)
            {
                selectedUnits.Add(unit);
            }
        }

        UnitController[] allUnits = Object.FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            unit.SetSelected(selectedUnits.Contains(unit));
        }
    }
}
