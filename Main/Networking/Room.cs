using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectNeva.Main.Networking;

public class Room
{
    private const int RoomDefaultSize = 2;
    private const int RoomDefaultRounds = 1;
    
    public enum RoomState
    {
        WaitingPlayers,
        Playing
    }

    public Room(string roomId)
    {
        RoomId = roomId;
    }

    public string RoomId { get; set; }

    public int RoomSize { get; set; } = RoomDefaultSize;
    
    public int MaxRounds { get; set; } = RoomDefaultRounds;

    public Dictionary<long, Peer> Peers { get; set; } = new();
    
    public RoomState State = RoomState.WaitingPlayers;

    public Peer SetRandomDrawer()
    {
        if (Peers.Count == 0) throw new InvalidOperationException("No peer to choose random drawer.");
        Random random = new();
        var randomIndex = random.Next(0, Peers.Count);
        var chosen = Peers.ElementAt(randomIndex).Value;
        chosen.IsDrawer = true;
        return chosen;
    }
}