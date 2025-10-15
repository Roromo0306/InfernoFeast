using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Device;
using Screen = UnityEngine.Screen;


public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance {  get; private set; }

    [Header("Data")]
    public SettingsData settingsData;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public string musicVolumeParam = "MusicVolume";

    [Header("Display")]
    public Resolution[] availableResolutions;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // rellenar resoluciones si no fueron asignadas en inspector
        if (availableResolutions == null || availableResolutions.Length == 0)
            availableResolutions = Screen.resolutions.Distinct().ToArray();

        LoadSettings();
        ApplyAll();
    }
    // Aplicar todas las opciones guardadas a la sesión actual
    public void ApplyAll()
    {
        ApplyMusicVolume(settingsData.musicVolume);
        ApplyResolutionIndex(settingsData.resolutionIndex, settingsData.fullscreen);
    }

    // AUDIO
    public void ApplyMusicVolume(float normalized) // normalized 0..1
    {
        settingsData.musicVolume = Mathf.Clamp01(normalized);
        float dB;
        if (normalized <= 0f) dB = -80f; // silencio
        else dB = 20f * Mathf.Log10(normalized); // conversión a dB
        audioMixer.SetFloat(musicVolumeParam, dB);
        SaveFloat("musicVolume", settingsData.musicVolume);
    }

    // DISPLAY
    public void ApplyResolutionIndex(int index, bool isFullscreen)
    {
        index = Mathf.Clamp(index, 0, availableResolutions.Length - 1);
        settingsData.resolutionIndex = index;
        settingsData.fullscreen = isFullscreen;

        Resolution r = availableResolutions[index];

        // Nuevo manejo del modo de pantalla
        FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        // Nuevo método (sin warning)
        Screen.SetResolution(r.width, r.height, mode, r.refreshRateRatio);

        SaveInt("resolutionIndex", index);
        SaveInt("fullscreen", isFullscreen ? 1 : 0);
    }

    // GUARDADO/CARGADO simple con PlayerPrefs
    void SaveFloat(string key, float v) => PlayerPrefs.SetFloat(key, v);
    void SaveInt(string key, int v) => PlayerPrefs.SetInt(key, v);
    bool HasKey(string key) => PlayerPrefs.HasKey(key);

    public void LoadSettings()
    {
        if (HasKey("musicVolume")) settingsData.musicVolume = PlayerPrefs.GetFloat("musicVolume");
        if (HasKey("resolutionIndex")) settingsData.resolutionIndex = PlayerPrefs.GetInt("resolutionIndex");
        if (HasKey("fullscreen")) settingsData.fullscreen = PlayerPrefs.GetInt("fullscreen") == 1;
    }

    // Restaurar defaults desde el ScriptableObject original 
    public void RestoreDefaults()
    {
       
        ApplyAll();
       
        PlayerPrefs.DeleteKey("musicVolume");
        PlayerPrefs.DeleteKey("resolutionIndex");
        PlayerPrefs.DeleteKey("fullscreen");
    }
}

