using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapItem : BossItem
{
    void Start()
    {
        if (PlayerInfo.Instance.mapUnlocked[TitleLoadManager.SAVE_SLOT]) Destroy(gameObject);
    }

    protected override void SetFlags()
    {
        PlayerInfo.Instance.mapUnlocked[TitleLoadManager.SAVE_SLOT] = true;
    }
}
