using System;
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
    [SerializeField] GameObject settingsPage;
    [SerializeField] GameObject settingsBackButton;
    [SerializeField] GameObject titleSettingsButton;
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject gameSlotsBackButton;
    [SerializeField] GameObject gameSlotsPage;

    static int saveSlot = 1;
    public static int SAVE_SLOT { get => saveSlot; }

    public void LoadGame(int slot)
    {
        saveSlot = slot;

        BossStatuses.Load();
        BlessingPickupInfo.Load();
        PlayerInfo.Load();

        try
        {
            changeScenes.levelName = PlayerInfo.Instance.sceneName[SAVE_SLOT];
            changeScenes.startPosition = PlayerInfo.Instance.loadPos[SAVE_SLOT];
        }
        catch
        {
            //save slot not instantiated
            PlayerInfo.Instance.Setup();
            PlayerInfo.Save();
            BlessingPickupInfo.Instance.Setup();
            BlessingPickupInfo.Save();
            BossStatuses.Instance.Setup();
            BossStatuses.Save();

            changeScenes.levelName = PlayerInfo.Instance.sceneName[SAVE_SLOT];
            changeScenes.startPosition = PlayerInfo.Instance.loadPos[SAVE_SLOT];
        }
        

        changeScenes.LoadScene();
        Instantiate(playerPrefab);

        //load in player settings (here or in player?)
    }

    public void DeleteSave(int slot)
    {
        //clear player save info

        PlayerInfo.Instance.DeleteSlot(slot);
        BlessingPickupInfo.Instance.DeleteSlot(slot);
        BossStatuses.Instance.DeleteSlot(slot);
        PlayerInfo.Save();
        BlessingPickupInfo.Save();
        BossStatuses.Save();
    }

    public void OpenGameSlots()
    {
        gameSlotsPage.SetActive(true);
        titleItems.SetActive(false);
        EventSystem.current.SetSelectedGameObject(gameSlotsBackButton);
    }

    public void CloseGameSlots()
    {
        gameSlotsPage.SetActive(false);
        titleItems.SetActive(true);
        EventSystem.current.SetSelectedGameObject(startGameButton);
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

    public void OpenSettings()
    {
        settingsPage.SetActive(true);
        titleItems.SetActive(false);
        EventSystem.current.SetSelectedGameObject(settingsBackButton);
    }

    public void CloseSettings()
    {
        settingsPage.SetActive(false);
        titleItems.SetActive(true);
        EventSystem.current.SetSelectedGameObject(titleSettingsButton);
    }
}
