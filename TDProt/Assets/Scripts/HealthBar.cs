using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Health health; // Ссылка на компонент здоровья персонажа
    public Image fillImage; // Ссылка на Image с типом Fill

    private void LateUpdate()
    {
        if (health != null && fillImage != null)
        {
            fillImage.fillAmount = health.GetHealthPercent();
        }

        // Повернуть к камере (если нужно)
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }
}