using System;
using ZSerializer;
using UnityEngine;

[Serializable, SerializeGlobalData(GlobalDataType.PerSaveFile)]
public partial class PlayerInfo
{
    //  Add serializable variables to this object to be able to serialize and access them.
    public string sceneName;
    public Vector3 loadPos;


    public int maxHealth;
    public bool skillUnlocked;
}
