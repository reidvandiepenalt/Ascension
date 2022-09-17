using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPanelManager : MonoBehaviour
{
    Dictionary<PlayerInfo.EgyptRooms, Image> rooms = new Dictionary<PlayerInfo.EgyptRooms, Image>();
    Dictionary<PlayerInfo.EgyptTransitions, LineRenderer> transitions = new Dictionary<PlayerInfo.EgyptTransitions, LineRenderer>();

    [SerializeField] List<Image> roomsOrder;
    [SerializeField] List<LineRenderer> linesOrder;

    [SerializeField] Color knownColor;
    [SerializeField] Color travelledColor;
    [SerializeField] Material knownMaterial;
    [SerializeField] Material travelledMaterial;

    [SerializeField] RectTransform playerIcon;

    public Transform playerTransform;


    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i = 0; i < roomsOrder.Count; i++)
        {
            rooms.Add((PlayerInfo.EgyptRooms)i, roomsOrder[i]);
        }
        for (int i = 0; i < linesOrder.Count; i++)
        {
            transitions.Add((PlayerInfo.EgyptTransitions)i, linesOrder[i]);
        }
    }

    private void OnEnable()
    {
        foreach (PlayerInfo.EgyptRooms room in PlayerInfo.Instance.travelledRooms.Keys)
        {
            rooms[room].gameObject.SetActive(true);
            switch (PlayerInfo.Instance.travelledRooms[room])
            {
                case PlayerInfo.RoomTransitionStates.known:
                    rooms[room].color = knownColor;
                    break;
                case PlayerInfo.RoomTransitionStates.travelled:
                    rooms[room].color = travelledColor;
                    break;
            }
        }

        foreach (PlayerInfo.EgyptTransitions transition in PlayerInfo.Instance.travelledTransitions.Keys)
        {
            transitions[transition].gameObject.SetActive(true);
            switch (PlayerInfo.Instance.travelledTransitions[transition])
            {
                case PlayerInfo.RoomTransitionStates.known:
                    transitions[transition].sharedMaterial = knownMaterial;
                    break;
                case PlayerInfo.RoomTransitionStates.travelled:
                    transitions[transition].sharedMaterial = travelledMaterial;
                    break;
            }
        }

        RectTransform curRoomRect = rooms[PlayerTestScript.curRoom].rectTransform;
        Vector2 curRoomAnchorPos = curRoomRect.anchoredPosition;
        float xPos = curRoomAnchorPos.x + (playerTransform.position.x - PlayerTestScript.curRoomCenterPoint.x);
        float yPos = curRoomAnchorPos.y + (playerTransform.position.y - PlayerTestScript.curRoomCenterPoint.y);
        playerIcon.anchoredPosition = new Vector2(xPos, yPos);
    }
}
