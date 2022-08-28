using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnPoint : MonoBehaviour
{
    public int sceneIndex;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //update player spawn point if they enter this respawns area
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("rs collide");
            PlayerTestScript playerScript = collision.gameObject.GetComponent<PlayerTestScript>();
            playerScript.RespawnScene = sceneIndex;
            playerScript.SpawnPoint = gameObject.transform.position;
        }
    }
}
