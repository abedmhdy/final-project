using UnityEngine;
using UnityEngine.UI;

// The in-game HUD. It only listens to events (health, waves, weapon,
// pickups) and never tells the game what to do.
// Top-left: health, current weapon, active boosts. Top-right: level and
// wave. Top-centre: short messages that fade out ("Wave 2/3",
// "Room Cleared!", "Equipped Axe!").
public class HUDController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Text weaponText;
    [SerializeField] private Text boostText;
    [SerializeField] private Text messageText;
    [SerializeField] private float messageDuration = 2f;

    private float messageTimer;
    private float speedBoostEndTime;
    private float damageBoostEndTime;

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthBar;
        WaveSpawner.OnWaveChanged += UpdateWaveText;
        WaveSpawner.OnAllWavesCleared += HandleAllWavesCleared;
        WaveSpawner.OnRewardSpawned += HandleRewardSpawned;
        PlayerCombat.OnWeaponChanged += UpdateWeapon;
        Pickup.OnCollected += HandlePickupCollected;
        Pickup.OnRejected += HandlePickupRejected;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealthBar;
        WaveSpawner.OnWaveChanged -= UpdateWaveText;
        WaveSpawner.OnAllWavesCleared -= HandleAllWavesCleared;
        WaveSpawner.OnRewardSpawned -= HandleRewardSpawned;
        PlayerCombat.OnWeaponChanged -= UpdateWeapon;
        Pickup.OnCollected -= HandlePickupCollected;
        Pickup.OnRejected -= HandlePickupRejected;
    }

    private void Start()
    {
        if (boostText != null) boostText.text = "";
        if (messageText != null) messageText.text = "";

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.CurrentLevelNumber > 0)
        {
            string level = $"Level {gameManager.CurrentLevelNumber}/{gameManager.LevelCount}";
            if (levelText != null) levelText.text = level;
            ShowMessage(level);
        }
    }

    private void Update()
    {
        UpdateMessageFade();
        UpdateBoostText();
    }

    private void UpdateHealthBar(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null) healthText.text = $"{current}/{max}";
    }

    private void UpdateWaveText(int current, int total)
    {
        if (waveText != null) waveText.text = $"Wave {current}/{total}";
        ShowMessage(current == total && total > 1 ? $"Final Wave {current}/{total}!" : $"Wave {current}/{total}");
    }

    private void HandleAllWavesCleared()
    {
        if (waveText != null) waveText.text = "Room Cleared";
        ShowMessage("Room Cleared! The EXIT is open");
    }

    private void HandleRewardSpawned(GameObject reward)
    {
        WeaponPickup weaponPickup = reward.GetComponent<WeaponPickup>();
        ShowMessage(weaponPickup != null
            ? $"New weapon: {weaponPickup.Weapon.weaponName}! Grab it"
            : "A reward appeared!");
    }

    private void UpdateWeapon(WeaponData weapon)
    {
        if (weaponText != null) weaponText.text = $"{weapon.weaponName}  (DMG {weapon.damage})";
        if (weaponIcon != null)
        {
            weaponIcon.sprite = weapon.icon;
            weaponIcon.enabled = weapon.icon != null;
        }
    }

    private void HandlePickupCollected(Pickup pickup)
    {
        ShowMessage(pickup.CollectMessage);

        // Remember when timed boosts run out, to show a countdown.
        if (pickup is PowerUpPickup powerUp)
        {
            PowerUpData data = powerUp.Data;
            if (data.type == PowerUpType.SpeedBoost) speedBoostEndTime = Time.time + data.duration;
            if (data.type == PowerUpType.DamageBoost) damageBoostEndTime = Time.time + data.duration;
        }
    }

    private void HandlePickupRejected(Pickup pickup)
    {
        if (pickup.RejectMessage != null) ShowMessage(pickup.RejectMessage);
    }

    private void UpdateBoostText()
    {
        if (boostText == null) return;

        string boosts = "";
        if (Time.time < speedBoostEndTime) boosts += $"Speed Boost {Mathf.CeilToInt(speedBoostEndTime - Time.time)}s   ";
        if (Time.time < damageBoostEndTime) boosts += $"Damage Boost {Mathf.CeilToInt(damageBoostEndTime - Time.time)}s";
        boostText.text = boosts.TrimEnd();
    }

    private void ShowMessage(string message)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageTimer = messageDuration;
    }

    // Uses scaled time, so a message freezes while the game is paused.
    private void UpdateMessageFade()
    {
        if (messageText == null || messageTimer <= 0f) return;

        messageTimer -= Time.deltaTime;
        Color color = messageText.color;
        color.a = Mathf.Clamp01(messageTimer / 0.5f);
        messageText.color = color;

        if (messageTimer <= 0f) messageText.text = "";
    }
}
