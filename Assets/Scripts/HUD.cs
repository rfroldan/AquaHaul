using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Oxygen")]
    public Slider oxygenBar;
    public Image oxygenFill;
    public Color oxygenNormal = new Color(0.2f, 0.8f, 1f);
    public Color oxygenLow = new Color(1f, 0.25f, 0.2f);
    public Image lowOxygenFlash;      // full-screen red Image, transparency 0

    [Header("Texts")]
    public TMP_Text coinsText;
    public TMP_Text livesText;
    public TMP_Text dodgeText;

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        float pct = gm.OxygenPercent;

        if (oxygenBar) oxygenBar.value = pct;
        if (oxygenFill) oxygenFill.color = Color.Lerp(oxygenLow, oxygenNormal, Mathf.InverseLerp(0.2f, 0.35f, pct));

        if (lowOxygenFlash)
        {
            var c = lowOxygenFlash.color;
            c.a = (gm.State == GameState.Diving && pct < 0.25f)
                ? (Mathf.Sin(Time.unscaledTime * 6f) + 1f) * 0.5f * 0.3f : 0f;
            lowOxygenFlash.color = c;
        }

        if (coinsText) coinsText.text = "Coins: " + gm.Coins;
        if (livesText) livesText.text = "Lives: " + gm.LivesLeft + "/" + gm.MaxLives;

        if (dodgeText)
        {
            if (!gm.HasDodge) dodgeText.text = "";
            else dodgeText.text = gm.player.DodgeReady
                ? "Dodge [SPACE]: READY"
                : "Dodge: " + gm.player.DodgeCooldownLeft.ToString("0.0") + "s";
        }
    }
}
