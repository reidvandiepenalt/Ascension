using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BlessEffect : ScriptableObject
{
    public abstract void Equip(PlayerTestScript player);
    public abstract void Unequip(PlayerTestScript player);
}
