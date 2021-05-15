using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarScript : MonoBehaviour
{
    public GameObject bottomHealth, topHealth;
    List<GameObject> hp;
    public IntValue PlayerMaxHealth, PlayerCurrentHealth;

    // Start is called before the first frame update
    void Start()
    {
        hp = new List<GameObject>();
        InitHealth();
    }

    /// <summary>
    /// Adjusts the health bar based on changes to playerHealth intvalue
    /// </summary>
    public void AdjustHP() 
    {
        if(PlayerCurrentHealth.Value == 0)
        {
            GameObject.Destroy(hp[0]);
            hp.Clear();
            return;
        }
        int diff = PlayerCurrentHealth.Value - hp.Count;
        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                if (i % 2 == 0)
                {
                    hp.Add(Instantiate(topHealth, transform));
                }
                else
                {
                    hp.Add(Instantiate(bottomHealth, transform));
                }
            }
        }
        else
        {
            for (int i = 0; i < -diff; i++)
            {
                GameObject.Destroy(hp[hp.Count - 1]);
                hp.RemoveAt(hp.Count - 1);
            }
        }
    }

    /// <summary>
    /// Initializes or resets the healthbar
    /// </summary>
    public void InitHealth()
    {
        hp.Clear();
        for (int i = 0; i < PlayerMaxHealth.Value; i++)
        {
            if (i % 2 == 0)
            {
                hp.Add(Instantiate(topHealth, transform));
            }
            else
            {
                hp.Add(Instantiate(bottomHealth, transform));
            }
        }
    }
}
