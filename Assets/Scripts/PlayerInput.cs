using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PlayerTestScript))] 
public class PlayerInput : MonoBehaviour
{
    PlayerTestScript player;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerTestScript>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);

        if (Input.GetButtonDown("Jump"))
        {
            player.OnJumpDown();
        }
        if (Input.GetButton("Jump"))
        {
            player.OnJumpHeld();
        }
        if (Input.GetButtonUp("Jump"))
        {
            player.OnJumpUp();
        }
    }
}
