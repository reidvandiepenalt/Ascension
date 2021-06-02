using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsGridManager : MonoBehaviour
{
    public Transform topRow, bottomRow;
    int count = 0;

    /// <summary>
    /// Instantiates the prefab as the child of the correct row
    /// </summary>
    /// <returns>The instance of the game object</returns>
    public GameObject AddIcon(GameObject prefab)
    {
        GameObject instance;

        if(count % 2 == 0)
        {
            instance = Instantiate(prefab, bottomRow);
        }
        else
        {
            instance = Instantiate(prefab, topRow);
        }
        count++;
        return instance;
    }
}
