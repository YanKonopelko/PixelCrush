using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using YG;

public class SettingsWindow : BaseWindowWithData<SettingsWindowData>
{
    [SerializeField] ToggleButton musicToggle;
    [SerializeField] ToggleButton soundsToggle;
    [SerializeField] ToggleButton effectsToggle;
    [SerializeField] ToggleButton vibrationToggle;

    void Start()
    {
        musicToggle.Init(GlobalData.Instance.MusicManager.MusicEnabled, OnMusicSwitch);
        soundsToggle.Init(GlobalData.Instance.SoundManager.SoundsEnable, OnSoundsSwitch);
        effectsToggle.Init(PlayerData.Instance.EffectsEnabled, OnEffectsSwitch);
        vibrationToggle.Init(PlayerData.Instance.VibrationEnable, OnVibrationSwitch);
    }

    private void OnMusicSwitch(bool value)
    {
        GlobalData.Instance.MusicManager.MusicEnabled = value;
        Debug.Log($"New Music Value: {value}");
    }
    private void OnSoundsSwitch(bool value)
    {
        GlobalData.Instance.SoundManager.SoundsEnable = value;
        Debug.Log($"New Sounds Value: {value}");
    }
    private void OnEffectsSwitch(bool value)
    {
        PlayerData.Instance.EffectsEnabled = value;
        Debug.Log($"New Effects Value: {value}");
    }
    private void OnVibrationSwitch(bool value)
    {
        PlayerData.Instance.VibrationEnable = value;
        Debug.Log($"New Vibration Value: {value}");
    }
    void OnDisable()
    {
        musicToggle.onSwitch -= OnMusicSwitch;
        soundsToggle.onSwitch -= OnSoundsSwitch;
        effectsToggle.onSwitch -= OnEffectsSwitch;
        vibrationToggle.onSwitch -= OnVibrationSwitch;
        YG2.SaveProgress();
    }
}
