using System.Reflection;
using UnityEngine;
using ZSerializer;

public partial class PlayerInfo : GlobalObject
{
    private static PlayerInfo _instance;
    public static PlayerInfo Instance => _instance ??= Get<PlayerInfo>();
    
    public static void Save() => ZSerialize.SaveGlobal(Instance);
    public static void Load() => ZSerialize.LoadGlobal(Instance);
}
