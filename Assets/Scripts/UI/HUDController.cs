using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text waveText;

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthBar;
        WaveSpawner.OnWaveChanged += UpdateWaveText;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealthBar;
        WaveSpawner.OnWaveChanged -= UpdateWaveText;
    }

    private void UpdateHealthBar(int current, int max)
    {
        if (healthSlider == null) return;
        healthSlider.maxValue = max;
        healthSlider.value = current;
    }

    private void UpdateWaveText(int current, int total)
    {
        if (waveText == null) return;
        waveText.text = $"Wave {current}/{total}";
    }
}
