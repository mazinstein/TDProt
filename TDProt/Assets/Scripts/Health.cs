using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public System.Action OnDeath; // событие для подписки

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead()) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}