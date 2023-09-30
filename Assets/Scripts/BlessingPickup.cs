using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlessingPickup : MonoBehaviour
{
    [SerializeField] Blessing blessing;
    [SerializeField] GameObject pickupScreen;
    
    public enum BlessingPickups
    {
        TernaryAnima,
        IronAegis,
        DesertSun,
        LethalRecompense
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            switch (blessing.pickup)
            {
                case BlessingPickups.TernaryAnima:
                    BlessingPickupInfo.Instance.TernaryAnimaPickedUp[TitleLoadManager.SAVE_SLOT] = true;
                    break;
                case BlessingPickups.IronAegis:
                    BlessingPickupInfo.Instance.IronAegisPickedUp[TitleLoadManager.SAVE_SLOT] = true;
                    break;
                case BlessingPickups.DesertSun:
                    BlessingPickupInfo.Instance.DesertSunPickedUp[TitleLoadManager.SAVE_SLOT] = true;
                    break;
                case BlessingPickups.LethalRecompense:
                    BlessingPickupInfo.Instance.LethalRecompensePickedUp[TitleLoadManager.SAVE_SLOT] = true;
                    break;
                default:
                    print("pickup enum not created");
                    break;
            }

            Instantiate(pickupScreen);

            //anim
            StartCoroutine(nameof(Anim));
        }
        
    }

    protected virtual IEnumerator Anim()
    {
        Destroy(gameObject);
        return null;
    }
}
