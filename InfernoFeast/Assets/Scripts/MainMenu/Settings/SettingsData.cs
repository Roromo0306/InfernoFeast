
using UnityEngine;
[CreateAssetMenu(fileName = "SettingsData", menuName = "Settings Data")]
public class SettingsData : ScriptableObject
{
    [Header("Audio")]
    [Range(0f, 1f)] public float musicVolume = 0.8f;

    [Header("Display")]
    public int resolutionIndex = 0;
    public bool fullscreen = true;
}
