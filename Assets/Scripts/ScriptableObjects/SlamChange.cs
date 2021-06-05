using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SlamChange : BlessEffect
{
    public SlamSwapTypes slamType;

    public override void Equip(PlayerTestScript player)
    {
        player.ChangeSlam(slamType);
    }

    public override void Unequip(PlayerTestScript player)
    {
        player.ChangeSlam(SlamSwapTypes.Default);
    }
}
