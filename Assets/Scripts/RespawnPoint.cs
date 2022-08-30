using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnPoint : MonoBehaviour
{
    public string sceneName;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //update player spawn point if they enter this respawns area
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerTestScript playerScript = collision.gameObject.GetComponent<PlayerTestScript>();
            playerScript.RespawnScene = sceneName;
            playerScript.SpawnPoint = gameObject.transform.position;
        }
    }
}
