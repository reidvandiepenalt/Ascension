using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SprayChange : BlessEffect
{
    public SpraySwapTypes sprayType;

    public override void Equip(PlayerTestScript player)
    {
        player.ChangeSpray(sprayType);
    }

    public override void Unequip(PlayerTestScript player)
    {
        player.ChangeSpray(SpraySwapTypes.Default);
    }
}
