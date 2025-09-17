using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tower — стрельба, поиск цели, апгрейд и продажа.
/// Поддерживает конфигурацию в инспекторе: базовая стоимость, множители апгрейда, макс уровень.
/// UI должен вызывать OnUpgradeButton / OnSellButton при клике.
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _towerPlace;
    [SerializeField] private SpriteRenderer _towerHead;

    [Header("Combat (editable per-prefab)")]
    [SerializeField] private int _shootPower = 1;
    [SerializeField] private float _shootDistance = 1f;
    [SerializeField] private float _shootDelay = 1f;
    [SerializeField] private float _bulletSpeed = 5f;
    [SerializeField] private float _bulletSplashRadius = 0f; // 0 = single-target

    [Header("Economy")]
    [SerializeField] private int _basePurchaseCost = 10; // цена покупки (инспектор)
    [SerializeField] private int _level = 0;
    [SerializeField] private int _maxLevel = 3;

    [Header("Upgrade multipliers (per level)")]
    [Tooltip("Множитель для урона на каждый уровень (применяется к текущему урону)")]
    [SerializeField] private float _upgradePowerMultiplier = 1.5f;
    [Tooltip("Множитель для дальности (перемножается)")]
    [SerializeField] private float _upgradeRangeMultiplier = 1.12f;
    [Tooltip("Множитель для задержки стрельбы (уменьшается, ставьте <1 чтобы стрелять быстрее)")]
    [SerializeField] private float _upgradeDelayMultiplier = 0.9f;
    [Tooltip("Множитель для стоимости апгрейда (цена растёт экспоненциально)")]
    [SerializeField] private float _upgradeCostMultiplier = 1.7f;

    [Header("Runtime / Pooling")]
    [SerializeField] private Bullet _bulletPrefab;

    private float _runningShootDelay = 0f;
    private Enemy _targetEnemy;

    // Интерфейс для внешних систем
    public Vector2? PlacePosition { get; private set; }
    public int TowerCost => _basePurchaseCost;
    public int CurrentLevel => _level;
    public int MaxLevel => _maxLevel;

    private void Start()
    {
        // Сбрасываем таймер так, чтобы башня могла стрелять сразу после постройки
        _runningShootDelay = 0f;
    }

    #region Placement / visuals
    public Sprite GetTowerHeadIcon()
    {
        return _towerHead != null ? _towerHead.sprite : null;
    }

    public void SetPlacePosition(Vector2? newPosition)
    {
        PlacePosition = newPosition;
    }

    public void LockPlacement()
    {
        if (PlacePosition.HasValue)
            transform.position = (Vector2)PlacePosition;
    }

    public void ToggleOrderInLayer(bool toFront)
    {
        if (_towerPlace != null) _towerPlace.sortingOrder = toFront ? 1 : 0;
        if (_towerHead != null) _towerHead.sortingOrder = toFront ? 2 : 1;
    }
    #endregion

    #region Targeting / shooting
    public void CheckNearestEnemy(List<Enemy> enemies)
    {
        if (_targetEnemy != null)
        {
            if (!_targetEnemy.gameObject.activeSelf ||
                Vector3.Distance(transform.position, _targetEnemy.transform.position) > _shootDistance)
            {
                _targetEnemy = null;
            }
            else
            {
                return;
            }
        }

        float nearestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeSelf) continue;
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > _shootDistance) continue;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        _targetEnemy = nearestEnemy;
    }

    public void SeekTarget() { } // заглушка — повороты не нужны для топ-дауна

    public void ShootTarget()
    {
        if (_targetEnemy == null) return;

        _runningShootDelay -= Time.unscaledDeltaTime;
        if (_runningShootDelay > 0f) return;

        if (_bulletPrefab == null)
        {
            Debug.LogError("Tower: bullet prefab not set.", this);
            return;
        }

        Bullet bullet = LevelManager.Instance.GetBulletFromPool(_bulletPrefab);
        if (bullet == null)
        {
            Debug.LogError("Tower: failed to get bullet from pool.", this);
            return;
        }

        bullet.transform.position = transform.position;
        bullet.SetProperties(_shootPower, _bulletSpeed, _bulletSplashRadius);
        bullet.SetTargetEnemy(_targetEnemy);
        bullet.gameObject.SetActive(true);

        _runningShootDelay = _shootDelay;
    }
    #endregion

    #region Upgrade / Sell API
    // public getter для стоимости апгрейда (можно показывать в UI)
    public int GetUpgradeCost()
    {
        if (_level >= _maxLevel) return 0;
        // цена апгрейда: basePurchaseCost * upgradeCostMultiplier^currentLevel, округляем вверх
        float raw = _basePurchaseCost * Mathf.Pow(_upgradeCostMultiplier, _level);
        return Mathf.CeilToInt(raw);
    }

    public bool CanUpgrade()
    {
        if (_level >= _maxLevel) return false;
        if (LevelManager.Instance == null) return false;
        int cost = GetUpgradeCost();
        return LevelManager.Instance.CoinsSafe >= cost;
    }

    public void Upgrade()
    {
        if (_level >= _maxLevel)
        {
            Debug.Log("Tower: already at max level.", this);
            return;
        }

        int cost = GetUpgradeCost();
        if (LevelManager.Instance == null)
        {
            Debug.LogError("Tower.Upgrade: LevelManager not found.", this);
            return;
        }

        if (!LevelManager.Instance.SpendCoins(cost))
        {
            Debug.Log("Tower: not enough coins to upgrade.", this);
            return;
        }

        // Применяем улучшения — округления вверх там, где нужен int
        _shootPower = Mathf.CeilToInt(_shootPower * _upgradePowerMultiplier);
        _shootDistance *= _upgradeRangeMultiplier;
        // уменьшаем задержку (если множитель <1), округляем к разумному
        _shootDelay = Mathf.Max(0.05f, _shootDelay * _upgradeDelayMultiplier);

        _level++;
        Debug.Log($"Tower upgraded to level {_level}. New stats: power={_shootPower}, range={_shootDistance:F2}, delay={_shootDelay:F2}", this);
    }

    // Возвращает сумму, которую игрок получит при продаже (и проводит само удаление/деактивацию)
    public int GetSellPrice()
    {
        // продаём по половине от базовой цены (purchase cost), округляем вверх
        return Mathf.CeilToInt(_basePurchaseCost * 0.5f);
    }

public void Sell()
{
    if (LevelManager.Instance != null)
    {
        int price = GetSellPrice();
        LevelManager.Instance.AddCoins(price);
        LevelManager.Instance.UnregisterSpawnedTower(this);
    }
    Destroy(gameObject); // или SetActive(false) если пул
}

    // UI callbacks (подвешивайте на кнопки)
    public void OnUpgradeButton()
    {
        Upgrade();
    }

    public void OnSellButton()
    {
        Sell();
    }
    #endregion
}
