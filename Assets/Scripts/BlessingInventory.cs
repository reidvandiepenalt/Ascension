using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts;


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

    private void OnValidate()
    {
        if (blessingsStorage != null)
        {
            blessingStorageSlots = blessingsStorage.GetComponentsInChildren<BlessingSlot>();
        }
    }

    private void Start()
    {
        //create a marker for each bless notch
        for (int i = 0; i < PlayerTestScript.blessingTotal; i++)
        {
                GameObject.Instantiate(activeBlessCounterPrefab, counterGrid);
        }

        for(int i = 0; i < blessingStorageSlots.Length; i++)
        {
            if (blessingStorageSlots[i].Blessing.equipped) { blessingStorageSlots[i].AddBlessing(true); }
        }

        EventSystem.current.SetSelectedGameObject(blessingStorageSlots[0].gameObject);
        superMenu.SelectedGameObject = EventSystem.current.currentSelectedGameObject;
    }

    public void UpdateDescriptions(Blessing selected)
    {
        headerText.text = selected.ItemName;
        descriptionText.text = selected.description;
    }

    public bool AddBlessing(Blessing blessing)
    {
        if (IsFull(blessing.cost)) { return false; }

        for(int i = 0; i < blessing.cost; i++)
        {
            Destroy(counterGrid.GetChild(i).gameObject);
        }

        PlayerTestScript.blessingTotal -= blessing.cost;

        ActiveBlessingSlots.Add(Instantiate(activeBlessSlotPrefab, activeBlessGrid).GetComponent<BlessingSlot>()); ;
        ActiveBlessingSlots[ActiveBlessingSlots.Count - 1].Blessing = blessing;
        ActiveBlessingSlots[ActiveBlessingSlots.Count - 1].Blessing.blessScript.Equip(player);

        return true;
    }

    public bool RemoveBlessing(BlessingSlot blessSlot)
    {
        if (!ActiveBlessingSlots.Contains(blessSlot)) { return false; }

        blessSlot.Blessing.blessScript.Unequip(player);

        PlayerTestScript.blessingTotal += blessSlot.Blessing.cost;
        for(int i = 0; i < blessSlot.Blessing.cost; i++)
        {
            GameObject.Instantiate(activeBlessCounterPrefab, counterGrid);
        }

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

    public bool IsFull(int cost)
    {
        return cost > PlayerTestScript.blessingTotal;
    }

    public void UpdatedSelection(GameObject newSelection, GameObject oldSelection)
    {
        if (newSelection.CompareTag("BlessSlot"))
        {
            UpdateDescriptions(newSelection.GetComponent<BlessingSlot>().Blessing);
        }
    }

    public void Closing()
    {
        
    }
}
