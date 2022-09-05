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

    public bool backstepUnlock = true;
    public bool slamUnlock = false;
    public bool doubleJumpUnlock = false;
    public bool doubleJumpUpgrade = false;
    public bool chargeJumpUnlock = false;
    public bool sprayUnlock = false;
    public bool shootUnlock = false;
    public bool guardUnlock = false;
    public bool dashUnlock = false;
    public bool dashUpgrade = false;
}
