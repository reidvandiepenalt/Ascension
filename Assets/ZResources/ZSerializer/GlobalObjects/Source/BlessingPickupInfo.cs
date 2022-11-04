using System;
using ZSerializer;

[Serializable, SerializeGlobalData(GlobalDataType.PerSaveFile)]
public partial class BlessingPickupInfo
{
    public bool TernaryAnimaPickedUp = false;
    public bool DesertSunPickedUp = false;
    public bool LethalRecompensePickedUp = false;
    public bool IronAegisPickedUp = false;
}
