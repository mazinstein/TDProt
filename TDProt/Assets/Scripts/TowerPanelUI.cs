using UnityEngine;
using UnityEngine.UI;
using TMPro; // ???? ?? ??????????? TMP — ?????? ???? ?? UnityEngine.UI.Text

public class TowerPanelUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject panelRoot; // ?????? ?????? (??????? inactive ?? ?????????)

    [Header("Texts")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellPriceText;

    [Header("Buttons")]
    public Button upgradeButton;
    public Button sellButton;

    private Tower _currentTower;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradePressed);
        if (sellButton != null) sellButton.onClick.AddListener(OnSellPressed);
    }

    public void ShowForTower(Tower tower)
    {
        _currentTower = tower;
        if (panelRoot != null) panelRoot.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        _currentTower = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        if (_currentTower == null)
        {
            Hide();
            return;
        }

        string nm = _currentTower.gameObject.name.Replace("(Clone)", "").Trim();
        if (titleText != null) titleText.text = nm;

        if (levelText != null) levelText.text = $"Level: {_currentTower.CurrentLevel}/{_currentTower.MaxLevel}";

        int upCost = _currentTower.GetUpgradeCost();
        if (upgradeCostText != null) upgradeCostText.text = upCost > 0 ? $"Upgrade: {upCost}" : "Upgrade: —";

        if (sellPriceText != null) sellPriceText.text = $"Sell: {_currentTower.GetSellPrice()}";

        if (upgradeButton != null) upgradeButton.interactable = _currentTower.CanUpgrade();
        if (sellButton != null) sellButton.interactable = true;
    }

    private void OnUpgradePressed()
    {
        if (_currentTower == null) return;
        _currentTower.Upgrade();
        Refresh();
    }

    private void OnSellPressed()
    {
        if (_currentTower == null) return;

        // ????????, ??? LevelManager ?????? ????? ?? ?????? ? UI ?? ????? ????????? ?? ????????? ??????
        _currentTower.Sell();

        // ???????? UI — ????? ?????????? ??? ??????????????
        Hide();
    }
}
