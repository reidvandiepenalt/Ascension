using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusItem : BossItem
{
    protected override void SetFlags()
    {
        PlayerInfo.Instance.doubleJumpUnlock[TitleLoadManager.SAVE_SLOT] = true;
        BossStatuses.Instance.horusKilled[TitleLoadManager.SAVE_SLOT] = true;
    }
}
