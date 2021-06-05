using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public enum SlamSwapTypes
{
    Default,
    Wave,
    Poison
}

public class SlamTypes : MonoBehaviour
{
    public PlayerTestScript playerScript;
    public ProgressBarScript slamUIScript;

    public GameObject slamAttack;
    public Animator anim;

    [SerializeField] float slamMaxSpeed = 3.0f;
    [SerializeField] float slamStepper = 0.25f;

    public void DefaultSlam()
    {
        Transform endpoint = gameObject.transform;
        float distance = 0;
        if ((playerScript.state == PlayerTestScript.PlayerState.idle || playerScript.state == PlayerTestScript.PlayerState.gliding) &&
             !playerScript.controller.collisions.below && slamUIScript.charge >= 1)
        {
            RaycastHit2D normal = Physics2D.Raycast(gameObject.transform.position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
            if (normal.transform != null)
            {
                distance = gameObject.transform.position.y - normal.transform.position.y;
                if (distance > 4)
                {
                    anim.SetTrigger("PlayerSlamStart");
                    playerScript.state = PlayerTestScript.PlayerState.slamming;
                    playerScript.velocity = new Vector2(0, playerScript.velocity.y);
                    endpoint = normal.transform;
                    slamUIScript.ResetCombo();
                }
            }
        }
        else if (playerScript.controller.collisions.below && playerScript.state == PlayerTestScript.PlayerState.slamming)
        {
            playerScript.state = PlayerTestScript.PlayerState.idle;
            anim.SetTrigger("PlayerSlamEnd");
            GameObject slam = Instantiate(slamAttack, endpoint.position, quaternion.identity);
            SlamAttackScript slamScript = slam.GetComponent<SlamAttackScript>();
            AttackInteract attScript = slam.GetComponent<AttackInteract>();
            attScript.player = gameObject;
            slamScript.attackScript = attScript;
            if (!playerScript.facingLeft)
            {
                slamScript.right = true;
            }
            slamScript.fallDistance = distance;
        }
        else if (Math.Abs(playerScript.velocity.y) < slamMaxSpeed && playerScript.state == PlayerTestScript.PlayerState.slamming)
        {
            playerScript.velocity = new Vector3(playerScript.velocity.x, playerScript.velocity.y - slamStepper);
        }
    }


    public void Wave()
    {

    }

    public void Poison()
    {

    }
}
