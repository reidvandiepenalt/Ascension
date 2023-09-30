using System.Reflection;
using UnityEngine;
using ZSerializer;

public partial class BlessingPickupInfo : GlobalObject
{
    private static BlessingPickupInfo _instance;
    public static BlessingPickupInfo Instance => _instance ??= Get<BlessingPickupInfo>();
    
    public static void Save() => ZSerialize.SaveGlobal(Instance);
    public static void Load() => ZSerialize.LoadGlobal(Instance);
}
