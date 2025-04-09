using System.Linq;
using Godot;

namespace ProjectNeva.Main.Utils.Logger;

public partial class Logger : Node
{
    public static readonly bool ToEditor = !OS.GetCmdlineArgs().Contains("--ide");

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