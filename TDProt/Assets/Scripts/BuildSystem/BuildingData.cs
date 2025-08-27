using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "RTS/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    [TextArea] public string description;
    public int costGold;
    public int costWood;
    public Sprite icon;
    public GameObject prefab;
}
