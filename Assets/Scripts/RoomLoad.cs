using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomLoad : MonoBehaviour
{
    public PlayerInfo.EgyptRooms room;
    public Vector2 centerPoint;
    public List<PlayerInfo.EgyptRooms> adjacentRooms;
    public List<PlayerInfo.EgyptTransitions> transitions;

    private void Awake()
    {
        PlayerTestScript.curRoom = room;
        PlayerTestScript.curRoomCenterPoint = centerPoint;

        if (!PlayerInfo.Instance.travelledRooms.ContainsKey(room))
        {
            PlayerInfo.Instance.travelledRooms.Add(room, PlayerInfo.RoomTransitionStates.travelled);
        }else if(PlayerInfo.Instance.travelledRooms[room] == PlayerInfo.RoomTransitionStates.known)
        {
            PlayerInfo.Instance.travelledRooms[room] = PlayerInfo.RoomTransitionStates.travelled;
        }

        foreach(PlayerInfo.EgyptRooms adjRoom in adjacentRooms)
        {
            if (!PlayerInfo.Instance.travelledRooms.ContainsKey(adjRoom))
            {
                PlayerInfo.Instance.travelledRooms.Add(adjRoom, PlayerInfo.RoomTransitionStates.known);
            }
        }

        foreach (PlayerInfo.EgyptTransitions transition in transitions)
        {
            if (!PlayerInfo.Instance.travelledTransitions.ContainsKey(transition))
            {
                PlayerInfo.Instance.travelledTransitions.Add(transition, PlayerInfo.RoomTransitionStates.known);
            }
        }
    }
}
