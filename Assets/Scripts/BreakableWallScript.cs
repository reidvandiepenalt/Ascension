using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWallScript : MonoBehaviour
{
    public GameObject parent;

    public void OnStun(object param)
    {
        float stunTime = (float)param;
    }

    public void OnHit(object param)
    {
        int health = (int)param;
        if(health < 0)
        {
            //need to add animation to attached sprite, probably figure a better way to do this effect later
            parent.SetActive(false);
        }
    }
}
