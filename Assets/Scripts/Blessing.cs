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
    public bool unlocked {
        get
        {
            switch (pickup)
            {
                case BlessingPickup.BlessingPickups.TernaryAnima:
                    return BlessingPickupInfo.Instance.TernaryAnimaPickedUp;
                case BlessingPickup.BlessingPickups.IronAegis:
                    return BlessingPickupInfo.Instance.IronAegisPickedUp;
                case BlessingPickup.BlessingPickups.DesertSun:
                    return BlessingPickupInfo.Instance.DesertSunPickedUp;
                case BlessingPickup.BlessingPickups.LethalRecompense:
                    return BlessingPickupInfo.Instance.LethalRecompensePickedUp;
                default:
                    return false;
            }
        }
    }
    public string description;
    public bool equipped;
    public BlessingPickup.BlessingPickups pickup;

    public BlessEffect blessScript;

}
