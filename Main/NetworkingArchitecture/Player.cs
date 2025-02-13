using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class Player : RefCounted
{
    public Player()
    {
    }

    public Player(long playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }

    public enum PlayerState
    {
        Loading,
        Playing
    }
    
    public long PlayerId { get; set; }

    public string PlayerName { get; set; }
    public bool WinState { get; set; } = false;
    public int Score { get; set; } = 0;
    public PlayerState State { get; set; } = PlayerState.Loading;
}

public struct PlayerGameAction
{
    public bool WinState { get; set; }
    public int Score { get; set; }
    
}