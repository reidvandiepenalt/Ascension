using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts;
using System;

public class BlessingInventory : MonoBehaviour, ICanvas
{

    [SerializeField] Transform blessingsStorage;
    [SerializeField] Transform activeBlessGrid;
    [SerializeField] Transform counterGrid;
    public BlessingSlot[] blessingStorageSlots;
    public List<BlessingSlot> ActiveBlessingSlots;

    public TextMeshProUGUI headerText;
    public TextMeshProUGUI descriptionText;

    public InventoryMenu superMenu;

    public GameObject activeBlessCounterPrefab;
    public GameObject activeBlessSlotPrefab;

    public PlayerTestScript player;

    public static BlessingInventory BlessingInventorySingleton = null;

    bool initBlessingEquip = false;

    private void OnValidate()
    {
        //in editor, get all storage slots and add to blessingStorageSlots
        if (blessingsStorage != null)
        {
            blessingStorageSlots = blessingsStorage.GetComponentsInChildren<BlessingSlot>();
        }
    }

    private void Awake()
    {
        BlessingInventorySingleton = this;
    }

    private void Start()
    {
        //create a marker for each bless notch
        for (int i = 0; i < PlayerTestScript.blessingTotal; i++)
        {
            Instantiate(activeBlessCounterPrefab, counterGrid);
        }

        //set selected
        EventSystem.current.SetSelectedGameObject(blessingStorageSlots[0].gameObject);
        superMenu.SelectedGameObject = EventSystem.current.currentSelectedGameObject;

        if (blessingsStorage != null)
        {
            blessingStorageSlots = blessingsStorage.GetComponentsInChildren<BlessingSlot>();
        }

 
    }

    private void Update()
    {
        if (!initBlessingEquip)
        {
            try
            {
                foreach (BlessingSlot blessingSlot in blessingStorageSlots)
                {
                    if (blessingSlot.Blessing)
                    {
                        if (blessingSlot.Blessing.equipped) blessingSlot.AddBlessing(true);
                    }
                }
                initBlessingEquip = true;
                gameObject.SetActive(false);
            }
            catch {}
            
        }   
    }

    /// <summary>
    /// Update description text to selected blessing
    /// </summary>
    /// <param name="selected">Selected blessing</param>
    public void UpdateDescriptions(Blessing selected)
    {
        headerText.text = selected.ItemName;
        descriptionText.text = selected.description;
    }

    /// <summary>
    /// Adds blessing to equipped blessings
    /// </summary>
    /// <param name="blessing">Blessing to equip</param>
    /// <returns>Returns true if blessing was equipped, false otherwise</returns>
    public bool AddBlessing(Blessing blessing)
    {
        //check if there is room to equip
        if (IsFull(blessing.cost)) { return false; }

        //Get rid of counter tokens
        for(int i = 0; i < blessing.cost; i++)
        {
            Destroy(counterGrid.GetChild(i).gameObject);
        }

        PlayerTestScript.blessingTotal -= blessing.cost;

        //Add blessing to equipped grid
        ActiveBlessingSlots.Add(Instantiate(activeBlessSlotPrefab, activeBlessGrid).GetComponent<BlessingSlot>());
        ActiveBlessingSlots[^1].Blessing = blessing;
        ActiveBlessingSlots[^1].Blessing.blessScript.Equip(player);

        return true;
    }

    /// <summary>
    /// Remove the selected blessing
    /// </summary>
    /// <param name="blessSlot">Blessing to remove</param>
    /// <returns>Returns true if blessing was removed</returns>
    public bool RemoveBlessing(BlessingSlot blessSlot)
    {
        //check if blessing is equipped
        if (!ActiveBlessingSlots.Contains(blessSlot)) { return false; }

        //update player
        blessSlot.Blessing.blessScript.Unequip(player);

        //add back space/counters
        PlayerTestScript.blessingTotal += blessSlot.Blessing.cost;
        for(int i = 0; i < blessSlot.Blessing.cost; i++)
        {
            GameObject.Instantiate(activeBlessCounterPrefab, counterGrid);
        }

        //remove from active grid and add back to collected grid
        int index = ActiveBlessingSlots.IndexOf(blessSlot);
        if (ActiveBlessingSlots.Count > 1)
        {
            if(index == ActiveBlessingSlots.Count - 1)
            {
                EventSystem.current.SetSelectedGameObject(ActiveBlessingSlots[index - 1].gameObject);
                superMenu.SelectedGameObject = EventSystem.current.currentSelectedGameObject;
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(ActiveBlessingSlots[index + 1].gameObject);
                superMenu.SelectedGameObject = EventSystem.current.currentSelectedGameObject;
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(blessingStorageSlots[0].gameObject);
            superMenu.SelectedGameObject = EventSystem.current.currentSelectedGameObject;
        }

        ActiveBlessingSlots.RemoveAt(index);
        foreach (BlessingSlot slot in blessingStorageSlots)
        {
            if (slot.Blessing == blessSlot.Blessing)
            {
                slot.ChangeColor(Color.white);
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if there is space to equip blessing
    /// </summary>
    public bool IsFull(int cost)
    {
        return cost > PlayerTestScript.blessingTotal;
    }


    /// <summary>
    /// Updates selections
    /// </summary>
    public void UpdatedSelection(GameObject newSelection, GameObject oldSelection)
    {
        if (newSelection.CompareTag("BlessSlot"))
        {
            //update the description text
            UpdateDescriptions(newSelection.GetComponent<BlessingSlot>().Blessing);
        }
    }


    /// <summary>
    /// Do closing set up as needed
    /// </summary>
    public void Closing()
    {
        
    }
}
