using System;
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
    
    public long PlayerId { get; set; }

    public string PlayerName { get; set; }
    public Image FinalImage { get; set; }
}