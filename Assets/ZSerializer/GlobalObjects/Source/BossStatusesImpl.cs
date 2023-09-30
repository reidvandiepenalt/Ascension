using System.Reflection;
using UnityEngine;
using ZSerializer;

public partial class BossStatuses : GlobalObject
{
    private static BossStatuses _instance;
    public static BossStatuses Instance => _instance ??= Get<BossStatuses>();
    
    public static void Save() => ZSerialize.SaveGlobal(Instance);
    public static void Load() => ZSerialize.LoadGlobal(Instance);
}
