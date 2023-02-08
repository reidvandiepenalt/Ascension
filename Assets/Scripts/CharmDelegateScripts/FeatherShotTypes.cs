using System.Collections;
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

    [SerializeField] GameObject shotFeather;
    [SerializeField] GameObject boomerang;
    [SerializeField] GameObject fireball;
    [SerializeField] Animator anim;


    public void DefaultShot()
    {
        if ((playerScript.state == PlayerTestScript.PlayerState.gliding || playerScript.state == PlayerTestScript.PlayerState.idle
            || playerScript.state == PlayerTestScript.PlayerState.walking) && (shotUIScript.charge >= 1 || playerScript.debugSkills))
        {
            playerScript.playerSFXManager.PlayFeatherShot();
            anim.SetTrigger("Shoot");
            playerScript.playerSFXManager.PlayFeatherShot();
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
            || playerScript.state == PlayerTestScript.PlayerState.walking) && (shotUIScript.charge >= 1 || playerScript.debugSkills))
        {
            playerScript.playerSFXManager.PlayFeatherShot();
            anim.SetTrigger("Shoot");
            playerScript.playerSFXManager.PlayFeatherShot();
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
        if ((playerScript.state == PlayerTestScript.PlayerState.gliding || playerScript.state == PlayerTestScript.PlayerState.idle
            || playerScript.state == PlayerTestScript.PlayerState.walking) && (shotUIScript.charge >= 1 || playerScript.debugSkills))
        {
            playerScript.playerSFXManager.PlayFeatherShot();
            anim.SetTrigger("Shoot");
            playerScript.playerSFXManager.PlayFeatherShot();
            playerScript.state = PlayerTestScript.PlayerState.shooting;
            shotUIScript.ResetCombo();
            GameObject fireBall = Instantiate(fireball, gameObject.transform.position, Quaternion.identity);
            FeatherScript fb = fireBall.GetComponent<FeatherScript>();
            fb.GetComponent<AttackInteract>().player = gameObject;
            if (playerScript.facingLeft)
            {
                fb.angle = 180;
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

    public void BallLightning()
    {

    }
}
