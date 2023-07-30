using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class PlayerOptions : MonoBehaviour
{
    protected static PlayerOptions instance;

    [SerializeField] private UIDocument _mainMenu;      //set in inspector
    [Header("Volume")]
    [SerializeField] protected AudioMixer _masterMixer;   //set in inspector
    [SerializeField] protected string MasterVolume = "MasterVolume";
    [SerializeField] protected string MusicVolume = "MusicVolume";
    [SerializeField] protected string SFXVolume = "SFXVolume";

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        else
        {
            instance = this;
        }

        SetupVolume();
    }

    //----------Volume----------
    void SetupVolume()
    {
        //master
        if (!PlayerPrefs.HasKey(MasterVolume))
            PlayerPrefs.SetFloat(MasterVolume, 80.0f);
        _masterMixer.SetFloat(MasterVolume, PlayerPrefs.GetFloat(MasterVolume) - 80.0f);

        //music
        if (!PlayerPrefs.HasKey(MusicVolume))
            PlayerPrefs.SetFloat(MusicVolume, 80.0f);
        _masterMixer.SetFloat(MusicVolume, PlayerPrefs.GetFloat(MusicVolume) - 80.0f);

        //sfx
        if (!PlayerPrefs.HasKey(SFXVolume))
            PlayerPrefs.SetFloat(SFXVolume, 80.0f);
        _masterMixer.SetFloat(SFXVolume, PlayerPrefs.GetFloat(SFXVolume) - 80.0f);
    }

    public static void SetMasterVolume(float newValue)
    {
        instance._masterMixer.SetFloat(instance.MasterVolume, newValue - 80.0f);
    }
    public static float GetMasterVolume()
    {
        float result;
        instance._masterMixer.GetFloat(instance.MasterVolume, out result);
        return result + 80.0f;
    }

    public static void SetMusicVolume(float newValue)
    {
        instance._masterMixer.SetFloat(instance.MusicVolume, newValue - 80.0f);
    }
    public static float GetMusicVolume()
    {
        float result;
        instance._masterMixer.GetFloat(instance.MusicVolume, out result);
        return result + 80.0f;
    }

    public static void SetSFXVolume(float newValue)
    {
        instance._masterMixer.SetFloat(instance.SFXVolume, newValue - 80.0f);
    }
    public static float GetSFXVolume()
    {
        float result;
        instance._masterMixer.GetFloat(instance.SFXVolume, out result);
        return result + 80.0f;
    }

    public static void VolumeResetToDefault()
    {
        //master
        instance._masterMixer.SetFloat(instance.MasterVolume, 0.0f);

        //music
        instance._masterMixer.SetFloat(instance.MusicVolume, 0.0f);

        //sfx
        instance._masterMixer.SetFloat(instance.SFXVolume, 0.0f);
    }

    public static void SaveVolumeSettings()
    {
        float value;
        instance._masterMixer.GetFloat(instance.MasterVolume, out value);
        PlayerPrefs.SetFloat(instance.MasterVolume, value);

        instance._masterMixer.GetFloat(instance.MusicVolume, out value);
        PlayerPrefs.SetFloat(instance.MusicVolume, value);

        instance._masterMixer.GetFloat(instance.SFXVolume, out value);
        PlayerPrefs.SetFloat(instance.SFXVolume, value);
    }
    //----------End Volume----------

    private void OnDisable()
    {
        SaveVolumeSettings();

        Debug.Log("Saved audio settings");
    }
}
