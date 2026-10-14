using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum UpgradeType { Oxygen = 0, Flippers = 1, Suit = 2, Dodge = 3 }
public enum GameState { Diving, Shop, Dead }

// Central brain: oxygen, lives (suit hits), coins, upgrades, shop-at-surface logic, saving.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public PlayerController player;
    public ShopUI shop;
    public CurrentSpawner spawner;
    public ParticleSystem splashFX;    
    public GameObject deathPanel;
    public TMP_Text deathText;

    [Header("Water Layout")]
    public float surfaceY = 3f;          // Y of the water line (near the top of the screen)
    public float spawnX = -5f;
    public float spawnDepth = 1.5f;      // player spawns this far BELOW the surface
    public float shopTriggerDepth = 0.6f;// shop opens when player is within this distance of the surface
    public float rearmDepth = 3f;        // must swim this far down before the shop can open again

    [Header("Balance")]
    public int bonusLives = 0;           // extra lives on top of Suit level (set 1 for a gentler start)
    public bool loseCoinsOnDeath = false;// if true, coins picked up during a failed dive are taken back

    // Runtime state
    public GameState State { get; private set; } = GameState.Diving;
    public float Oxygen { get; private set; }
    public int LivesLeft { get; private set; }
    public int Coins { get; private set; }
    public int DiveCoins { get; private set; }

    public const int MaxLevel = 6;
    static readonly int[] baseCost = { 25, 25, 40, 100 };
    readonly int[] levels = new int[4];
    float warnTimer;
    bool shopArmed;

    // Derived stats
    public int GetLevel(UpgradeType t) => levels[(int)t];
    public int GetMaxLevel(UpgradeType t) => t == UpgradeType.Dodge ? 1 : MaxLevel;
    public bool IsMaxed(UpgradeType t) => GetLevel(t) >= GetMaxLevel(t);
    public int GetCost(UpgradeType t)
    {
        if (t == UpgradeType.Dodge) return baseCost[3];
        return Mathf.RoundToInt(baseCost[(int)t] * Mathf.Pow(1.6f, GetLevel(t) - 1));
    }
    public float MaxOxygen => 30f + 12f * (GetLevel(UpgradeType.Oxygen) - 1);
    public float SwimSpeed => 3.5f + 0.6f * (GetLevel(UpgradeType.Flippers) - 1);
    public int MaxLives => GetLevel(UpgradeType.Suit) + bonusLives;
    public bool HasDodge => GetLevel(UpgradeType.Dodge) > 0;
    public float OxygenPercent => MaxOxygen <= 0 ? 0 : Mathf.Clamp01(Oxygen / MaxOxygen);

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        LoadSave();
    }

    void Start()
    {
        if (deathPanel) deathPanel.SetActive(false);
        if (shop) shop.Close();
        StartDive();
    }

    void OnDestroy() { if (Instance == this) Instance = null; Time.timeScale = 1f; }

    void Update()
    {
        if (State != GameState.Diving) return;

        Oxygen -= Time.deltaTime;

        if (OxygenPercent < 0.25f)
        {
            warnTimer -= Time.deltaTime;
            if (warnTimer <= 0f) { AudioManager.Play(Sfx.Warning); warnTimer = 1.5f; }
        }

        if (Oxygen <= 0f) { Die("You ran out of oxygen!", 0.3f, false); return; }

        // Shop opens when the player reaches the surface
        float y = player.transform.position.y;
        if (!shopArmed && surfaceY - y > rearmDepth) shopArmed = true;
        if (shopArmed && y >= surfaceY - shopTriggerDepth) ReachSurface();
    }

    // Dive flow
    public void StartDive()
    {
        Time.timeScale = 1f;
        State = GameState.Diving;
        if (deathPanel) deathPanel.SetActive(false);

        DiveCoins = 0;
        Oxygen = MaxOxygen;
        LivesLeft = MaxLives;
        shopArmed = false;
        warnTimer = 0f;

        player.Respawn(new Vector2(spawnX, surfaceY - spawnDepth));
        if (spawner) spawner.ResetSpawner();

        if (splashFX)
        {
            splashFX.transform.position = new Vector3(spawnX, surfaceY, 0f);
            splashFX.Play();
        }
        AudioManager.Play(Sfx.Splash);
    }

    void ReachSurface()
    {
        State = GameState.Shop;
        int collected = DiveCoins;
        DiveCoins = 0;
        Save();

        player.SetFrozen(true);
        Time.timeScale = 0f;
        shop.Open(collected);
    }

    public void CloseShop()
    {
        shop.Close();
        StartDive();
    }

    void Die(string reason, float delay, bool hidePlayer)
    {
        if (State == GameState.Dead) return;
        State = GameState.Dead;
        player.SetFrozen(true);
        if (hidePlayer) player.SetVisible(false);

        string extra = "";
        if (loseCoinsOnDeath && DiveCoins > 0)
        {
            Coins = Mathf.Max(0, Coins - DiveCoins);
            extra = "\nYou lost " + DiveCoins + " coins.";
        }
        DiveCoins = 0;
        Save();

        StartCoroutine(ShowDeathPanel(reason + extra, delay));
    }

    IEnumerator ShowDeathPanel(string text, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 0f;
        if (deathText) deathText.text = text;
        if (deathPanel) deathPanel.SetActive(true);
    }

    public void Retry() { StartDive(); }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Gameplay API
    public void DamagePlayer(Vector2 sourcePos)
    {
        if (State != GameState.Diving || player.IsInvulnerable) return;
        LivesLeft--;                         // each hit removes one life
        AudioManager.Play(Sfx.Hurt);
        player.Hurt(sourcePos);
        if (LivesLeft <= 0) Die("You ran out of lives!", 0.3f, false);
    }

    // Instant death (bombs), ignores lives
    public void KillPlayer(string reason, float delay = 1f, bool hidePlayer = true)
    {
        if (State != GameState.Diving) return;
        LivesLeft = 0;
        Die(reason, delay, hidePlayer);
    }

    public bool CollectCoin(int value)
    {
        if (State != GameState.Diving) return false;
        Coins += value;
        DiveCoins += value;
        AudioManager.Play(Sfx.Pickup);
        return true;
    }

    public bool TryBuy(UpgradeType t)
    {
        if (IsMaxed(t)) return false;
        int cost = GetCost(t);
        if (Coins < cost) return false;
        Coins -= cost;
        levels[(int)t]++;
        Oxygen = MaxOxygen;
        LivesLeft = MaxLives;
        Save();
        AudioManager.Play(Sfx.Upgrade);
        return true;
    }

    // Reset stats in inspector
    public void ResetUpgradesToDefault()
    {
        levels[(int)UpgradeType.Oxygen] = 1;
        levels[(int)UpgradeType.Flippers] = 1;
        levels[(int)UpgradeType.Suit] = 1;
        levels[(int)UpgradeType.Dodge] = 0;

        Oxygen = MaxOxygen;
        LivesLeft = MaxLives;

        Save();

        Debug.Log("Upgrades reset to default levels!");
    }

    // Saving
    void LoadSave()
    {
        Coins = PlayerPrefs.GetInt("aq_coins", 0);
        for (int i = 0; i < levels.Length; i++)
            levels[i] = PlayerPrefs.GetInt("aq_lvl_" + i, i == (int)UpgradeType.Dodge ? 0 : 1);
    }

    void Save()
    {
        PlayerPrefs.SetInt("aq_coins", Coins);
        for (int i = 0; i < levels.Length; i++) PlayerPrefs.SetInt("aq_lvl_" + i, levels[i]);
        PlayerPrefs.Save();
    }

    public static void ResetSave()
    {
        PlayerPrefs.DeleteKey("aq_coins");
        for (int i = 0; i < 4; i++) PlayerPrefs.DeleteKey("aq_lvl_" + i);
        PlayerPrefs.Save();
    }

    [ContextMenu("DEBUG: Add 500 Coins")]
    void DebugAddCoins() { Coins += 500; Save(); }
}
