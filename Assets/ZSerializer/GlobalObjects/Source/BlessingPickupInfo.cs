using System;
using ZSerializer;

[Serializable, SerializeGlobalData(GlobalDataType.PerSaveFile)]
public partial class BlessingPickupInfo
{
    public bool[] TernaryAnimaPickedUp     = { false, false, false };
    public bool[] DesertSunPickedUp        = { false, false, false };
    public bool[] LethalRecompensePickedUp = { false, false, false };
    public bool[] IronAegisPickedUp        = { false, false, false };

    public void Setup()
    {
        TernaryAnimaPickedUp = new bool[3];
        Array.Fill(TernaryAnimaPickedUp, false);
        DesertSunPickedUp = new bool[3];
        Array.Fill(DesertSunPickedUp, false);
        LethalRecompensePickedUp = new bool[3];
        Array.Fill(LethalRecompensePickedUp, false);
        IronAegisPickedUp = new bool[3];
        Array.Fill(IronAegisPickedUp, false);
    }

    public void DeleteSlot(int slot)
    {
        TernaryAnimaPickedUp[slot] = false;
        DesertSunPickedUp[slot] = false;
        LethalRecompensePickedUp[slot] = false;
        IronAegisPickedUp[slot] = false;
    }
}
