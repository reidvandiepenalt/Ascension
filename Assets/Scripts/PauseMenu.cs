using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI, settingMenuUI, SkillsUI;

    public GameObject pauseFirstButton, optionsFirstButton, optionsClosedButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !Pause.isPaused)
        {
            if (Pause.isPaused)
            {
                Resume();
            }
            else { SetPause(); }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        SkillsUI.SetActive(true);
        Time.timeScale = 1f;
        Pause.isPaused = false;
    }

    void SetPause()
    {
        pauseMenuUI.SetActive(true);
        SkillsUI.SetActive(false);
        Time.timeScale = 0f;
        Pause.isPaused = true;

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
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
    }
}
