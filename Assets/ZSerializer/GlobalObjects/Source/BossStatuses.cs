using System;
using ZSerializer;

[Serializable, SerializeGlobalData(GlobalDataType.PerSaveFile)]
public partial class BossStatuses
{
    //  Add serializable variables to this object to be able to serialize and access them.
    public bool[] bennuKilled = { false, false, false };
    public bool[] bastKilled = { false, false, false };
    public bool[] horusKilled = { false, false, false };


    public void Setup()
    {
        bennuKilled = new bool[3];
        bastKilled = new bool[3];
        horusKilled = new bool[3];
        Array.Fill(bennuKilled, false);
        Array.Fill(bastKilled, false);
        Array.Fill(horusKilled, false);
    }

    public void DeleteSlot(int slot)
    {
        bennuKilled[slot] = false;
        bastKilled[slot] = false;
        horusKilled[slot] = false;
    }
}
