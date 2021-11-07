using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarScript : MonoBehaviour
{
    public float maximumExpireTime;
    float currentExpireTime;
    public Image mask;
    Image uiImage;
    public int comboToCharge;
    int currentCombo;
    int lastCharge;
    public int charge
    {
        get
        {
            return currentCombo / comboToCharge;
        }
    }

    public float currentFill
    {
        get { return currentExpireTime / maximumExpireTime; }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentExpireTime = maximumExpireTime;
        uiImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (charge == 0)
        {
            mask.fillAmount = 1 - (float)currentCombo / (float)comboToCharge;
        }else if (charge == 1 && lastCharge == 0)
        {
            currentExpireTime = 0f;
            UpdateFill();
        }
        else
        {
            UpdateFill();
        }
        lastCharge = charge;
    }

    /// <summary>
    /// Increase the combo by one
    /// </summary>
    public void IncreaseCombo()
    {
        currentCombo += 1;
    }

    /// <summary>
    /// Resets the combo and skill to 0/default
    /// </summary>
    public void ResetCombo()
    {
        currentCombo = 0;
    }

    /// <summary>
    /// Ran out of time to use skill
    /// </summary>
    public void Expired()
    {
        currentCombo -= charge * comboToCharge;
    }

    /// <summary>
    /// Updates the mask fill
    /// </summary>
    void UpdateFill()
    {
        currentExpireTime += Time.deltaTime;
        if (currentFill < 1.01)
        {
            mask.fillAmount = currentFill;
        }
        else
        {
            Expired();
        }
    }

    public void UpdateImage(Sprite newImage)
    {
        uiImage.sprite = newImage;
    }
}
