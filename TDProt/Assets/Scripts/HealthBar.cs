using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Health health;      // Ссылка на компонент здоровья персонажа
    public Transform fill;     // Ссылка на спрайт Fill (зелёная часть)

    private Vector3 initialScale;

    void Start()
    {
        if (fill != null)
            initialScale = fill.localScale; // запоминаем начальный масштаб
    }

    void Update()
    {
        if (health == null || fill == null) return;

        // Обновляем ширину Fill в зависимости от здоровья
        float percent = health.GetHealthPercent();
        fill.localScale = new Vector3(initialScale.x * percent, initialScale.y, initialScale.z);
    }
}
