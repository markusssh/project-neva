using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class Player : RefCounted
{
    public long PlayerId;
    public string PlayerName;
    public Image FinalImage;
    
    public Player()
    {
    }

    public Player(long playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }
}