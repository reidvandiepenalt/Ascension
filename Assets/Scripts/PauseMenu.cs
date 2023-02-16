using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI, settingMenuUI, SkillsUI;

    public GameObject pauseFirstButton, optionsFirstButton, optionsClosedButton;

    public AudioSource open, close;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Pause.pauseState == Pause.PauseState.Escape)
            {
                Resume();
            }
            else if(Pause.pauseState == Pause.PauseState.Playing){ SetPause(); }
        }
    }

    public void Resume()
    {
        close.Play();

        pauseMenuUI.SetActive(false);
        SkillsUI.SetActive(true);
        Time.timeScale = 1f;
        Pause.pauseState = Pause.PauseState.Playing;
        AudioListener.pause = false;
    }

    void SetPause()
    {

        pauseMenuUI.SetActive(true);
        SkillsUI.SetActive(false);
        Time.timeScale = 0f;
        Pause.pauseState = Pause.PauseState.Escape;
        AudioListener.pause = true;

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);

        open.Play();
    }

    public void OpenSettings()
    {
        settingMenuUI.SetActive(true);
        pauseMenuUI.SetActive(false);

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
    }

    public void CloseSettings()
    {
        settingMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsClosedButton);
    }

    public void Exit()
    {
        //go to main menu
        PlayerInfo.Save();
        BlessingPickupInfo.Save();
        BossStatuses.Save();
        Settings.Save();

        Destroy(PlayerTestScript.vmCam);
        Destroy(PlayerTestScript.mainCam.gameObject);
        Destroy(PlayerTestScript.eventSystem);
        Destroy(PlayerTestScript.pauseCanvas);
        Destroy(PlayerTestScript.inGameMenuCanvas);
        Destroy(PlayerTestScript.settingsCanvas);
        Destroy(PlayerTestScript.UICanvas);
        Destroy(PlayerTestScript.playerInstance);

        Time.timeScale = 1f;
        Pause.pauseState = Pause.PauseState.Playing;
        StartCoroutine(nameof(LoadMainMenu));
    }

    IEnumerator LoadMainMenu()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Title");
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }
}
