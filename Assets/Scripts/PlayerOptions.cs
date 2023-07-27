using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class PlayerOptions : MonoBehaviour
{
    [SerializeField] private UIDocument _mainMenu;      //set in inspector
    [Header("Volume")]
    [SerializeField] private AudioMixer _masterMixer;   //set in inspector
    [SerializeField] private string MasterVolume = "MasterVolume";
    [SerializeField] private string MusicVolume = "MusicVolume";
    [SerializeField] private string SFXVolume = "SFXVolume";

    //volume
    private Button _resetToDefaultButton;
    private Slider _masterVolumeSlider;
    private Slider _musicVolumeSlider;
    private Slider _sfxVolumeSlider;

    private void Awake()
    {
        SetupVolume();
    }

    //----------Volume----------
    void SetupVolume()
    {
        VisualElement volumeSubGroup = _mainMenu.rootVisualElement.Q<VisualElement>("volume-options");

        //reset
        _resetToDefaultButton = volumeSubGroup.Q<Button>("reset-to-default-button");
        _resetToDefaultButton.clicked += VolumeResetToDefault;

        //master
        if (!PlayerPrefs.HasKey(MasterVolume))
            PlayerPrefs.SetFloat(MasterVolume, 80.0f);
        _masterMixer.SetFloat(MasterVolume, PlayerPrefs.GetFloat(MasterVolume) - 80.0f);

        _masterVolumeSlider = volumeSubGroup.Q<Slider>("master-volume-slider");
        _masterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MasterVolume));
        _masterVolumeSlider.RegisterValueChangedCallback(MasterVolumeChanged);

        //music
        if (!PlayerPrefs.HasKey(MusicVolume))
            PlayerPrefs.SetFloat(MusicVolume, 80.0f);
        _masterMixer.SetFloat(MusicVolume, PlayerPrefs.GetFloat(MusicVolume) - 80.0f);

        _musicVolumeSlider = volumeSubGroup.Q<Slider>("music-volume-slider");
        _musicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicVolume));
        _musicVolumeSlider.RegisterValueChangedCallback(MusicVolumeChanged);

        //sfx
        if (!PlayerPrefs.HasKey(SFXVolume))
            PlayerPrefs.SetFloat(SFXVolume, 80.0f);
        _masterMixer.SetFloat(SFXVolume, PlayerPrefs.GetFloat(SFXVolume) - 80.0f);

        _sfxVolumeSlider = volumeSubGroup.Q<Slider>("sfx-volume-slider");
        _sfxVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SFXVolume));
        _sfxVolumeSlider.RegisterValueChangedCallback(SFXVolumeChanged);
    }

    void MasterVolumeChanged(ChangeEvent<float> evt)
    {
        _masterMixer.SetFloat(MasterVolume, evt.newValue - 80.0f);
    }

    void MusicVolumeChanged(ChangeEvent<float> evt)
    {
        _masterMixer.SetFloat(MusicVolume, evt.newValue - 80.0f);
    }

    void SFXVolumeChanged(ChangeEvent<float> evt)
    {
        _masterMixer.SetFloat(SFXVolume, evt.newValue - 80.0f);
    }

    void VolumeResetToDefault()
    {
        //master
        PlayerPrefs.SetFloat(MasterVolume, 80.0f);
        _masterMixer.SetFloat(MasterVolume, PlayerPrefs.GetFloat(MasterVolume) - 80.0f);
        _masterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MasterVolume));

        //music
        PlayerPrefs.SetFloat(MusicVolume, 80.0f);
        _masterMixer.SetFloat(MusicVolume, PlayerPrefs.GetFloat(MusicVolume) - 80.0f);
        _musicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicVolume));

        //sfx
        PlayerPrefs.SetFloat(SFXVolume, 80.0f);
        _masterMixer.SetFloat(SFXVolume, PlayerPrefs.GetFloat(SFXVolume) - 80.0f);
        _sfxVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SFXVolume));
    }
    //----------End Volume----------

    private void OnDisable()
    {
        //volume
        //master
        PlayerPrefs.SetFloat(MasterVolume, _masterVolumeSlider.value);
        _masterVolumeSlider.UnregisterValueChangedCallback(MasterVolumeChanged);

        //music
        PlayerPrefs.SetFloat(MusicVolume, _musicVolumeSlider.value);
        _musicVolumeSlider.UnregisterValueChangedCallback(MusicVolumeChanged);

        //sfx
        PlayerPrefs.SetFloat(SFXVolume, _sfxVolumeSlider.value);
        _sfxVolumeSlider.UnregisterValueChangedCallback(SFXVolumeChanged);

        //reset button
        _resetToDefaultButton.clicked -= VolumeResetToDefault;

        Debug.Log("Saved audio settings");
        //end volume
    }
}
