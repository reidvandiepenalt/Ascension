using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Template for a blessing
/// </summary>
[CreateAssetMenu]
public class Blessing : ScriptableObject
{
    public string ItemName;
    public Sprite Icon;
    public int cost;
    public bool unlocked;
    public string description;
    public bool equipped;

    public BlessEffect blessScript;
    
}
