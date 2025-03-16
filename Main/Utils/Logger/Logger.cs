using Godot;

namespace ProjectNeva.Main.Utils.Logger;

[GlobalClass]
public partial class Logger : GodotObject
{
    public static void LogNetwork(string message)
    {
        Log.Create()
            .Prefix(Log.PrefixType.Time)
            .Prefix(Log.PrefixType.UId)
            .Body(message)
            .Out();
    }
    
    
    public static void LogMessage(string message)
    {
        Log.Create()
            .Prefix(Log.PrefixType.Time)
            .Body(message)
            .Out();
    }
}