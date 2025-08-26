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

        // ЛКМ отпущена → выделение рамкой или клик
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging && (Vector3.Distance(startPos, Input.mousePosition) > 10f))
            {
                SelectUnits(); // рамка
            }
            else
            {
                SelectUnitByClick(); // одиночный клик
            }

            isDragging = false;

            if (selectionBoxImage != null)
                selectionBoxImage.gameObject.SetActive(false);
        }

        // ПКМ → движение выделенных юнитов
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f; // для 2D

            if (selectedUnits.Count <= 9)
                MoveUnitsInFormation(worldPos);
            else
                MoveUnitsInCircle(worldPos);
        }
    }

    void UpdateSelectionBox(Vector3 currentMousePos)
    {
        float x = Mathf.Min(startPos.x, currentMousePos.x);
        float y = Mathf.Max(startPos.y, currentMousePos.y);

        float width = Mathf.Abs(startPos.x - currentMousePos.x);
        float height = Mathf.Abs(startPos.y - currentMousePos.y);

        selectionBox.position = new Vector3(x, y, 0f);
        selectionBox.sizeDelta = new Vector2(width, height);
    }

    void SelectUnits()
    {
        selectedUnits.Clear();

        Vector3 min = Vector3.Min(startPos, Input.mousePosition);
        Vector3 max = Vector3.Max(startPos, Input.mousePosition);
        Rect selectionRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

        UnitController[] allUnits = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        selectedUnits.Clear();

        if (hit.collider != null)
        {
            UnitController unit = hit.collider.GetComponent<UnitController>();
            if (unit != null)
            {
                selectedUnits.Add(unit);
            }
        }

        UnitController[] allUnits = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            unit.SetSelected(selectedUnits.Contains(unit));
        }
    }

    // --- Расстановка юнитов ---

    void MoveUnitsInFormation(Vector3 center)
    {
        int rows = Mathf.CeilToInt(Mathf.Sqrt(selectedUnits.Count));
        float spacing = 1.5f;

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            int row = i / rows;
            int col = i % rows;

            Vector3 target = center + new Vector3(col * spacing, -row * spacing, 0f);
            selectedUnits[i].MoveTo(target, false);
        }
    }

    void MoveUnitsInCircle(Vector3 center)
    {
        float radius = 2f + selectedUnits.Count * 0.1f;

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            float angle = (i / (float)selectedUnits.Count) * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            selectedUnits[i].MoveTo(center + offset, false);
        }
    }
}
