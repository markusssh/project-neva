using System;
using Godot;
using ProjectNeva.Main.NetworkingArchitecture;

namespace ProjectNeva.Main.Utils.Logger;

public class Log
{
    public enum PrefixType
    {
        Time,
        UId
    }

    private class LogPartBuilder
    {
        private string Styling { get; set; }
        private string Text { get; set; }
        private string StylingClose { get; set; }

        public LogPartBuilder Message(string message)
        {
            Text = message;
            return this;
        }

        public LogPartBuilder Brackets()
        {
            Styling = "[" + Styling;
            StylingClose += "]";
            return this;
        }

        public LogPartBuilder Color(Color color)
        {
            Styling = $"[color={color.ToHtml()}]" + Styling;
            StylingClose += "[/color]";
            return this;
        }

        public LogPartBuilder BackgroundColor(Color color)
        {
            Styling = $"[bgcolor={color.ToHtml()}]" + Styling;
            StylingClose += "[/bgcolor]";
            return this;
        }

        public LogPartBuilder Outline()
        {
            Styling = "[outline_size=1][outline_color=black]" + Styling;
            StylingClose += "[/outline_color][/outline_size]";
            return this;
        }

        public string Build()
        {
            return Styling + Text + StylingClose;
        }
    }

    private string _log;

    public static Log Create()
    {
        return new Log();
    }

    public Log Prefix(PrefixType prefixType)
    {
        var prefixBuilder = new LogPartBuilder();
        switch (prefixType)
        {
            case PrefixType.Time:
                prefixBuilder
                    .Message($"{DateTime.Now:HH:mm:ss.fff}")
                    .Brackets();
                break;
            case PrefixType.UId:
                var id = Networking.Instance.Multiplayer.MultiplayerPeer.GetUniqueId();
                // Если игрок уже отключился, взять последний ClientId
                if (Networking.Instance.IsClient && id == Networking.GameServerPeerId) id = (int)Networking.ClientId;

                prefixBuilder
                    .Message(id == Networking.GameServerPeerId ? "SERVER" : id.ToString())
                    .Brackets();

                if (Logger.ToEditor)
                {
                    var color = new Color((uint)id)
                    {
                        A = 1
                    };
                    var bgColor = new Color(1 - color.R, 1 - color.G, 1 - color.B);
                    prefixBuilder
                        .Color(color)
                        .BackgroundColor(bgColor)
                        .Outline();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(prefixType), prefixType, null);
        }

        _log += prefixBuilder.Build();
        return this;
    }

    public Log Body(string body)
    {
        _log += $" {body}";
        return this;
    }

    public void Out()
    {
        GD.PrintRich(_log);
    }
}