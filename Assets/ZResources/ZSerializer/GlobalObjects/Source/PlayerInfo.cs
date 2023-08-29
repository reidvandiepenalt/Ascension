using System;
using ZSerializer;
using UnityEngine;
using System.Collections.Generic;

[Serializable, SerializeGlobalData(GlobalDataType.PerSaveFile)]
public partial class PlayerInfo
{
    public enum RoomTransitionStates
    {
        known,
        travelled
    }

    public enum EgyptRooms
    {
        egypt1,
        egypt2,
        egypt3,
        egypt4,
        egypt5,
        egypt7,
        egypt8,
        egypt13,
        bastet,
        bennu,
        horus
    }
    public enum EgyptTransitions
    {
        egypt1to2,
        egypt2to3,
        egypt2to4,
        egypt3to5,
        egypt5to8,
        egypt8toBastet,
        egypt4toHorus,
        egypt4to7,
        egypt7to13,
        egypt13toBennu,
    }

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
    public bool mapUnlocked = false;

    public Dictionary<EgyptRooms, RoomTransitionStates> travelledRooms = new Dictionary<EgyptRooms, RoomTransitionStates>();
    public Dictionary<EgyptTransitions, RoomTransitionStates> travelledTransitions = new Dictionary<EgyptTransitions, RoomTransitionStates>();
}
