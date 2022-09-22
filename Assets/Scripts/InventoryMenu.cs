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
    [SerializeField] const int CanvasIndex = 1;

    public PlayerTestScript player;

    [SerializeField] ICanvas[] canvasScripts = new ICanvas[3];

    public GameObject highlight;
    GameObject lastHighlight;
    GameObject highlightInst = null;
    public Sprite unfoundHighlight;
    private GameObject selectedGameObject;
    public GameObject SelectedGameObject
    {
        get { return selectedGameObject; }
        set
        {
            EventSystem.current.firstSelectedGameObject = value;
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
                        print("previous arrow");
                        GameObject lastArrow = selectedGameObject;
                        StartCoroutine(lastArrow.GetComponentInChildren<HighlightScript>().Fade(0.5f, 0f, 0.2f, false));
                    }
                }
                if (value.CompareTag("Arrow"))
                {
                    print("current arrow");
                    StartCoroutine(value.GetComponentInChildren<HighlightScript>().Fade(0f, 0.5f, 0.2f, false));
                }

                canvasScripts[CanvasIndex].UpdatedSelection(value, selectedGameObject);
                selectedGameObject = value;

                if (selectedGameObject.CompareTag("BlessSlot"))
                {
                    highlightInst = Instantiate(highlight, selectedGameObject.transform);

                    if (!selectedGameObject.GetComponent<BlessingSlot>().Blessing.unlocked)
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
        if (EventSystem.current.firstSelectedGameObject == null){ 
            EventSystem.current.firstSelectedGameObject = inventoryFirstSlot;
            print("setting first selected");
        }

        //Load current canvas?

        //get all three canvases scripts
        canvasScripts[0] = null; //Canvases[0].GetComponent<MapCanvasScript>();
        canvasScripts[1] = Canvases[1].GetComponent<BlessingInventory>();
        canvasScripts[2] = null; //Canvases[2].GetComponent<SwordInvetoryScript>();

        //canvas scripts setups
        Canvases[1].GetComponent<BlessingInventory>().player = player;
    }

    // Update is called once per frame
    void Update()
    {
        //pause/unpause
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Pause.isPaused)
            {
                Resume();
            } else
            {
                SetPause();
            }
        }

        //do nothing if not in pause menu
        if (!Pause.isPaused) { return; }

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
        Pause.isPaused = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// To be called when entering the menu (game being paused)
    /// </summary>
    void SetPause()
    {
        //activate ui elements and canvases
        Canvases[1].SetActive(true);
        foreach (GameObject arrow in Arrows)
        {
            arrow.SetActive(true);
        }
        SkillsUICanvas.SetActive(false);
        Pause.isPaused = true;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Scroll one canvas left or right
    /// </summary>
    /// <param name="direction">1 for right, -1 for left</param>
    public void Scroll(int direction)
    {
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
    }
}
