using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tower: стрельба, поиск цели, покупка/продажа, классический апгрейд по уровням
/// + гибкая система "UpgradeOption" (до 3 иконок) с возможностью менять параметры или заменять префаб.
/// Совместимо с LevelManager интерфейсом:
///  - LevelManager.Instance.SpendCoins(int)
///  - LevelManager.Instance.AddCoins(int)
///  - LevelManager.Instance.RegisterSpawnedTower(Tower)
///  - LevelManager.Instance.UnregisterSpawnedTower(Tower)
///  - LevelManager.Instance.GetBulletFromPool(Bullet)
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
    [SerializeField] private float _bulletSpeed = 7f;
    [SerializeField] private float _bulletSplashRadius = 0f; // 0 = single-target

    [Header("Economy / Purchase")]
    [SerializeField] private int _basePurchaseCost = 10; // цена покупки в инспекторе
    [SerializeField] private int _level = 0;            // классический уровень
    [SerializeField] private int _maxLevel = 3;

    [Header("Classic upgrade multipliers (if using Upgrade())")]
    [SerializeField] private float _upgradePowerMultiplier = 1.5f;
    [SerializeField] private float _upgradeRangeMultiplier = 1.12f;
    [SerializeField] private float _upgradeDelayMultiplier = 0.9f;
    [SerializeField] private float _upgradeCostMultiplier = 1.7f;

    [Header("Upgrade options (3 icons)")]
    [Tooltip("Можно задать до 3 UpgradeOption ассетов (иконка, стоимость, модификаторы, замена префаба).")]
    [SerializeField] private UpgradeOption[] _upgradeOptions = new UpgradeOption[3];

    // internal state of used options
    private bool[] _optionUsed;

    [Header("Bullet")]
    [SerializeField] private Bullet _bulletPrefab;

    // placement support (для drag&drop)
    public Vector2? PlacePosition { get; private set; }

    // shooting runtime
    private float _runningShootDelay = 0f;
    private Enemy _targetEnemy;

    // --- Public accessors for other systems / UI ---
    public int TowerCost => _basePurchaseCost;
    public int CurrentLevel => _level;
    public int MaxLevel => _maxLevel;

    private void Awake()
    {
        // init option flags
        if (_upgradeOptions != null && _upgradeOptions.Length > 0)
            _optionUsed = new bool[_upgradeOptions.Length];
        else
            _optionUsed = new bool[0];

        // allow immediate shot after placement
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

    #region Targeting / Shooting
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

    public void SeekTarget()
    {
        // placeholder for rotation logic (not needed for top-down)
    }

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

        if (LevelManager.Instance == null)
        {
            Debug.LogError("Tower: LevelManager not found when shooting.", this);
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

    #region Classic leveling (optional)
    /// <summary>
    /// Цена классического апгрейда: basePurchaseCost * upgradeCostMultiplier^level (округление вверх)
    /// </summary>
    public int GetUpgradeCost()
    {
        if (_level >= _maxLevel) return 0;
        float raw = _basePurchaseCost * Mathf.Pow(_upgradeCostMultiplier, _level);
        return Mathf.CeilToInt(raw);
    }

    public bool CanUpgrade()
    {
        if (_level >= _maxLevel) return false;
        if (LevelManager.Instance == null) return false;
        return LevelManager.Instance.CoinsSafe >= GetUpgradeCost();
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

        _shootPower = Mathf.CeilToInt(_shootPower * _upgradePowerMultiplier);
        _shootDistance *= _upgradeRangeMultiplier;
        _shootDelay = Mathf.Max(0.01f, _shootDelay * _upgradeDelayMultiplier);

        _level++;
        Debug.Log($"Tower upgraded to level {_level}. power={_shootPower}, range={_shootDistance:F2}, delay={_shootDelay:F2}", this);
    }
    #endregion

    #region Upgrade options (icons)
    public UpgradeOption[] GetUpgradeOptions() => _upgradeOptions;

    public bool IsOptionUsed(int index)
    {
        if (_optionUsed == null || index < 0 || index >= _optionUsed.Length) return true;
        return _optionUsed[index];
    }

    public bool CanApplyOption(int index)
    {
        if (_upgradeOptions == null || index < 0 || index >= _upgradeOptions.Length) return false;
        if (_optionUsed[index]) return false;
        var opt = _upgradeOptions[index];
        if (opt == null) return false;
        if (LevelManager.Instance == null) return false;
        return LevelManager.Instance.CoinsSafe >= opt.cost;
    }

    /// <summary>
    /// Применить опцию апгрейда (иконку) с индексом index.
    /// Снимает деньги через LevelManager.SpendCoins(opt.cost).
    /// Поддерживается простая модификация статов и опциональная замена префаба.
    /// </summary>
    public void ApplyUpgradeOption(int index)
    {
        if (_upgradeOptions == null || index < 0 || index >= _upgradeOptions.Length) return;
        var opt = _upgradeOptions[index];
        if (opt == null) return;
        if (_optionUsed == null) _optionUsed = new bool[_upgradeOptions.Length];

        if (LevelManager.Instance == null)
        {
            Debug.LogError("ApplyUpgradeOption: LevelManager missing.", this);
            return;
        }

        if (_optionUsed[index])
        {
            Debug.Log("ApplyUpgradeOption: option already used.", this);
            return;
        }

        if (!LevelManager.Instance.SpendCoins(opt.cost))
        {
            Debug.Log("ApplyUpgradeOption: not enough coins.", this);
            return;
        }

        // Применяем модификаторы (порядок: add -> mul)
        _shootPower = Mathf.CeilToInt((_shootPower + opt.addPower) * opt.mulPower);
        _shootDistance = (_shootDistance + opt.addRange) * opt.mulRange;
        _shootDelay = Mathf.Max(0.01f, (_shootDelay + opt.addDelay) * opt.mulDelay);

        _optionUsed[index] = true;

        Debug.Log($"Applied upgrade option '{opt.optionId}' on tower {name}.", this);

        // Если опция заменяет префаб — создаём replacementPrefab на том же месте и регистрируем в LevelManager
        if (opt.replaceWithPrefab && opt.replacementPrefab != null)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Transform parent = transform.parent;

            // Убираем старую башню из LevelManager
            LevelManager.Instance.UnregisterSpawnedTower(this);

            // Создаём новую башню
            GameObject go = Instantiate(opt.replacementPrefab.gameObject, pos, rot, parent);
            Tower newTower = go.GetComponent<Tower>();
            if (newTower != null)
            {
                // Возможно, стоит перенести часть состояния сюда (например, used options, уровень и т.п.)
                // Регистрация
                LevelManager.Instance.RegisterSpawnedTower(newTower);
            }
            // Уничтожаем старую
            Destroy(gameObject);
            return;
        }

        // Если не было замены префаба — UI/панель должны обновиться извне (например, TowerPanelUI.Refresh)
    }
    #endregion

    #region Sell / Remove
    /// <summary>
    /// Возвращает цену продажи (половина от базовой покупки, округление вверх)
    /// </summary>
    public int GetSellPrice()
    {
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

        // Если используешь пул для башен — лучше деактивировать вместо Destroy
        Destroy(gameObject);
    }
    #endregion
}
