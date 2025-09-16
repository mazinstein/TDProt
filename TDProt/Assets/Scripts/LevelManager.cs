using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    [Header("Enemies")]
    [SerializeField] private Enemy[] _enemyPrefabs;
    [SerializeField] private Transform[] _enemyPaths;
    [SerializeField] private float _spawnDelay = 5f;

    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private float _runningSpawnDelay;

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

    [Header("Economy")]
    [SerializeField] private int _startCoins = 5; // стартовое количество монет
    [SerializeField] private TextMeshProUGUI _coinsInfo;
    private int _currentCoins;

    private int _currentLives;
    private int _enemyCounter;

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

        // Спавн врагов
        _runningSpawnDelay -= Time.unscaledDeltaTime;
        if (_runningSpawnDelay <= 0f)
        {
            SpawnEnemy();
            _runningSpawnDelay = _spawnDelay;
        }

        // Логика башен
        foreach (Tower tower in _spawnedTowers)
        {
            tower.CheckNearestEnemy(_spawnedEnemies);
            tower.SeekTarget();
            tower.ShootTarget();
        }

        // Логика врагов
        foreach (Enemy enemy in _spawnedEnemies)
        {
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
                }
            }
            else
            {
                enemy.MoveToTarget();
            }
        }
    }

    // Создание UI для всех башен
    private void InstantiateAllTowerUI()
    {
        foreach (Tower tower in _towerPrefabs)
        {
            GameObject newTowerUIObj = Instantiate(_towerUIPrefab.gameObject, _towerUIParent);
            TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI>();
            newTowerUI.SetTowerPrefab(tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    // Регистрируем башню после установки
    public void RegisterSpawnedTower(Tower tower)
    {
        _spawnedTowers.Add(tower);
    }

    // Метод спавна врагов
    private void SpawnEnemy()
    {
        _enemyCounter--;
        if (_enemyCounter < 0)
        {
            bool allDestroyed = _spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;
            if (allDestroyed)
                SetGameOver(true);
            return;
        }

        int randomIndex = Random.Range(0, _enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString();

        GameObject newEnemyObj = _spawnedEnemies.Find(e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString))?.gameObject;
        if (newEnemyObj == null)
            newEnemyObj = Instantiate(_enemyPrefabs[randomIndex].gameObject);

        Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();
        if (!_spawnedEnemies.Contains(newEnemy))
            _spawnedEnemies.Add(newEnemy);

        newEnemy.transform.position = _enemyPaths[0].position;
        newEnemy.SetTargetPosition(_enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex(1);
        newEnemy.gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < _enemyPaths.Length - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_enemyPaths[i].position, _enemyPaths[i + 1].position);
        }
    }

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

    public void ExplodeAt(Vector2 point, float radius, int damage)
    {
        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (enemy.gameObject.activeSelf && Vector2.Distance(enemy.transform.position, point) <= radius)
                enemy.ReduceEnemyHealth(damage);
        }
    }

    #region Lives
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
    #endregion

    #region EnemyCounter
    public void SetTotalEnemy(int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        if (_totalEnemyInfo != null)
            _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";
    }
    #endregion

    #region GameOver
    public void SetGameOver(bool isWin)
    {
        IsOver = true;
        if (_statusInfo != null)
            _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        if (_panel != null)
            _panel.SetActive(true);
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
}
