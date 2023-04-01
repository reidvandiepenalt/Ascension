using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCutscene : MonoBehaviour
{
    [SerializeField] AudioSource sfx;
    [SerializeField] GameObject fadePanel;
    [SerializeField] ChangeScenes changeScenes;

    bool hasStartedSFX = false;

    private void Update()
    {
        if (!sfx.isPlaying && hasStartedSFX)
        {
            changeScenes.LoadScene();
        }
    }

    void PlayCutsceneSFX()
    {
        sfx.Play();
        hasStartedSFX = true;
    }

    public void PlayCutscene()
    {
        fadePanel.SetActive(true);
        Invoke(nameof(PlayCutsceneSFX), 2);
    }
}
