using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GuardChange : BlessEffect
{
    public GuardSwapTypes guardType;

    public override void Equip(PlayerTestScript player)
    {
        player.ChangeGuard(guardType);
    }

    public override void Unequip(PlayerTestScript player)
    {
        player.ChangeGuard(GuardSwapTypes.Default);
    }
}
