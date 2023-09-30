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

        if (!PlayerInfo.Instance.travelledRooms[TitleLoadManager.SAVE_SLOT].ContainsKey(room))
        {
            PlayerInfo.Instance.travelledRooms[TitleLoadManager.SAVE_SLOT].Add(room, PlayerInfo.RoomTransitionStates.travelled);
        }else if(PlayerInfo.Instance.travelledRooms[TitleLoadManager.SAVE_SLOT][room] == PlayerInfo.RoomTransitionStates.known)
        {
            PlayerInfo.Instance.travelledRooms[TitleLoadManager.SAVE_SLOT][room] = PlayerInfo.RoomTransitionStates.travelled;
        }

        foreach(PlayerInfo.EgyptRooms adjRoom in adjacentRooms)
        {
            if (!PlayerInfo.Instance.travelledRooms[TitleLoadManager.SAVE_SLOT].ContainsKey(adjRoom))
            {
                PlayerInfo.Instance.travelledRooms[TitleLoadManager.SAVE_SLOT].Add(adjRoom, PlayerInfo.RoomTransitionStates.known);
            }
        }

        foreach (PlayerInfo.EgyptTransitions transition in transitions)
        {
            if (!PlayerInfo.Instance.travelledTransitions[TitleLoadManager.SAVE_SLOT].ContainsKey(transition))
            {
                PlayerInfo.Instance.travelledTransitions[TitleLoadManager.SAVE_SLOT].Add(transition, PlayerInfo.RoomTransitionStates.known);
            }
        }
    }
}
