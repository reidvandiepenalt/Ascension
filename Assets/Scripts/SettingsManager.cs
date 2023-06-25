using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider brightnessSlider;

    // Start is called before the first frame update
    void Start()
    {
        LoadSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
        PlayerPrefs.SetFloat("brightness", brightnessSlider.value);
        Resolution curResolution = Screen.currentResolution;
        PlayerPrefs.SetString("resolution", $"{curResolution.width}|{curResolution.height}|{curResolution.refreshRate}");
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        if (PlayerPrefs.HasKey("sfxVolume"))
        {
            sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        }
        if (PlayerPrefs.HasKey("brightness"))
        {
            brightnessSlider.value = PlayerPrefs.GetFloat("brightness") * 4;
        }
        if (PlayerPrefs.HasKey("resolution"))
        {
            string[] parsedResolution = PlayerPrefs.GetString("resolution").Split('|');
            Screen.SetResolution(int.Parse(parsedResolution[0]), int.Parse(parsedResolution[1]), true, int.Parse(parsedResolution[2]));
        }
    }

    private void OnDisable()
    {
        SaveSettings();
    }
}
