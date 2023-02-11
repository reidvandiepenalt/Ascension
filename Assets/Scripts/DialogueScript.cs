using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueScript : MonoBehaviour
{
    public List<GameObject> texts;
    int index = 0;
    public PlayerTestScript player;

    // Start is called before the first frame update
    void Start()
    {
        Pause.pauseState = Pause.PauseState.Dialogue;
    }

    void Update()
    {
        if (Input.GetButtonDown("Submit"))
        {
            if(index == texts.Count - 1)
            {
                //end dialogue by disabling the canvas and resetting timescale
                gameObject.SetActive(false);
                Pause.pauseState = Pause.PauseState.Playing;
            }
            else
            {
                texts[index].SetActive(false);
                texts[++index].SetActive(true);
            }
        }
    }
}
