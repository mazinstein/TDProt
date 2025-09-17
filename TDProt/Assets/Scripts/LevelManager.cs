using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LevelManager — контролирует UI, жизнь/монеты, логику башен и движение врагов.
/// Спавн врагов теперь осуществляется внешним Spawner. Spawner должен вызывать RegisterSpawnedEnemy(...).
/// Когда Spawner закончит спавнить всех врагов, он должен вызвать SetAllEnemiesSpawned().
/// </summary>
public class LevelManager : MonoBehaviour
{
    // Singleton
    private static LevelManager _instance = null;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<LevelManager>();
            return _instance;
        }
    }

    [Header("Tower UI")]
    [SerializeField] private Transform _towerUIParent;
    [SerializeField] private GameObject _towerUIPrefab;
    [SerializeField] private Tower[] _towerPrefabs;

    private List<Tower> _spawnedTowers = new List<Tower>();

    [Header("Enemies (for compatibility)")]
    // В большинстве случаев спавн делегирован Spawner; оставляем массив префабов как запасной вариант
    [SerializeField] private Enemy[] _enemyPrefabs;
    [SerializeField] private Transform[] _enemyPaths;

    // Старый таймер спавна больше не используется — спавн контролирует Spawner
    // [SerializeField] private float _spawnDelay = 5f;

    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private List<Bullet> _spawnedBullets = new List<Bullet>();

    public bool IsOver { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _totalEnemy = 15; // остаётся, чтобы показывать в UI при необходимости

    [Header("UI Elements")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;

    [Header("Economy")]
    [SerializeField] private int _startCoins = 5; // стартовое количество монет
    [SerializeField] private TextMeshProUGUI _coinsInfo;
    private int _currentCoins;

    private int _currentLives;

    // Флаги/состояния для интеграции со Spawner
    private bool _allEnemiesSpawned = false; // spawner установит true, когда больше не будет спавнов
    // Примечание: "_enemyCounter" можно оставить для отображения в UI, но реальное спавниг-логика вне LevelManager
    private int _enemyCounter;
    private int killedEnemies = 0;

    [Header("Victory UI")]
    [SerializeField] private GameObject victoryPanel;

    private void Awake()
    {
        // Защита синглтона (не уничтожаемый не нужен для этой сцены, просто базовая проверка)
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(gameObject);
    }

    private void Start()
    {
        SetCurrentLives(_maxLives);
        SetTotalEnemy(_totalEnemy);

        _currentCoins = _startCoins;
        UpdateCoinsUI();

        InstantiateAllTowerUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (IsOver) return;

        // Управление башнями — без изменений
        foreach (Tower tower in _spawnedTowers)
        {
            if (tower == null) continue;
            tower.CheckNearestEnemy(_spawnedEnemies);
            tower.SeekTarget();
            tower.ShootTarget();
        }

        // Логика и движение врагов
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            Enemy enemy = _spawnedEnemies[i];
            if (enemy == null) continue;
            if (!enemy.gameObject.activeSelf) continue;

            // Если враг достиг текущей цели
            if (Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);
                if (enemy.CurrentPathIndex < _enemyPaths.Length)
                {
                    enemy.SetTargetPosition(_enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    // Враг дошёл до базы
                    ReduceLives(1);

                    // Деактивируем и даём спавнеру знать (если нужно)
                    enemy.gameObject.SetActive(false);

                    // оповестим об деактивации (удаление/логирование при желании)
                    OnEnemyDeactivated(enemy);
                }
            }
            else
            {
                enemy.MoveToTarget();
            }
        }

        // Условия победы:
        // Если Spawner сказал, что больше врагов не будет (_allEnemiesSpawned) и активных врагов нет -> win
        if (_allEnemiesSpawned && !ExistsActiveEnemy())
        {
            SetGameOver(true);
        }
    }

    #region Registration / Integration
    /// <summary>
    /// Вызывать из Spawner сразу после активации/инициализации врага.
    /// LevelManager начнёт обновлять и контролировать врага в Update().
    /// </summary>
    public void RegisterSpawnedEnemy(Enemy enemy)
    {
        if (enemy == null) return;

        // защита от дублирования
        if (!_spawnedEnemies.Contains(enemy))
            _spawnedEnemies.Add(enemy);

        // Если у нас задан маршрут, выставляем старт и первую цель
        if (_enemyPaths != null && _enemyPaths.Length >= 2)
        {
            // Ставим врага ровно на старт (переопределяем спавн-позицию spawner'а, если нужно)
            enemy.transform.position = _enemyPaths[0].position;

            // Первый реальный шаг — индекс 1, цель = paths[1]
            enemy.SetCurrentPathIndex(1);
            enemy.SetTargetPosition(_enemyPaths[1].position);

            // Визуальная помощь в редакторе: покажем линию направления на кадр
            Debug.DrawLine(enemy.transform.position, enemy.TargetPosition, Color.red, 2f);
        }
        else
        {
            Debug.LogWarning("LevelManager.RegisterSpawnedEnemy: enemyPaths not configured or too short. Enemy will not move.", this);
        }

        // Обновление счётчика в UI (если нужен)
        _enemyCounter = Mathf.Max(_enemyCounter - 1, 0);
        if (_totalEnemyInfo != null)
            _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";

        Debug.Log($"Registered enemy '{enemy.name}' pos={enemy.transform.position} target={enemy.TargetPosition}", this);
    }


    /// <summary>
    /// Spawner вызывает, когда он завершил все запланированные спавны.
    /// </summary>
    /// 
    public void UnregisterSpawnedTower(Tower tower)
    {
        if (tower == null) return;
        if (_spawnedTowers.Contains(tower))
            _spawnedTowers.Remove(tower);
    }

    public void SetAllEnemiesSpawned()
    {
        _allEnemiesSpawned = true;
        Debug.Log("LevelManager: SetAllEnemiesSpawned called, _allEnemiesSpawned set to true");
        // Проверяем победу сразу после установки флага
        if (!_spawnedEnemies.Exists(e => e != null && e.gameObject.activeSelf))
        {
            Debug.Log("Victory condition met in SetAllEnemiesSpawned, calling SetGameOver(true)");
            SetGameOver(true);
        }
    }

    /// <summary>
    /// Вызывать из Enemy.Die() или где-то при деактивации врага — чтобы мы могли реагировать.
    /// Текущая реализация оставляет объект в списке, но при желании здесь можно удалять из списка.
    /// </summary>
    public void OnEnemyDeactivated(Enemy enemy)
    {
        killedEnemies++;
        Debug.Log($"Enemy killed: {killedEnemies}/{_totalEnemy}");

        if (killedEnemies >= _totalEnemy)
        {
            Debug.Log("All enemies killed! Showing victory panel.");
            SetGameOver(true);
        } 
    }
    #endregion

    #region Towers
    private void InstantiateAllTowerUI()
    {
        if (_towerPrefabs == null || _towerUIParent == null || _towerUIPrefab == null) return;

        foreach (Tower tower in _towerPrefabs)
        {
            GameObject newTowerUIObj = Instantiate(_towerUIPrefab.gameObject, _towerUIParent);
            TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI>();
            if (newTowerUI != null)
            {
                newTowerUI.SetTowerPrefab(tower);
                newTowerUI.transform.name = tower.name;
            }
        }
    }

    public void RegisterSpawnedTower(Tower tower)
    {
        if (!_spawnedTowers.Contains(tower))
            _spawnedTowers.Add(tower);
    }
    #endregion

    #region Bullets / Pools
    public Bullet GetBulletFromPool(Bullet prefab)
    {
        GameObject newBulletObj = _spawnedBullets.Find(b => !b.gameObject.activeSelf && b.name.Contains(prefab.name))?.gameObject;
        if (newBulletObj == null)
            newBulletObj = Instantiate(prefab.gameObject);

        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        if (!_spawnedBullets.Contains(newBullet))
            _spawnedBullets.Add(newBullet);

        return newBullet;
    }
    #endregion

    #region Explosions
    public void ExplodeAt(Vector2 point, float radius, int damage)
    {
        // Создаём копию списка для безопасного перебора
        var enemiesCopy = new List<Enemy>(_spawnedEnemies);
        foreach (Enemy enemy in enemiesCopy)
        {
            if (enemy.gameObject.activeSelf && Vector2.Distance(enemy.transform.position, point) <= radius)
                enemy.ReduceEnemyHealth(damage);
        }
    }
    #endregion

    #region Lives / GameOver
    public void ReduceLives(int value)
    {
        SetCurrentLives(_currentLives - value);
        if (_currentLives <= 0)
            SetGameOver(false);
    }

    public void SetCurrentLives(int currentLives)
    {
        _currentLives = Mathf.Max(currentLives, 0);
        if (_livesInfo != null)
            _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetGameOver(bool isWin)
    {
        IsOver = true;
        Debug.Log($"SetGameOver called. isWin={isWin}, _panel={_panel}, _statusInfo={_statusInfo}");

        if (_statusInfo != null)
            _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        else
            Debug.LogWarning("StatusInfo is not assigned!");

        if (_panel != null)
            _panel.SetActive(true);
        else
            Debug.LogWarning("Panel is not assigned!");

        // Показываем VictoryPanel только при победе
        if (isWin && victoryPanel != null)
            victoryPanel.SetActive(true);
    }
    #endregion

    #region EnemyCounter / UI
    public void SetTotalEnemy(int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        if (_totalEnemyInfo != null)
            _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";
    }
    #endregion

    #region Coins
    public bool SpendCoins(int amount)
    {
        if (_currentCoins >= amount)
        {
            _currentCoins -= amount;
            UpdateCoinsUI();
            return true;
        }
        return false;
    }

    public void AddCoins(int amount)
    {
        _currentCoins += amount;
        UpdateCoinsUI();
    }

    private void UpdateCoinsUI()
    {
        if (_coinsInfo != null)
            _coinsInfo.text = $"Coins: {_currentCoins}";
    }
    #endregion

    #region Utilities / Info
    // Публичные геттеры для внешних систем (DifficultyManager и т.п.)
    public int CoinsSafe => _currentCoins;
    public int GetLivesSafe() => _currentLives;
    public int MaxLives => _maxLives;

    private bool ExistsActiveEnemy()
    {
        int activeCount = 0;
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            var e = _spawnedEnemies[i];
            if (e != null)
            {
                Debug.Log($"Enemy {e.name}: activeSelf={e.gameObject.activeSelf}");
                if (e.gameObject.activeSelf)
                    activeCount++;
            }
        }
        Debug.Log($"ExistsActiveEnemy: activeCount={activeCount}");
        return activeCount > 0;
    }
    #endregion
}
