using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuardUIScript : MonoBehaviour
{
    public float maximum;
    public float current;
    public Image mask;

    // Start is called before the first frame update
    void Start()
    {
        current = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //subtract based on time
        current -= Time.deltaTime;
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        //update ui fill
        float fillAmount = current / maximum;
        if (fillAmount >= 0)
        {
            mask.fillAmount = fillAmount;
        }
    }
}
