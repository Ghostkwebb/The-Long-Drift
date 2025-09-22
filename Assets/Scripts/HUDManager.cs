using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider fuelSlider;

    private PlayerController playerController;

    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (playerController != null && fuelSlider != null)
        {
            UpdateFuelGauge();
        }
    }

    private void UpdateFuelGauge()
    {
        // This logic remains correct.
        float fuelPercentage = playerController.CurrentFuel / playerController.MaxFuel;
        fuelSlider.value = fuelPercentage;
    }
}