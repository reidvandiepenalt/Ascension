using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HighlightOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public TMP_Text text;

    public void Start()
    {
        text.fontMaterial.enableInstancing = true;
    }

    public void OnSelect(BaseEventData eventData)
    {
        EnableGlow();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DisableGlow();
    }

    void EnableGlow()
    {
        text.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0.4f);
    }

    void DisableGlow()
    {
        text.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0);
    }
}
