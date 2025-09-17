using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _towerIcon;

    private Tower _towerPrefab;
    private Tower _currentSpawnedTower;

    public void SetTowerPrefab(Tower tower)
    {
        _towerPrefab = tower;
        _towerIcon.sprite = tower.GetTowerHeadIcon();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        int towerCost = _towerPrefab.TowerCost;
        if (!LevelManager.Instance.SpendCoins(towerCost))
        {
            Debug.Log("Недостаточно монет для постройки башни!");
            return;
        }

        GameObject newTowerObj = Instantiate(_towerPrefab.gameObject);
        _currentSpawnedTower = newTowerObj.GetComponent<Tower>();
        _currentSpawnedTower.ToggleOrderInLayer(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower == null) return;

        Camera mainCamera = Camera.main;
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        _currentSpawnedTower.transform.position = targetPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentSpawnedTower == null) return;

        int towerCost = _towerPrefab.TowerCost;
        if (_currentSpawnedTower.PlacePosition == null)
        {
            Destroy(_currentSpawnedTower.gameObject);
            LevelManager.Instance.AddCoins(towerCost); // возвращаем монеты
        }
        else
        {
            _currentSpawnedTower.LockPlacement();
            _currentSpawnedTower.ToggleOrderInLayer(false);
            LevelManager.Instance.RegisterSpawnedTower(_currentSpawnedTower);
            _currentSpawnedTower = null;
        }
    }
}
