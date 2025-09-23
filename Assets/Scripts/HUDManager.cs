using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private TextMeshProUGUI velocityText;

    private PlayerController playerController;

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (playerController == null) return; // Simplified null check

        if (fuelSlider != null)
        {
            UpdateFuelGauge();
        }

        if (velocityText != null)
        {
            UpdateVelocityText(); // Call the new method
        }
    }

    private void UpdateFuelGauge()
    {
        // This logic remains correct.
        float fuelPercentage = playerController.CurrentFuel / playerController.MaxFuel;
        fuelSlider.value = fuelPercentage;
    }

    private void UpdateVelocityText()
    {
        // "F1" formats the speed to one decimal place.
        velocityText.text = $"Space Ship Speed: {playerController.Speed:F1}";
    }
}