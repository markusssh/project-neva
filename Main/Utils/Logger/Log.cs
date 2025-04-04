using System;
using Godot;
using ProjectNeva.Main.NetworkingArchitecture;

namespace ProjectNeva.Main.Utils.Logger;

public class Log
{

    public enum PrefixType
    {
        Time,
        UId,
    }
    
    private string _log;

    public static Log Create()
    {
        return new Log();
    }

    public Log Prefix(PrefixType prefixType)
    {
        string log = null;
        switch (prefixType)
        {
            case PrefixType.Time: 
                log = Bracketize($"{DateTime.Now:HH:mm:ss.fff}");
                break;
            case PrefixType.UId:
                var id = Networking.Instance.Multiplayer.MultiplayerPeer.GetUniqueId();
                log = id.Equals(Networking.GameServerPeerId) ? "SERVER" : id.ToString();
                var color = new Color((uint)id)
                {
                    A = 1
                };
                var bgColor = new Color(1 - color.R, 1 - color.G, 1 - color.B);
                log = Outlinize(ColorizeBg(Colorize(Bracketize(log), color), bgColor));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(prefixType), prefixType, null);
        }
        _log += log;
        return this;
    }

    public Log Body(string body)
    {
        _log += $" {body}";
        return this;
    }

    private static string Bracketize(string _) {return $"[{_}]";}

    private static string Colorize(string _, Color color)
    {
        return $"[color={color.ToHtml()}]{_}[/color]";
    }
    
    private static string ColorizeBg(string _, Color color)
    {
        return $"[bgcolor={color.ToHtml()}]{_}[/bgcolor]";
    }

    private static string Outlinize(string _)
    {
        return $"[outline_size=1][outline_color=black]{_}[/outline_color][/outline_size]";
    }

    public void Out()
    {
        GD.PrintRich(_log);
    }


}