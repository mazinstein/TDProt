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

    public Vector3 TargetPosition { get; private set; }
    public int CurrentPathIndex { get; private set; }

    private void OnEnable()
    {
        _currentHealth = _maxHealth;
        _healthFill.size = _healthBar.size;
    }

    public void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, _moveSpeed * Time.deltaTime);
        // Запрещаем любые повороты
        transform.rotation = Quaternion.identity;
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

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }

        float healthPercentage = (float)_currentHealth / _maxHealth;
        _healthFill.size = new Vector2(healthPercentage * _healthBar.size.x, _healthBar.size.y);
    }

    private void Die()
    {
        gameObject.SetActive(false);
        // Добавляем монеты игроку
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddCoins(_coinReward);
        }
    }
}
