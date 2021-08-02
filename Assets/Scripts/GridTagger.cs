using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GridTagger : MonoBehaviour
{
    public GridGraph grid;
    public string graphName;

    private void Start()
    {
        grid = AstarData.active.data.gridGraph;

        foreach (GridNode node in grid.nodes)
        {
            //connected to node directly below it
            if(grid.HasNodeConnection(node, 0))
            {
                node.Tag = 1;
                node.Penalty = 5000;
            }
        }
    }
}
