using System;
using ZSerializer;

[Serializable, SerializeGlobalData(GlobalDataType.PerSaveFile)]
public partial class Settings
{
    //  Add serializable variables to this object to be able to serialize and access them.
}
