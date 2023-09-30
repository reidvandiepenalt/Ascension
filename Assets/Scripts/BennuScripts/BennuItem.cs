using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BennuItem : BossItem
{
    protected override void SetFlags()
    {
        PlayerInfo.Instance.shootUnlock[TitleLoadManager.SAVE_SLOT] = true;
        //raise signal to player
        BossStatuses.Instance.bennuKilled[TitleLoadManager.SAVE_SLOT] = true;
    }
}
