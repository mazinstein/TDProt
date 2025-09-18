using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 1;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private SpriteRenderer _healthBar;
    [SerializeField] private SpriteRenderer _healthFill;
    [SerializeField] private int _coinReward = 1; // награда за убийство

    private int _currentHealth;

    // Для плавного изменения полоски
    private float _targetHealthWidth;
    private float _currentHealthWidth;
    [SerializeField] private float _healthBarLerpSpeed = 8f;

    private SimplePool _ownerPool;

    private Spawner _spawnerRef;
    public Vector3 TargetPosition { get; private set; }
    public int CurrentPathIndex { get; private set; }
    
    private void OnEnable()
    {
        _currentHealth = _maxHealth;
        _targetHealthWidth = _healthBar.size.x;
        _currentHealthWidth = _healthBar.size.x;
        if (_healthBar != null && _healthFill != null)
            _healthFill.size = _healthBar.size;
    }

    private void Update()
    {
        // Плавно изменяем ширину полоски
        _currentHealthWidth = Mathf.Lerp(_currentHealthWidth, _targetHealthWidth, Time.deltaTime * _healthBarLerpSpeed);
        _healthFill.size = new Vector2(_currentHealthWidth, _healthBar.size.y);

        // Смещаем fill так, чтобы левый край был на месте
        // Вместо transform.position используем transform.localPosition
        float leftEdgeLocal = _healthBar.transform.localPosition.x - (_healthBar.size.x / 2f);
        _healthFill.transform.localPosition = new Vector3(
            leftEdgeLocal + (_currentHealthWidth / 2f),
            _healthFill.transform.localPosition.y,
            _healthFill.transform.localPosition.z
        );
    }

    public void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, _moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.identity;
    }

    public void InitFromType(EnemyType type, SimplePool ownerPool, Spawner spawner = null)
    {
        if (type == null) return;
        _ownerPool = ownerPool;
        _spawnerRef = spawner;

        _maxHealth = Mathf.Max(1, type.baseHealth);
        _moveSpeed = Mathf.Max(0.01f, type.baseSpeed);
        _coinReward = type.coinReward;
        _currentHealth = _maxHealth;

        if (_healthBar != null && _healthFill != null)
            _healthFill.size = _healthBar.size;
    }
    public void SetTargetPosition(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
        _healthBar.transform.parent = null;

        // rotation больше не меняется
        // Просто обновляем позицию здоровья

        _healthBar.transform.parent = transform;
    }

    public void SetCurrentPathIndex(int currentIndex)
    {
        CurrentPathIndex = currentIndex;
    }

    public void ReduceEnemyHealth(int damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        // Просто обновляем целевую ширину полоски
        _targetHealthWidth = _healthBar.size.x * (_currentHealth / (float)_maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        gameObject.SetActive(false);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddCoins(_coinReward);
            if (_spawnerRef != null)
                _spawnerRef.NotifyEnemyDead();
            LevelManager.Instance.OnEnemyDeactivated(this);
            Debug.Log("Enemy: Die called, deactivated and notified LevelManager/Spawner");
        }
    }
}
