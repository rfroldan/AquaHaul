using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Different characteristics of the shop rows
public class UpgradeRow : MonoBehaviour
{
    public UpgradeType type;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text levelText;
    public TMP_Text costText;
    public Button buyButton;
    public ShopUI shop;

    bool hooked;

    // Connects the buy button to the OnBuy method
    void Hook()
    {
        if (hooked) return;
        hooked = true;
        buyButton.onClick.AddListener(OnBuy);
    }

    // Tries to buy the upgrade and refreshes the shop
    void OnBuy()
    {
        if (!GameManager.Instance.TryBuy(type)) AudioManager.Play(Sfx.Click);
        shop.Refresh();
    }

    // Updates the upgrade's name, description, level, cost, and button
    public void Refresh()
    {
        Hook();
        var gm = GameManager.Instance;
        bool maxed = gm.IsMaxed(type);

        if (nameText) nameText.text = Label(type);
        if (descriptionText) descriptionText.text = Describe(gm, type);

        if (levelText)
            levelText.text = type == UpgradeType.Dodge
                ? (maxed ? "Owned" : "Locked")
                : "Lv " + gm.GetLevel(type) + "/" + gm.GetMaxLevel(type);

        if (costText) costText.text = maxed ? "MAX" : gm.GetCost(type) + " coins";
        buyButton.interactable = !maxed && gm.Coins >= gm.GetCost(type);
    }

    // Gets the name of each upgrade
    static string Label(UpgradeType t)
    {
        switch (t)
        {
            case UpgradeType.Oxygen: return "Oxygen Tank";
            case UpgradeType.Flippers: return "Flippers";
            case UpgradeType.Suit: return "Dive Suit";
            default: return "Dodge Skill";
        }
    }

    // Shows what each upgrade does
    static string Describe(GameManager gm, UpgradeType t)
    {
        switch (t)
        {
            case UpgradeType.Oxygen: return "Air time: " + gm.MaxOxygen.ToString("0") + "s";
            case UpgradeType.Flippers: return "Swim speed: " + gm.SwimSpeed.ToString("0.0");
            case UpgradeType.Suit: return "Lives: " + gm.MaxLives;
            default: return "Press SPACE to dash and dodge";
        }
    }
}
