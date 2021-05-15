using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayTypes : MonoBehaviour
{
    public PlayerTestScript playerScript;
    public ProgressBarScript sprayUIScript;

    private float sprayTimer = 0.0f;
    [SerializeField] float sprayTimeLength = 0.35f;

    public Animator anim;
    public GameObject sprayFeather;


    public void DefaultSpray()
    {
        if (!playerScript.controller.collisions.below &&
            (playerScript.state == PlayerTestScript.PlayerState.gliding || playerScript.state == PlayerTestScript.PlayerState.idle) && sprayUIScript.charge >= 1)
        {
            anim.SetTrigger("Spray");
            playerScript.state = PlayerTestScript.PlayerState.spraying;
            sprayUIScript.ResetCombo();
            for (int angle = 240; angle <= 300; angle += 15)
            {
                GameObject feather = Instantiate(sprayFeather, gameObject.transform.position, Quaternion.identity);
                FeatherScript fs = feather.GetComponent<FeatherScript>();
                fs.GetComponent<AttackInteract>().player = gameObject;
                fs.angle = angle;
            }
            if (playerScript.velocity.y < 1) //mini jump
            {
                playerScript.velocity = new Vector2(playerScript.velocity.x, 1.0f);
            }
            else
            {
                playerScript.velocity = new Vector2(playerScript.velocity.x, playerScript.velocity.y + 1.0f);
            }
        }
        else if (playerScript.state == PlayerTestScript.PlayerState.spraying)
        {
            sprayTimer += Time.deltaTime;
            if (sprayTimer > sprayTimeLength)
            {
                playerScript.state = PlayerTestScript.PlayerState.idle;
                sprayTimer = 0.0f;
            }
        }
    }
}
