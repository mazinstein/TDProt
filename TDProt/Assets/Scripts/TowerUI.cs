using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// TowerUI — UI слот магазина для башен.
/// Поддерживает:
///  - один префаб через SetTowerPrefab(Tower)
///  - несколько префабов через SetTowerPrefabs(Tower[])
///  - цикл выбора варианта CycleOption()
///  - drag&drop постройки (BeginDrag/Drag/EndDrag)
/// Backward-compatible: содержит SetTowerPrefab (для LevelManager и старого кода).
/// </summary>
public class TowerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _towerIcon;

    [Tooltip("Если один префаб — поместите его сюда. Можно поместить несколько вариантов.")]
    [SerializeField] private Tower[] _towerPrefabs;

    private int _selectedIndex = 0;
    private Tower _currentSpawnedTower;

    #region Editor helpers
    private void OnValidate()
    {
        // В редакторе поддерживаем видимость иконки
        RefreshIcon();
    }
    #endregion

    #region Public API (backwards-compatible)
    // Совместимый метод: если старый код вызывает SetTowerPrefab(single), он продолжит работать.
    public void SetTowerPrefab(Tower tower)
    {
        if (tower == null)
        {
            _towerPrefabs = new Tower[0];
            _selectedIndex = 0;
        }
        else
        {
            _towerPrefabs = new Tower[] { tower };
            _selectedIndex = 0;
        }
        RefreshIcon();
    }

    // Новый метод: передаём массив вариантов
    public void SetTowerPrefabs(Tower[] towers)
    {
        _towerPrefabs = towers ?? new Tower[0];
        _selectedIndex = 0;
        RefreshIcon();
    }
    #endregion

    #region Icon / selection
    // Показываем иконку текущего выбранного варианта
    public void RefreshIcon()
    {
        if (_towerIcon == null)
            return;

        if (_towerPrefabs == null || _towerPrefabs.Length == 0)
        {
            _towerIcon.sprite = null;
            _towerIcon.color = new Color(1f, 1f, 1f, 0.0f);
            return;
        }

        var t = _towerPrefabs[_selectedIndex];
        if (t != null && t.GetTowerHeadIcon() != null)
        {
            _towerIcon.sprite = t.GetTowerHeadIcon();
            _towerIcon.color = Color.white;
        }
        else
        {
            _towerIcon.sprite = null;
            _towerIcon.color = new Color(1f, 1f, 1f, 0.0f);
        }
    }

    // Нажатие на кнопку рядом со слотом может вызвать CycleOption()
    public void CycleOption()
    {
        if (_towerPrefabs == null || _towerPrefabs.Length == 0) return;
        _selectedIndex = (_selectedIndex + 1) % _towerPrefabs.Length;
        RefreshIcon();
    }
    #endregion

    #region Drag & Drop (постройка)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_towerPrefabs == null || _towerPrefabs.Length == 0) return;

        Tower prefab = _towerPrefabs[_selectedIndex];
        if (prefab == null) return;

        int towerCost = prefab.TowerCost;
        if (!LevelManager.Instance.SpendCoins(towerCost))
        {
            Debug.Log("Недостаточно монет для постройки башни!");
            return;
        }

        GameObject newTowerObj = Instantiate(prefab.gameObject);
        _currentSpawnedTower = newTowerObj.GetComponent<Tower>();
        if (_currentSpawnedTower == null)
        {
            Debug.LogError("TowerUI: instantiated prefab does not contain Tower component.", this);
            Destroy(newTowerObj);
            LevelManager.Instance.AddCoins(towerCost);
            return;
        }

        // Ставим визуально "вперед"
        _currentSpawnedTower.ToggleOrderInLayer(true);

        // Если префаб требует какую-то начальную инициализацию — можно вызвать тут
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower == null) return;

        Camera mainCamera = Camera.main;
        Vector3 mousePosition = Input.mousePosition;
        // корректируем z, чтобы ScreenToWorldPoint работал корректно
        float camZ = -mainCamera.transform.position.z;
        mousePosition.z = camZ;
        Vector3 targetPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        _currentSpawnedTower.transform.position = targetPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower == null) return;

        Tower prefab = (_towerPrefabs != null && _towerPrefabs.Length > 0) ? _towerPrefabs[_selectedIndex] : null;
        int towerCost = prefab != null ? prefab.TowerCost : 0;

        if (_currentSpawnedTower.PlacePosition == null)
        {
            // невалидно — возврат денег и удаление
            Destroy(_currentSpawnedTower.gameObject);
            if (LevelManager.Instance != null)
                LevelManager.Instance.AddCoins(towerCost);
        }
        else
        {
            // фиксируем позицию, регистрируем в LevelManager
            _currentSpawnedTower.LockPlacement();
            _currentSpawnedTower.ToggleOrderInLayer(false);
            LevelManager.Instance?.RegisterSpawnedTower(_currentSpawnedTower);
            _currentSpawnedTower = null;
        }
    }
    #endregion
}
