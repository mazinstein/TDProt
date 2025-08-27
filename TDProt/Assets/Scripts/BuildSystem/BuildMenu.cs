using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenu : MonoBehaviour
{
    public BuildingData[] buildings;
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    public Tooltip tooltip;

    void Start()
    {
        foreach (var building in buildings)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            Image icon = btnObj.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = building.icon;

            btn.onClick.AddListener(() => OnSelectBuilding(building));

            // Подсказка при наведении
            EventTriggerListener.Get(btnObj).onEnter = (go) => tooltip.Show(building);
            EventTriggerListener.Get(btnObj).onExit = (go) => tooltip.Hide();
        }
    }

    void OnSelectBuilding(BuildingData building)
    {
        //BuildManager.Instance.SelectBuilding(building.prefab);
    }
}
