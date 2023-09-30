using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class MapMenu : MonoBehaviour, ICanvas
{
    [SerializeField] List<GameObject> childObjects;

    public void Closing()
    {
        
    }

    public void UpdatedSelection(GameObject newSelection, GameObject oldSelection)
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdatePanels();
    }

    public void UpdatePanels()
    {
        if (PlayerInfo.Instance.mapUnlocked[TitleLoadManager.SAVE_SLOT])
        {
            foreach(GameObject child in childObjects)
            {
                child.SetActive(true);
            }
        }
    }
}
