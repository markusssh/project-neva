using Godot;

namespace ProjectNeva.Main.Networking;

public partial class Peer : GodotObject
{
    public Peer()
    {
    }

    public Peer(long playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }
    
    public long PlayerId { get; set; }

    public string PlayerName { get; set; }
    public bool WinState { get; set; } = false;
    public int Score { get; set; } = 0;
    public bool IsDrawer { get; set; } = false;
}

public struct PlayerGameAction
{
    public bool WinState { get; set; }
    public int Score { get; set; }
    
}