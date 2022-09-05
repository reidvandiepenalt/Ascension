using System;
using ZSerializer;

[Serializable, SerializeGlobalData(GlobalDataType.PerSaveFile)]
public partial class BossStatuses
{
    //  Add serializable variables to this object to be able to serialize and access them.
    public bool bennuKilled = false;
    public bool bastKilled = false;
    public bool horusKilled = false;
}
