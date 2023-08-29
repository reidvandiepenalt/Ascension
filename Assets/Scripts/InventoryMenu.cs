using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts;
using System;

public class InventoryMenu : MonoBehaviour
{
    public GameObject inventoryFirstSlot;
    public GameObject SkillsUICanvas;
    [SerializeField] GameObject[] Canvases;
    [SerializeField] GameObject[] Arrows;
    const int CanvasIndex = 1;

    public PlayerTestScript player;
    public AudioSource open;
    public AudioSource close;
    public AudioSource scroll;

    bool scrolling = false;

    [SerializeField] ICanvas[] canvasScripts = new ICanvas[3];
    MapMenu mapMenu;

    public GameObject highlight;
    GameObject lastHighlight;
    GameObject highlightInst = null;
    public Sprite unfoundHighlight;

    public GameObject firstSelected;
    private GameObject selectedGameObject;
    public GameObject SelectedGameObject
    {
        get { return selectedGameObject; }
        set
        {
            //fades ui elements when they are set?
            if (value != null)
            {
                if (highlightInst != null && Canvases[CanvasIndex].CompareTag("BlessInventory"))
                {
                    //fade out and then destroy
                    lastHighlight = Instantiate(highlightInst, selectedGameObject.transform);
                    Destroy(highlightInst);
                    StartCoroutine(lastHighlight.GetComponent<HighlightScript>().Fade(0.5f, 0f, 0.2f, true));
                }
                if (selectedGameObject != null)
                {
                    if (selectedGameObject.CompareTag("Arrow"))
                    {
                        GameObject lastArrow = selectedGameObject;
                        StartCoroutine(lastArrow.GetComponentInChildren<HighlightScript>().Fade(0.5f, 0f, 0.2f, false));
                    }
                }
                if (value.CompareTag("Arrow"))
                {
                    StartCoroutine(value.GetComponentInChildren<HighlightScript>().Fade(0f, 0.5f, 0.2f, false));
                }

                canvasScripts[CanvasIndex].UpdatedSelection(value, selectedGameObject);
                selectedGameObject = value;

                if (selectedGameObject.CompareTag("BlessSlot"))
                {
                    highlightInst = Instantiate(highlight, selectedGameObject.transform);

                    if (!selectedGameObject.GetComponent<BlessingSlot>().Blessing.Unlocked)
                    {
                        highlightInst.GetComponent<Image>().sprite = unfoundHighlight;
                    }
                    StartCoroutine(highlightInst.GetComponent<HighlightScript>().Fade(0f, 0.5f, 0.2f, false));
                }
            }
        }
    }

    private void Start()
    {
        if (inventoryFirstSlot.GetComponent<BlessingSlot>().Blessing.Unlocked)
        {
            firstSelected = inventoryFirstSlot;
        }
        else
        {
            firstSelected = Arrows[0];
        }

        //Load current canvas?

        //get all three canvases scripts
        canvasScripts[0] = mapMenu = Canvases[0].GetComponent<MapMenu>();
        canvasScripts[1] = Canvases[1].GetComponent<BlessingInventory>();
        canvasScripts[2] = null; //Canvases[2].GetComponent<SwordInvetoryScript>();

        //canvas scripts setups
        Canvases[1].GetComponent<BlessingInventory>().player = player;
    }

    // Update is called once per frame
    void Update()
    {
        //pause/unpause
        if (Input.GetButtonDown("Inventory"))
        {
            if (Pause.pauseState == Pause.PauseState.Inventory)
            {
                Resume();
            } else if(Pause.pauseState == Pause.PauseState.Playing)
            {
                SetPause();
            }
        }else if(Input.GetButtonDown("Cancel"))
        {
            if (Pause.pauseState == Pause.PauseState.Inventory)
            {
                Resume();
            }
        }

        //do nothing if not in pause menu
        if (Pause.pauseState != Pause.PauseState.Inventory) { return; }

        //selection handling
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(SelectedGameObject);
        }
        else if (SelectedGameObject != EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject != null)
        {
            SelectedGameObject = EventSystem.current.currentSelectedGameObject;
        }
    }

    /// <summary>
    /// To be called when exiting menu (resuming game)
    /// </summary>
    void Resume()
    {
        close.Play();

        firstSelected = SelectedGameObject;

        //close each canvas
        foreach(ICanvas canvas in canvasScripts)
        {
            if(canvas != null)
            {
                canvas.Closing();
            }
        }

        //set canvases and others inactive
        Canvases[1].SetActive(false);
        foreach (GameObject arrow in Arrows)
        {
            arrow.SetActive(false);
        }
        SkillsUICanvas.SetActive(true);
        Pause.pauseState = Pause.PauseState.Playing;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    /// <summary>
    /// To be called when entering the menu (game being paused)
    /// </summary>
    void SetPause()
    {
        open.Play();

        //activate ui elements and canvases
        foreach (GameObject arrow in Arrows)
        {
            arrow.SetActive(true);
        }
        Canvases[1].SetActive(true);
        SkillsUICanvas.SetActive(false);
        mapMenu.UpdatePanels();
        Pause.pauseState = Pause.PauseState.Inventory;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        EventSystem.current.firstSelectedGameObject = firstSelected;
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    /// <summary>
    /// Scroll one canvas left or right
    /// </summary>
    /// <param name="direction">1 for right, -1 for left</param>
    public void Scroll(int direction)
    {
        if (scrolling) return;
        scrolling = true;

        scroll.Play();

        //animate canvas movements and bring new canvas to center
        Vector3[] positions = new Vector3[] { Canvases[0].transform.position, Canvases[1].transform.position, Canvases[2].transform.position };
        if (direction >= 0) //right
        {
            Canvases[2].SetActive(true);
            Canvases[0].transform.position = positions[2];
            Canvases[1].LeanMoveX(positions[0].x, 0.25f).setIgnoreTimeScale(true).setOnComplete(() => DeactivateCanvas(Canvases[0]));
            Canvases[2].LeanMoveX(positions[1].x, 0.25f).setIgnoreTimeScale(true);
            GameObject[] temp2 = new GameObject[] { Canvases[1], Canvases[2], Canvases[0] };
            Canvases = temp2;
            return;
        }
        //left
        Canvases[0].SetActive(true);
        Canvases[0].LeanMoveX(positions[1].x, 0.25f).setIgnoreTimeScale(true);
        Canvases[1].LeanMoveX(positions[2].x, 0.25f).setIgnoreTimeScale(true).setOnComplete(() => DeactivateCanvas(Canvases[2]));
        Canvases[2].transform.position = positions[0];
        GameObject[] temp = new GameObject[] { Canvases[2], Canvases[0], Canvases[1] };
        Canvases = temp;
    }

    /// <summary>
    /// Deactivates a given canvas
    /// </summary>
    void DeactivateCanvas(GameObject canvas)
    {
        canvas.SetActive(false);
        scrolling = false;
    }
}
