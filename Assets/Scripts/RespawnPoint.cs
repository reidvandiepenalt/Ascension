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
            PlayerInfo.Instance.respawnPos[TitleLoadManager.SAVE_SLOT] = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 1.5f, 0);
            PlayerInfo.Instance.sceneName[TitleLoadManager.SAVE_SLOT] = sceneName;
            /*
            PlayerTestScript playerScript = collision.gameObject.GetComponent<PlayerTestScript>();
            playerScript.RespawnScene = sceneName;
            playerScript.SpawnPoint = gameObject.transform.position;*/
        }
    }
}
