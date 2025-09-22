using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerPanelUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject panelRoot;

    [Header("Basic texts")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI levelText;

    [Header("Upgrade icons (3)")]
    public Image[] upgradeIconImages = new Image[3];
    public Button[] upgradeIconButtons = new Button[3];
    public TextMeshProUGUI[] upgradeIconCostTexts = new TextMeshProUGUI[3];

    [Header("Sell")]
    public TextMeshProUGUI sellPriceText;
    public Button sellButton;

    private Tower _current;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        // ????????? ??????????? ??? ??????
        for (int i = 0; i < upgradeIconButtons.Length; i++)
        {
            int idx = i;
            if (upgradeIconButtons[idx] != null)
                upgradeIconButtons[idx].onClick.AddListener(() => OnUpgradeIconPressed(idx));
        }
        if (sellButton != null) sellButton.onClick.AddListener(OnSellPressed);
    }

    public void ShowForTower(Tower tower)
    {
        _current = tower;
        if (panelRoot != null) panelRoot.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        _current = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        if (_current == null) { Hide(); return; }

        if (titleText != null) titleText.text = _current.gameObject.name.Replace("(Clone)", "").Trim();
        if (levelText != null) levelText.text = $"Level: {_current.CurrentLevel}/{_current.MaxLevel}";

        var opts = _current.GetUpgradeOptions();
        for (int i = 0; i < upgradeIconImages.Length; i++)
        {
            if (i < opts.Length && opts[i] != null)
            {
                var opt = opts[i];
                if (upgradeIconImages[i] != null) upgradeIconImages[i].sprite = opt.icon;
                if (upgradeIconCostTexts[i] != null) upgradeIconCostTexts[i].text = $"{opt.cost}";
                if (upgradeIconButtons[i] != null)
                {
                    bool used = _current.IsOptionUsed(i);
                    bool can = _current.CanApplyOption(i);
                    upgradeIconButtons[i].interactable = !used && can;
                    // ????????? ????????? ???? ??? ????????????
                    upgradeIconImages[i].color = used ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                }
            }
            else
            {
                // ????????????? ????? — ???????? ??????/??????
                if (upgradeIconImages[i] != null) upgradeIconImages[i].sprite = null;
                if (upgradeIconCostTexts[i] != null) upgradeIconCostTexts[i].text = "-";
                if (upgradeIconButtons[i] != null) upgradeIconButtons[i].interactable = false;
                if (upgradeIconImages[i] != null) upgradeIconImages[i].color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            }
        }

        if (sellPriceText != null) sellPriceText.text = $"Sell: {_current.GetSellPrice()}";
        if (sellButton != null) sellButton.interactable = true;
    }

    private void OnUpgradeIconPressed(int index)
    {
        if (_current == null) return;
        _current.ApplyUpgradeOption(index);

        // ????????? ??????????? (?, ???? ?????, LevelManager/UI)
        Refresh();
    }

    private void OnSellPressed()
    {
        if (_current == null) return;
        _current.Sell();
        Hide();
    }
}