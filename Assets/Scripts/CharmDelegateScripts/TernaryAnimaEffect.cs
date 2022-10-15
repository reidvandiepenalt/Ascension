using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TernaryAnimaEffect : BlessEffect
{
    [SerializeField] GameObject animaPrefab;
    private GameObject animaInst;

    public override void Equip(PlayerTestScript player)
    {
        animaInst = Instantiate(animaPrefab, player.transform);
    }

    public override void Unequip(PlayerTestScript player)
    {
        Destroy(animaInst);
    }
}
