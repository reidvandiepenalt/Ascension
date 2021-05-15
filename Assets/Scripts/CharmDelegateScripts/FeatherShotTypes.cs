﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FeatherTypes
{
    Default,
    Fireball,
    Boomerang,
    BallLightning
}

public class FeatherShotTypes : MonoBehaviour
{
    public PlayerTestScript playerScript;
    public ProgressBarScript shotUIScript;

    private float shotTimer = 0.0f;
    [SerializeField] float shotTimeLength = 0.25f;

    public GameObject shotFeather;
    public GameObject boomerang;
    public Animator anim;


    public void DefaultShot()
    {
        if ((playerScript.state == PlayerTestScript.PlayerState.gliding || playerScript.state == PlayerTestScript.PlayerState.idle
            || playerScript.state == PlayerTestScript.PlayerState.walking))
        {
            anim.SetTrigger("Shoot");
            playerScript.state = PlayerTestScript.PlayerState.shooting;
            shotUIScript.ResetCombo();
            GameObject feather = Instantiate(shotFeather, gameObject.transform.position, Quaternion.identity);
            FeatherScript fs = feather.GetComponent<FeatherScript>();
            fs.GetComponent<AttackInteract>().player = gameObject;
            if (playerScript.facingLeft)
            {
                fs.angle = 180;
            }
        }
        else if (playerScript.state == PlayerTestScript.PlayerState.shooting)
        {
            shotTimer += Time.deltaTime;
            if (shotTimer > shotTimeLength)
            {
                playerScript.state = PlayerTestScript.PlayerState.idle;
                shotTimer = 0.0f;
            }
        }
    }

    public void BoomerangShot()
    {
        if ((playerScript.state == PlayerTestScript.PlayerState.gliding || playerScript.state == PlayerTestScript.PlayerState.idle 
            || playerScript.state == PlayerTestScript.PlayerState.walking))
        {
            anim.SetTrigger("Shoot");
            playerScript.state = PlayerTestScript.PlayerState.shooting;
            shotUIScript.ResetCombo();
            GameObject boomer = Instantiate(boomerang, gameObject.transform.position, Quaternion.identity);
            BoomerangScript bs = boomer.GetComponent<BoomerangScript>();
            bs.GetComponent<AttackInteract>().player = gameObject;
            if (playerScript.facingLeft)
            {
                bs.left = true;
            }
        }
        else if (playerScript.state == PlayerTestScript.PlayerState.shooting)
        {
            shotTimer += Time.deltaTime;
            if (shotTimer > shotTimeLength)
            {
                playerScript.state = PlayerTestScript.PlayerState.idle;
                shotTimer = 0.0f;
            }
        }
    }

    public void Fireball()
    {

    }

    public void BallLightning()
    {

    }
}
