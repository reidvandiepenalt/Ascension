using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.IO;

public class GridTagger : MonoBehaviour
{
    public GridGraph grid;
    public string fileName;

    private void Start()
    {
        grid = AstarData.active.data.gridGraph;

        foreach (GridNode node in grid.nodes)
        {
            //connected to node directly below it
            if(grid.HasNodeConnection(node, 0) && !(0 == node.ZCoordinateInGrid))
            {
                node.Tag = 1;
                node.Penalty = 5000;
            }
        }

        /*byte[] bytes = AstarPath.active.data.SerializeGraphs();

        File.WriteAllBytes("Assets/Graphs/" + fileName, bytes);*/
    }
}
