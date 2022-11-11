using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleLoadManager : MonoBehaviour
{
    [SerializeField] ChangeScenes changeScenes;
    [SerializeField] GameObject playerPrefab;

    public void LoadGame()
    {
        BossStatuses.Load();
        BlessingPickupInfo.Load();
        PlayerInfo.Load();

        changeScenes.levelName = PlayerInfo.Instance.sceneName;
        changeScenes.startPosition = PlayerInfo.Instance.loadPos;

        changeScenes.LoadScene();
        Instantiate(playerPrefab);

        //load in player settings (here or in player?)
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
