using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int _bulletPower;
    private float _bulletSpeed;
    private float _bulletSplashRadius;

    private Enemy _targetEnemy;

    private void FixedUpdate()
    {
        if (LevelManager.Instance.IsOver || _targetEnemy == null)
            return;

        if (!_targetEnemy.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            _targetEnemy = null;
            return;
        }

        // Направление к цели
        Vector3 direction = _targetEnemy.transform.position - transform.position;
        direction.Normalize();

        // Движение к врагу
        transform.position += direction * _bulletSpeed * Time.fixedDeltaTime;

        // Поворот спрайта так, чтобы он смотрел на цель
        // Если пуля «носом» вверх, используем Atan2
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_targetEnemy == null)
            return;

        if (collision.gameObject.Equals(_targetEnemy.gameObject))
        {
            gameObject.SetActive(false);

            if (_bulletSplashRadius > 0f)
                LevelManager.Instance.ExplodeAt(transform.position, _bulletSplashRadius, _bulletPower);
            else
                _targetEnemy.ReduceEnemyHealth(_bulletPower);

            _targetEnemy = null;
        }
    }

    public void SetProperties(int bulletPower, float bulletSpeed, float bulletSplashRadius)
    {
        _bulletPower = bulletPower;
        _bulletSpeed = bulletSpeed;
        _bulletSplashRadius = bulletSplashRadius;
    }

    public void SetTargetEnemy(Enemy enemy)
    {
        _targetEnemy = enemy;
    }
}
