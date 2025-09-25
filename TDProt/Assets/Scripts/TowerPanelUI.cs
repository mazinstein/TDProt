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

    [Header("Classic upgrade (single button)")]
    public GameObject classicUpgradeContainer;
    public Button classicUpgradeButton;
    public Image classicUpgradeImage;
    public TextMeshProUGUI classicUpgradeCostText;

    private Tower _current;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        if (classicUpgradeButton != null) classicUpgradeButton.onClick.AddListener(OnClassicUpgradePressed);

        for (int i = 0; i < upgradeIconButtons.Length; i++)
        {
            int idx = i;
            if (upgradeIconButtons[idx] != null)
                upgradeIconButtons[idx].onClick.AddListener(() => OnUpgradeIconPressed(idx));
        }

        if (sellButton != null) sellButton.onClick.AddListener(OnSellPressed);
    }

    // Debug helper — ?????? ? Inspector, ????? ??????? ??????? ?????
    [ContextMenu("LogLinkedImages")]
    public void LogLinkedImages()
    {
        Debug.Log($"--- TowerPanelUI linked images (panelRoot={panelRoot?.name}) ---", this);
        for (int i = 0; i < upgradeIconImages.Length; i++)
        {
            var img = upgradeIconImages[i];
            Debug.Log($"upgradeIconImages[{i}] -> {(img != null ? img.gameObject.name : "NULL")}", img);
        }
        Debug.Log($"classicUpgradeImage -> {(classicUpgradeImage != null ? classicUpgradeImage.gameObject.name : "NULL")}", classicUpgradeImage);
    }

    public void ShowForTower(Tower tower)
    {
        _current = tower;
        if (panelRoot != null) panelRoot.SetActive(true);

        Debug.Log($"ShowForTower: tower='{_current?.name}' (instance id: {_current?.GetInstanceID().ToString()} )", this);

        // ???????, ????? Image ??????? ????????? (??????? ???????????)
        LogLinkedImages();

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
        if (_current.gameObject == null) { Hide(); return; }

        if (titleText != null) titleText.text = _current.gameObject.name.Replace("(Clone)", "").Trim();
        if (levelText != null) levelText.text = $"Level: {_current.CurrentLevel}/{_current.MaxLevel}";

        // --- classic upgrade ---
        if (classicUpgradeContainer != null)
        {
            if (_current.CurrentLevel >= _current.MaxLevel)
            {
                classicUpgradeContainer.SetActive(false);
            }
            else
            {
                classicUpgradeContainer.SetActive(true);
                Sprite icon = _current.GetClassicUpgradeIcon();
                if (classicUpgradeImage != null)
                {
                    // ???????? ????? ??????????
                    if (icon == null)
                    {
                        classicUpgradeImage.sprite = null;
                        classicUpgradeImage.color = new Color(1, 1, 1, 0f);
                    }
                    else
                    {
                        classicUpgradeImage.sprite = icon;
                        classicUpgradeImage.color = Color.white;
                        classicUpgradeImage.SetNativeSize();
                    }
                }

                int cost = _current.GetUpgradeCost();
                if (classicUpgradeCostText != null) classicUpgradeCostText.text = cost > 0 ? $"{cost}" : "-";

                if (classicUpgradeButton != null)
                    classicUpgradeButton.interactable = _current.HasClassicUpgradeAvailable();
            }
        }

        // --- options ---
        var opts = _current.GetUpgradeOptions();
        for (int i = 0; i < upgradeIconImages.Length; i++)
        {
            Image img = upgradeIconImages[i];
            TextMeshProUGUI costText = (i < upgradeIconCostTexts.Length) ? upgradeIconCostTexts[i] : null;
            Button btn = (i < upgradeIconButtons.Length) ? upgradeIconButtons[i] : null;

            if (opts != null && i < opts.Length && opts[i] != null)
            {
                var opt = opts[i];
                // assign sprite
                if (img != null)
                {
                    img.sprite = opt.icon;
                    img.color = Color.white;
                    img.SetNativeSize();
                }

                if (costText != null) costText.text = $"{opt.cost}";

                if (btn != null)
                {
                    bool used = _current.IsOptionUsed(i);
                    bool can = _current.CanApplyOption(i);
                    btn.interactable = !used && can;
                }
            }
            else
            {
                if (img != null)
                {
                    img.sprite = null;
                    img.color = new Color(1, 1, 1, 0f);
                }
                if (costText != null) costText.text = "-";
                if (btn != null) btn.interactable = false;
            }
        }

        // sell button: ?????????? ?????? ???? GetSellPrice() > 0
        if (sellPriceText != null) sellPriceText.text = $"Sell: {_current.GetSellPrice()}";
        if (sellButton != null) sellButton.interactable = (_current.GetSellPrice() > 0);

        Debug.Log($"Refresh panel for '{_current.name}'. SellPrice={_current.GetSellPrice()}. Options count={(opts != null ? opts.Length : 0)}", this);
    }

    private void OnUpgradeIconPressed(int index)
    {
        if (_current == null) return;

        Debug.Log($"OnUpgradeIconPressed: index={index} for tower {_current.name}", this);

        Tower returned = _current.ApplyUpgradeOption(index);

        if (returned == null)
        {
            Debug.LogWarning("OnUpgradeIconPressed: upgrade replaced tower but new instance is null. Hiding panel.");
            Hide();
            return;
        }

        if (returned != _current)
        {
            Debug.Log($"Upgrade replaced tower instance. Updating panel reference to new tower: {returned.name}", this);
            _current = returned;
        }

        Refresh();
    }

    private void OnSellPressed()
    {
        if (_current == null) return;
        Debug.Log($"OnSellPressed: selling tower {_current.name}. SellPrice={_current.GetSellPrice()}", this);
        _current.Sell();
        Hide();
    }

    private void OnClassicUpgradePressed()
    {
        if (_current == null) return;

        Debug.Log($"OnClassicUpgradePressed for {_current.name}", this);
        _current.Upgrade();
        Refresh();
    }
}
