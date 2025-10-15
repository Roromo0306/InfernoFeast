using UnityEngine;
using UnityEngine.UI;
using TMPro; // si usas TextMeshPro - si no, usa UnityEngine.UI.Text
using System.Collections.Generic;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("References")]
    public SettingsManager settingsManager; // arrastra el objeto con SettingsManager
    public SettingsData settingsData; // arrastra el mismo asset

    [Header("UI Elements")]
    public Slider musicSlider;
    public TMP_Dropdown resolutionDropdown; // puedes usar Dropdown si no usas TMPro
    public Toggle fullscreenToggle;
    public Button applyButton;
    public Button defaultsButton;

    private void Start()
    {
        // llenar UI con valores actuales
        PopulateResolutionOptions();
        InitUIValues();
        AddListeners();
    }

    void PopulateResolutionOptions()
    {
        resolutionDropdown.ClearOptions();
        var options = new List<string>();
        var resList = settingsManager.availableResolutions;

        for (int i = 0; i < resList.Length; i++)
        {
            Resolution r = resList[i];
            string option = $"{r.width} x {r.height} @ {r.refreshRateRatio}Hz";
            options.Add(option);
        }

        resolutionDropdown.AddOptions(options);
    }

    void InitUIValues()
    {
        // Audio slider (0..1)
        musicSlider.value = settingsData.musicVolume;

        // Resolution dropdown
        resolutionDropdown.value = settingsData.resolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Fullscreen toggle
        fullscreenToggle.isOn = settingsData.fullscreen;
    }

    void AddListeners()
    {
        musicSlider.onValueChanged.AddListener((v) =>
        {
            // Aplicar directamente al cambiar
            settingsManager.ApplyMusicVolume(v);
        });

        resolutionDropdown.onValueChanged.AddListener((idx) =>
        {
            // temporal: actualiza índice pero no cambia fullscreen
            settingsManager.ApplyResolutionIndex(idx, fullscreenToggle.isOn);
        });

        fullscreenToggle.onValueChanged.AddListener((isOn) =>
        {
            settingsManager.ApplyResolutionIndex(resolutionDropdown.value, isOn);
        });

        if (applyButton != null) applyButton.onClick.AddListener(() =>
        {
            // Re-aplica todo (opcional)
            settingsManager.ApplyAll();
        });

        if (defaultsButton != null) defaultsButton.onClick.AddListener(() =>
        {
            // Restaurar defaults definidos en SettingsData asset
            settingsManager.RestoreDefaults();
            InitUIValues();
        });
    }

    private void OnDestroy()
    {
        // limpiar listeners para evitar fugas
        musicSlider.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        if (applyButton != null) applyButton.onClick.RemoveAllListeners();
        if (defaultsButton != null) defaultsButton.onClick.RemoveAllListeners();
    }
}
