using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusItem : BossItem
{
    protected override void SetFlags()
    {
        PlayerInfo.Instance.doubleJumpUnlock = true;
        BossStatuses.Instance.horusKilled = true;
    }
}
