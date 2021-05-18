using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //update player spawn point if they enter this respawns area
        if(collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerTestScript>().SpawnPoint = gameObject.transform.position;
        }
    }
}
