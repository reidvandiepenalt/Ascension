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
    public BlessingInventory inventory;
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
        if (image == null)
        {
            image = GetComponent<Image>();
            defaultSprite = image.sprite;
        }
    }

    private void OnEnable()
    {
        inventory = GetComponentInParent<BlessingInventory>();
        if (Blessing.unlocked)
        {
            image.sprite = Blessing.Icon;
            gameObject.GetComponent<Button>().enabled = true;
        }
    }

    public void RemoveBlessing()
    {
        if (Blessing.equipped)
        {
            inventory.RemoveBlessing(this);
            RemoveAnim();
            Blessing.equipped = false;
        }
    }

    public void AddBlessing(bool ignoreEquip)
    {
        if (!Blessing.unlocked) { return; }
        if (Blessing.equipped && !ignoreEquip) { return; }
        if (inventory.AddBlessing(Blessing))
        {
            if (!ignoreEquip)
            {
                animInst = Instantiate(animObj, transform);
                animInst.GetComponent<Image>().sprite = image.sprite;
                Vector3 moveTo = inventory.ActiveBlessingSlots[inventory.ActiveBlessingSlots.Count - 1].transform.position;
                animInst.LeanMove(moveTo, 0.25f).setIgnoreTimeScale(true).setDestroyOnComplete(true);
            }
            Blessing.equipped = true;
            ChangeColor(Color.gray);
            return;
        }
        animInst = Instantiate(animObj, transform);
        animInst.GetComponent<Image>().sprite = image.sprite;
        ChangeColor(Color.clear);
        animInst.LeanMoveLocalX(15f, 0.025f).setIgnoreTimeScale(true).setOnComplete(AnimLeft);
    }

    public void RemoveAnim()
    {
        Vector3 endpoint = new Vector3(0,0);
        foreach (BlessingSlot slot in inventory.blessingStorageSlots)
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

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void ChangeColor(Color color)
    {
        image.color = color;
    }

    private void AnimLeft()
    {
        animInst.LeanMoveLocalX(-15f, 0.05f).setIgnoreTimeScale(true).setOnComplete(AnimRight);
    }

    private void AnimRight()
    {
        animInst.LeanMoveLocalX(15f, 0.05f).setIgnoreTimeScale(true).setOnComplete(AnimFinish);
    }

    private void AnimFinish()
    {
        animInst.LeanMoveLocalX(0f, 0.0125f).setIgnoreTimeScale(true).setDestroyOnComplete(true);
        ChangeColor(Color.white);
    }

}
