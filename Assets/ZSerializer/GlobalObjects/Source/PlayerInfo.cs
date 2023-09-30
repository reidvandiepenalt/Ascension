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
    public string[] sceneName = { "TutorialRoom1", "TutorialRoom1", "TutorialRoom1" };
    public Vector3[] loadPos = { new Vector3(-18, -3, 0), new Vector3(-18, -3, 0), new Vector3(-18, -3, 0) };


    public int[] maxHealth = new int[3];

    public bool[] backstepUnlock = {false, false, false};
    public bool[] slamUnlock = { false, false, false };
    public bool[] doubleJumpUnlock = {false, false, false};
    public bool[] doubleJumpUpgrade = {false, false, false};
    public bool[] chargeJumpUnlock = {false, false, false};
    public bool[] sprayUnlock = {false, false, false};
    public bool[] shootUnlock = {false, false, false};
    public bool[] guardUnlock = {false, false, false};
    public bool[] dashUnlock = {false, false, false};
    public bool[] dashUpgrade = {false, false, false};
    public bool[] mapUnlocked = { false, false, false };

    public Dictionary<EgyptRooms, RoomTransitionStates>[] travelledRooms = { new Dictionary<EgyptRooms, RoomTransitionStates>(), new Dictionary<EgyptRooms, RoomTransitionStates>(), new Dictionary<EgyptRooms, RoomTransitionStates>() };
    public Dictionary<EgyptTransitions, RoomTransitionStates>[] travelledTransitions = { new Dictionary<EgyptTransitions, RoomTransitionStates>(), new Dictionary<EgyptTransitions, RoomTransitionStates>(), new Dictionary<EgyptTransitions, RoomTransitionStates>() };

    public void Setup()
    {
        backstepUnlock = new bool[3];
        slamUnlock = new bool[3];
        doubleJumpUnlock = new bool[3];
        doubleJumpUpgrade = new bool[3];
        chargeJumpUnlock = new bool[3];
        sprayUnlock = new bool[3];
        shootUnlock = new bool[3];
        guardUnlock = new bool[3];
        dashUnlock = new bool[3];
        dashUpgrade = new bool[3];
        mapUnlocked = new bool[3];
        sceneName = new string[3];
        sceneName[0] = "TutorialRoom1";
        sceneName[1] = "TutorialRoom1";
        sceneName[2] = "TutorialRoom1";
        loadPos = new Vector3[3];
        loadPos[0] = new Vector3(-18, -3, 0);
        loadPos[1] = new Vector3(-18, -3, 0);
        loadPos[2] = new Vector3(-18, -3, 0);
        travelledRooms = new Dictionary<EgyptRooms, RoomTransitionStates>[3];
        travelledRooms[0] = new Dictionary<EgyptRooms, RoomTransitionStates>();
        travelledRooms[1] = new Dictionary<EgyptRooms, RoomTransitionStates>();
        travelledRooms[2] = new Dictionary<EgyptRooms, RoomTransitionStates>();
        travelledTransitions = new Dictionary<EgyptTransitions, RoomTransitionStates>[3];
        travelledTransitions[0] = new Dictionary<EgyptTransitions, RoomTransitionStates>();
        travelledTransitions[1] = new Dictionary<EgyptTransitions, RoomTransitionStates>();
        travelledTransitions[2] = new Dictionary<EgyptTransitions, RoomTransitionStates>();
        maxHealth = new int[3];
        maxHealth[0] = 5;
        maxHealth[1] = 5;
        maxHealth[2] = 5;
    }
}
