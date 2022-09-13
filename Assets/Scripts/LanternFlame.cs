using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LanternFlame : MonoBehaviour
{
    [SerializeField] UnityEngine.Rendering.Universal.Light2D light;
    [SerializeField] float minInnerRadius, maxInnerRadius, minOuterRadius, maxOuterRadius, minIntensity, maxIntensity;
    [SerializeField] Color color1, color2;
    float innerRadiusDif, outerRadiusDif, intensityDif;
    float rDif, bDif, gDif;
    float minR, minB, minG;
    float innerPerlin, outerPerlin, intensityPerlin;
    float rPerlin, bPerlin, gPerlin;

    // Start is called before the first frame update
    void Start()
    {
        innerRadiusDif = maxInnerRadius - minInnerRadius;
        outerRadiusDif = maxOuterRadius - minOuterRadius;
        intensityDif = maxIntensity - minIntensity;
        rDif = Mathf.Abs(color1.r - color2.r);
        gDif = Mathf.Abs(color1.g - color2.g);
        bDif = Mathf.Abs(color1.b - color2.b);
        minR = Mathf.Min(color1.r, color2.r);
        minG = Mathf.Min(color1.g, color2.g);
        minB = Mathf.Min(color1.b, color2.b);
        innerPerlin = Random.Range(0f, 2f);
        outerPerlin = Random.Range(0f, 2f);
        intensityPerlin = Random.Range(0f, 2f);
        rPerlin = Random.Range(0f, 2f);
        bPerlin = Random.Range(0f, 2f);
        gPerlin = Random.Range(0f, 2f);
    }

    void FixedUpdate()
    {
        light.pointLightInnerRadius = minInnerRadius + (Mathf.PerlinNoise(innerPerlin, 0) * innerRadiusDif);
        light.pointLightOuterRadius = minOuterRadius + (Mathf.PerlinNoise(outerPerlin, 0) * outerRadiusDif);
        light.intensity = minIntensity + (Mathf.PerlinNoise(intensityPerlin, 0) * intensityDif);
        light.color = new Color(minR + (Mathf.PerlinNoise(rPerlin, 0) * rDif), minG + (Mathf.PerlinNoise(gPerlin, 0) * gDif), minB + (Mathf.PerlinNoise(bPerlin, 0) * bDif));
        innerPerlin += 0.01f;
        outerPerlin += 0.01f;
        intensityPerlin += 0.01f;
        rPerlin += 0.005f;
        gPerlin += 0.005f;
        bPerlin += 0.005f;
    }
}
