using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
    [SerializeField] private Enemy[] _enemyPrefabs;
    [SerializeField] private Transform[] _enemyPaths;

    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private List<Bullet> _spawnedBullets = new List<Bullet>();

    public bool IsOver { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _totalEnemy = 15;

    [Header("UI Elements")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;

    [Header("Economy")]
    [SerializeField] private int _startCoins = 5;
    [SerializeField] private TextMeshProUGUI _coinsInfo;
    [SerializeField] private Image _coinIcon; // <-- добавьте это поле
    private int _currentCoins;

    private int _currentLives;

    private bool _allEnemiesSpawned = false;
    private int _enemyCounter;
    private int killedEnemies = 0;

    [Header("Victory UI")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Hearts UI")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsParent;
    private List<Image> hearts = new List<Image>();

    private void Awake()
    {
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
        CreateHearts();

        // Запускаем музыку только если она не играет
        if (SoundManager.Instance != null && (SoundManager.Instance.musicSource == null || !SoundManager.Instance.musicSource.isPlaying))
            SoundManager.Instance.PlayBackgroundMusic();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (IsOver) return;

        foreach (Tower tower in _spawnedTowers)
        {
            if (tower == null) continue;
            tower.CheckNearestEnemy(_spawnedEnemies);
            tower.SeekTarget();
            tower.ShootTarget();
        }

        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            Enemy enemy = _spawnedEnemies[i];
            if (enemy == null) continue;
            if (!enemy.gameObject.activeSelf) continue;

            if (Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);
                if (enemy.CurrentPathIndex < _enemyPaths.Length)
                {
                    enemy.SetTargetPosition(_enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    ReduceLives(1);
                    enemy.gameObject.SetActive(false);
                    OnEnemyDeactivated(enemy);
                }
            }
            else
            {
                enemy.MoveToTarget();
            }
        }

        if (_allEnemiesSpawned && !ExistsActiveEnemy())
        {
            SetGameOver(true);
        }
    }

    #region Registration / Integration
    public void RegisterSpawnedEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        if (!_spawnedEnemies.Contains(enemy))
            _spawnedEnemies.Add(enemy);

        if (_enemyPaths != null && _enemyPaths.Length >= 2)
        {
            enemy.transform.position = _enemyPaths[0].position;
            enemy.SetCurrentPathIndex(1);
            enemy.SetTargetPosition(_enemyPaths[1].position);
            Debug.DrawLine(enemy.transform.position, enemy.TargetPosition, Color.red, 2f);
        }
        else
        {
            Debug.LogWarning("LevelManager.RegisterSpawnedEnemy: enemyPaths not configured or too short. Enemy will not move.", this);
        }

        _enemyCounter = Mathf.Max(_enemyCounter - 1, 0);
        if (_totalEnemyInfo != null)
            _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";

        Debug.Log($"Registered enemy '{enemy.name}' pos={enemy.transform.position} target={enemy.TargetPosition}", this);
    }

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
        if (!_spawnedEnemies.Exists(e => e != null && e.gameObject.activeSelf))
        {
            Debug.Log("Victory condition met in SetAllEnemiesSpawned, calling SetGameOver(true)");
            SetGameOver(true);
        }
    }

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

        UpdateHearts();
    }

    public void SetGameOver(bool isWin)
    {
        IsOver = true;

        if (_statusInfo != null)
            _statusInfo.text = isWin ? "You Win!" : "You Lose!";

        if (_panel != null)
            _panel.SetActive(true);

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            if (!isWin)
            {
                // Центрируем и смещаем кнопку чуть ниже центра
                var rt = restartButton.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, -100); // смещение вниз на 100
            }
        }

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(isWin);

        if (!isWin)
            SoundManager.Instance.PlayDefeatSound();
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
        AnimateCoinEffect();
    }

    private void UpdateCoinsUI()
    {
        if (_coinsInfo != null)
            _coinsInfo.text = _currentCoins.ToString();
    }

    private void AnimateCoinEffect()
    {

        if (_coinIcon != null)
        {
            _coinIcon.transform.DOKill();
            _coinIcon.transform.localScale = Vector3.one;
            _coinIcon.transform.DOScale(1.3f, 0.15f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutBack);
        }
    }
    #endregion

    #region Hearts UI
    private void CreateHearts()
    {
        for (int i = 0; i < _maxLives; i++)
        {
            GameObject heartObj = Instantiate(heartPrefab, heartsParent);
            Image heartImg = heartObj.GetComponent<Image>();
            hearts.Add(heartImg);
        }
        UpdateHearts();
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < _currentLives)
            {
                if (!hearts[i].enabled)
                {
                    hearts[i].enabled = true;
                    hearts[i].transform.localScale = Vector3.zero;
                    hearts[i].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            }
            else
            {
                if (hearts[i].enabled)
                {
                    Image img = hearts[i];
                    img.transform.DOShakePosition(0.3f, 5f, 10, 90, false, true);
                    img.DOColor(Color.red, 0.15f).SetLoops(2, LoopType.Yoyo);
                    img.transform
                        .DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => img.enabled = false);
                }
            }
        }
    }
    #endregion

    #region Utilities / Info
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
