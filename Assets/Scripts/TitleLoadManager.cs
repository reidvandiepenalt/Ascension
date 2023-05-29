using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleLoadManager : MonoBehaviour
{
    [SerializeField] ChangeScenes changeScenes;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject creditsPage;
    [SerializeField] GameObject titleItems;
    [SerializeField] GameObject creditsBackButton;
    [SerializeField] GameObject titleCreditsButton;
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

    public void OpenCredits()
    {
        creditsPage.SetActive(true);
        titleItems.SetActive(false);
        EventSystem.current.SetSelectedGameObject(creditsBackButton);
    }

    public void CloseCredits()
    {
        creditsPage.SetActive(false);
        titleItems.SetActive(true);
        EventSystem.current.SetSelectedGameObject(titleCreditsButton);
    }
}
