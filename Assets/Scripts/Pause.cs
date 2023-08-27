using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause
{
    public static bool IsPaused { get { return (pauseState != PauseState.Playing); } }
    public static PauseState pauseState = PauseState.Playing;

    public enum PauseState
    {
        Escape,
        Inventory,
        Playing,
        Dialogue,
        Pickup
    }
}
