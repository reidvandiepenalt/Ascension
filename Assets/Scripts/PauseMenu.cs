using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{

    public static bool IsPaused = false;

    public GameObject pauseMenuUI, settingMenuUI, SkillsUI;

    public GameObject pauseFirstButton, optionsFirstButton, optionsClosedButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !InventoryMenu.InInventory)
        {
            if (IsPaused)
            {
                Resume();
            }
            else { Pause(); }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        SkillsUI.SetActive(true);
        Time.timeScale = 1f;
        IsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        SkillsUI.SetActive(false);
        Time.timeScale = 0f;
        IsPaused = true;

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
