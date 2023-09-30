using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template for a blessing
/// </summary>
[CreateAssetMenu]
public class Blessing : ScriptableObject
{
    public string ItemName;
    public Sprite Icon;
    public int cost;
    public bool Unlocked {
        get
        {
            return pickup switch
            {
                BlessingPickup.BlessingPickups.TernaryAnima => BlessingPickupInfo.Instance.TernaryAnimaPickedUp[TitleLoadManager.SAVE_SLOT],
                BlessingPickup.BlessingPickups.IronAegis => BlessingPickupInfo.Instance.IronAegisPickedUp[TitleLoadManager.SAVE_SLOT],
                BlessingPickup.BlessingPickups.DesertSun => BlessingPickupInfo.Instance.DesertSunPickedUp[TitleLoadManager.SAVE_SLOT],
                BlessingPickup.BlessingPickups.LethalRecompense => BlessingPickupInfo.Instance.LethalRecompensePickedUp[TitleLoadManager.SAVE_SLOT],
                _ => false,
            };
        }
    }
    public string description;
    public bool equipped;
    public BlessingPickup.BlessingPickups pickup;

    public BlessEffect blessScript;

}
