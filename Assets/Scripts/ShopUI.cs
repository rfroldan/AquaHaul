using TMPro;
using UnityEngine;

// This goes on the canvas object for the Shop UI
public class ShopUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text summaryText;
    public TMP_Text coinsText;
    public UpgradeRow[] rows;

    public void Open(int collectedThisDive)
    {
        panel.SetActive(true);
        summaryText.text = collectedThisDive > 0
            ? "Coins collected this dive: " + collectedThisDive
            : "No coins this time. Better luck next dive!";
        Refresh();
    }

    public void Close() { panel.SetActive(false); }

    public void Refresh()
    {
        if (coinsText) coinsText.text = "Coins: " + GameManager.Instance.Coins;
        foreach (var r in rows) r.Refresh();
    }

    // Hook to the "dive again" button's OnClick
    public void OnDiveButton()
    {
        AudioManager.Play(Sfx.Click);
        GameManager.Instance.CloseShop();
    }

    // Hook to the "main menu" button's OnClick
    public void OnMenuButton()
    {
        AudioManager.Play(Sfx.Click);
        GameManager.Instance.QuitToMenu();
    }
}
