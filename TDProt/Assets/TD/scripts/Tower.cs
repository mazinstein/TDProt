using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _towerPlace;
    [SerializeField] private SpriteRenderer _towerHead;
    [SerializeField] private int _shootPower = 1;
    [SerializeField] private float _shootDistance = 1f;
    [SerializeField] private float _shootDelay = 5f;
    [SerializeField] private float _bulletSpeed = 1f;
    [SerializeField] private float _bulletSplashRadius = 0f;

    [SerializeField] private Bullet _bulletPrefab;

    private float _runningShootDelay;
    private Enemy _targetEnemy;

    public Vector2? PlacePosition { get; private set; }

    public Sprite GetTowerHeadIcon()
    {
        return _towerHead.sprite;
    }

    public void SetPlacePosition(Vector2? newPosition)
    {
        PlacePosition = newPosition;
    }

    public void LockPlacement()
    {
        transform.position = (Vector2)PlacePosition;
    }

    public void ToggleOrderInLayer(bool toFront)
    {
        if (toFront)
        {
            _towerPlace.sortingOrder = 1;
            _towerHead.sortingOrder = 2;
        }
        else
        {
            _towerPlace.sortingOrder = 0;
            _towerHead.sortingOrder = 1;
        }
    }

    // Проверяем ближайшего врага
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
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > _shootDistance)
            {
                continue;
            }
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }
        _targetEnemy = nearestEnemy;
    }

    // Стрельба по врагу
    public void ShootTarget()
    {
        // Проверяем, есть ли цель
        if (_targetEnemy == null)
        {
            Debug.Log("Нет цели для стрельбы");
            return;
        }

        // Уменьшаем таймер
        _runningShootDelay -= Time.unscaledDeltaTime;

        // Проверяем, можно ли стрелять
        if (_runningShootDelay <= 0f)
        {
            Debug.Log(">>> Tower пытается стрелять!");

            // Проверка префаба
            if (_bulletPrefab == null)
            {
                Debug.LogError("BulletPrefab не назначен в инспекторе!");
                return;
            }

            // Получаем пулю из LevelManager
            Bullet bullet = LevelManager.Instance.GetBulletFromPool(_bulletPrefab);
            if (bullet == null)
            {
                Debug.LogError("Не удалось создать пулю!");
                return;
            }

            // Устанавливаем позицию пули
            bullet.transform.position = transform.position;

            // Настраиваем свойства пули
            bullet.SetProperties(_shootPower, _bulletSpeed, _bulletSplashRadius);
            bullet.SetTargetEnemy(_targetEnemy);

            // Активируем пулю
            bullet.gameObject.SetActive(true);

            Debug.Log(">>> Пуля создана на позиции: " + bullet.transform.position + " с целью: " + _targetEnemy.name);

            // Сбрасываем таймер стрельбы
            _runningShootDelay = _shootDelay;
        }
    }



    // Больше не нужен поворот — метод оставляем пустым
    public void SeekTarget() { }
}
