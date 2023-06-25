using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BrightnessSlider : MonoBehaviour
{
    [SerializeField] Slider brightnessSlider;

    [SerializeField] VolumeProfile brightness;
    ColorAdjustments colorAdjustments;


    void Start()
    {
        brightness.TryGet(out colorAdjustments);
        AdjustBrightness(brightnessSlider.value);
    }

    public void AdjustBrightness(float value)
    {
        if(colorAdjustments != null) colorAdjustments.postExposure.value = value / 4f;
    }
}
