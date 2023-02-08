using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FeatherShotChange : BlessEffect
{
    public FeatherTypes featherType;
    [SerializeField] Sprite UIicon;
    [SerializeField] AudioClip useSFX;

    public override void Equip(PlayerTestScript player)
    {
        player.ChangeFeather(featherType, useSFX, UIicon);
    }

    public override void Unequip(PlayerTestScript player)
    {
        player.ChangeFeather(FeatherTypes.Default, null);
    }

}
