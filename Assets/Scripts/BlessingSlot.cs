using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BlessingSlot : MonoBehaviour
{
    [SerializeField] Image image;
    public Sprite defaultSprite;
    public GameObject animObj;
    private GameObject animInst;

    [SerializeField] Blessing _blessing;
    public Blessing Blessing
    {
        get { return _blessing; }
        set
        {
            _blessing = value;

            if (_blessing == null)
            {
                image.sprite = defaultSprite;
            } else
            {
                image.sprite = _blessing.Icon;
            }
        }
    }

    private void OnValidate()
    {
        //in editor, update image to default sprite
        if (image == null)
        {
            image = GetComponent<Image>();
            defaultSprite = image.sprite;
        }
    }

    private void OnEnable()
    {
        //allow blessing to be equipped if unlocked
        if (Blessing && Blessing.unlocked)
        {
            image.sprite = Blessing.Icon;
            gameObject.GetComponent<Button>().enabled = true;
        }
    }

    /// <summary>
    /// Remove the blessing
    /// </summary>
    public void RemoveBlessing()
    {
        //remove and animate if equipped
        if (Blessing.equipped)
        {
            BlessingInventory.BlessingInventorySingleton.RemoveBlessing(this);
            RemoveAnim();
            Blessing.equipped = false;
        }
    }

    /// <summary>
    /// Adds the blessing to equipped list
    /// </summary>
    /// <param name="ignoreEquip">Ignore animation and only do setup</param>
    public void AddBlessing(bool ignoreEquip)
    {
        if (!Blessing.unlocked) { return; }
        if (Blessing.equipped && !ignoreEquip) { return; }
        //equip and animate
        if (BlessingInventory.BlessingInventorySingleton.AddBlessing(Blessing))
        {
            if (!ignoreEquip)
            {
                animInst = Instantiate(animObj, transform);
                animInst.GetComponent<Image>().sprite = image.sprite;
                Vector3 moveTo = BlessingInventory.BlessingInventorySingleton.ActiveBlessingSlots[^1].transform.position;
                animInst.LeanMove(moveTo, 0.25f).setIgnoreTimeScale(true).setDestroyOnComplete(true);
            }
            Blessing.equipped = true;
            ChangeColor(Color.gray);
            return;
        }
        animInst = Instantiate(animObj);
        animInst.transform.parent = transform;
        //animInst = Instantiate(animObj, transform);
        animInst.GetComponent<Image>().sprite = image.sprite;
        ChangeColor(Color.clear);
        animInst.LeanMoveLocalX(15f, 0.025f).setIgnoreTimeScale(true).setOnComplete(AnimLeft);
    }


    /// <summary>
    /// Removal animation of the blessing
    /// </summary>
    public void RemoveAnim()
    {
        Vector3 endpoint = Vector2.zero;
        //find the correct slot and set endpoint to its position
        foreach (BlessingSlot slot in BlessingInventory.BlessingInventorySingleton.blessingStorageSlots)
        {
            if (slot.Blessing == Blessing)
            {
                endpoint = slot.transform.position;
                break;
            }
        }
        GameObject removeInst = Instantiate(animObj, transform);
        removeInst.GetComponent<Image>().sprite = image.sprite;
        removeInst.LeanMove(endpoint, 0.25f).setIgnoreTimeScale(true).setOnComplete(DestroySelf).setDestroyOnComplete(true);
    }

    /// <summary>
    /// Destroy this game object
    /// </summary>
    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Change the image color to the given color
    /// </summary>
    public void ChangeColor(Color color)
    {
        image.color = color;
    }

    /// <summary>
    /// animate to the left
    /// </summary>
    private void AnimLeft()
    {
        animInst.LeanMoveLocalX(-15f, 0.05f).setIgnoreTimeScale(true).setOnComplete(AnimRight);
    }

    /// <summary>
    /// animate to the left
    /// </summary>
    private void AnimRight()
    {
        animInst.LeanMoveLocalX(15f, 0.05f).setIgnoreTimeScale(true).setOnComplete(AnimFinish);
    }

    /// <summary>
    /// final animation portion
    /// </summary>
    private void AnimFinish()
    {
        animInst.LeanMoveLocalX(0f, 0.0125f).setIgnoreTimeScale(true).setDestroyOnComplete(true);
        ChangeColor(Color.white);
    }

}
