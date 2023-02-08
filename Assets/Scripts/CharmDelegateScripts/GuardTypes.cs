using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GuardSwapTypes
{
    Default,
    QuickCharge,
    Retaliation
}

public class GuardTypes : MonoBehaviour
{
    public float hitguardCD = 6.0f;

    public Animator anim;
    public PlayerTestScript playerScript;
    public GuardUIScript guardUIScript;

    public void DefaultGuard()
    {
        if ((playerScript.state == PlayerTestScript.PlayerState.idle || 
            playerScript.state == PlayerTestScript.PlayerState.walking) && (guardUIScript.mask.fillAmount <= 0) 
            && playerScript.controller.collisions.below)
        {
            playerScript.playerSFXManager.PlayGuard();
            anim.SetBool("Guarding", true);
            playerScript.state = PlayerTestScript.PlayerState.guarding;
            playerScript.velocity = new Vector2(0, 0);

        }
        else if (playerScript.state == PlayerTestScript.PlayerState.guarding && !Input.GetKey("y"))
        {
            Unguard(false);
        }
    }

    public void Unguard(bool hit)
    {
        if (hit)
        {
            guardUIScript.current = guardUIScript.maximum;
        }
        anim.SetBool("Guarding", false);
        playerScript.state = PlayerTestScript.PlayerState.idle;
    }

    public void Retaliation()
    {

    }
}
