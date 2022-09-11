using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomLoad : MonoBehaviour
{
    public PlayerInfo.EgyptRooms room;

    private void Awake()
    {
        if (!PlayerInfo.Instance.travelledRooms.Contains(room))
        {
            PlayerInfo.Instance.travelledRooms.Add(room);
        }
    }
}
