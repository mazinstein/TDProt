using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public Text title;
    public Text description;
    public Text cost;
    public GameObject panel;

    public void Show(BuildingData data)
    {
        title.text = data.buildingName;
        description.text = data.description;
        cost.text = $"Gold: {data.costGold} | Wood: {data.costWood}";
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
