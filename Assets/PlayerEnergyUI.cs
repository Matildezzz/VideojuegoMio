using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerEnergyUI : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private PlayerEnergy playerEnergy;

    [Header("UI")]
    [SerializeField] private Slider energySlider;
    [SerializeField] private TMP_Text energyText;

    [Header("Texto")]
    [SerializeField] private bool showNumbers = true;

    private void Awake()
    {
        if (playerEnergy == null)
        {
            playerEnergy = FindFirstObjectByType<PlayerEnergy>();
        }

        if (energySlider == null)
        {
            energySlider = GetComponent<Slider>();
        }
    }

    private void OnEnable()
    {
        if (playerEnergy != null)
        {
            playerEnergy.OnEnergyChanged += HandleEnergyChanged;
        }

        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (playerEnergy != null)
        {
            playerEnergy.OnEnergyChanged -= HandleEnergyChanged;
        }
    }

    private void HandleEnergyChanged(int currentEnergy, int maxEnergy)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (playerEnergy == null)
        {
            return;
        }

        if (energySlider != null)
        {
            energySlider.minValue = 0;
            energySlider.maxValue = playerEnergy.MaxEnergy;
            energySlider.value = playerEnergy.CurrentEnergy;
        }

        if (energyText != null)
        {
            if (showNumbers)
            {
                energyText.text = playerEnergy.CurrentEnergy + " / " + playerEnergy.MaxEnergy;
            }
            else
            {
                energyText.text = string.Empty;
            }
        }
    }
}