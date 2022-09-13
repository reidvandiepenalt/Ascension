using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPanelManager : MonoBehaviour
{
    Dictionary<PlayerInfo.EgyptRooms, Image> rooms;
    Dictionary<PlayerInfo.EgyptTransitions, LineRenderer> transitions;

    [SerializeField] List<Image> roomsOrder;
    [SerializeField] List<LineRenderer> linesOrder;


    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < roomsOrder.Count; i++)
        {
            rooms.Add((PlayerInfo.EgyptRooms)i, roomsOrder[i]);
        }
        for (int i = 0; i < linesOrder.Count; i++)
        {
            transitions.Add((PlayerInfo.EgyptTransitions) i, linesOrder[i]);
        }

        foreach (PlayerInfo.EgyptRooms room in PlayerInfo.Instance.travelledRooms)
        {
            rooms[room].gameObject.SetActive(true);
        }

        foreach(PlayerInfo.EgyptTransitions transition in PlayerInfo.Instance.travelledTransitions)
        {
            transitions[transition].gameObject.SetActive(true);
        }
    }
}
