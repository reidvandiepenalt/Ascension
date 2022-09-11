using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPanelManager : MonoBehaviour
{
    [SerializeField] Dictionary<PlayerInfo.EgyptRooms, Image> rooms;
    [SerializeField] Dictionary<PlayerInfo.EgyptTransitions, LineRenderer> transitions;

    

    // Start is called before the first frame update
    void Start()
    {
        foreach(PlayerInfo.EgyptRooms room in PlayerInfo.Instance.travelledRooms)
        {
            rooms[room].gameObject.SetActive(true);
        }

        foreach(PlayerInfo.EgyptTransitions transition in PlayerInfo.Instance.travelledTransitions)
        {
            transitions[transition].gameObject.SetActive(true);
        }
    }
}
