using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class BastetItem : BossItem
{
    protected override void SetFlags()
    {
        PlayerInfo.Instance.guardUnlock = true;
        //raise signal to player to check unlocks
        BossStatuses.Instance.bastKilled = true;
    }
}
