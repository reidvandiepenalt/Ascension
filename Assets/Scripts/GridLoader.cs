using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.IO;

public class GridLoader : MonoBehaviour
{
    public TextAsset graphData;

    // Start is called before the first frame update
    void Start()
    {
        AstarPath.active.data.DeserializeGraphsAdditive(graphData.bytes);
    }
}
