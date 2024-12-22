using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectNeva.Main.NetworkingArchitecture;

public class Lobby
{
    private const int LobbyDefaultSize = 2;
    private const int LobbyDefaultRounds = 1;
    private const int LobbyDefaultRoundLength = 30;
    
    public enum LobbyState
    {
        WaitingPlayers,
        Playing
    }

    public Lobby(string lobbyId)
    {
        LobbyId = lobbyId;
    }

    public string LobbyId { get; set; }

    public int LobbySize { get; set; } = LobbyDefaultSize;
    
    public int MaxRounds { get; set; } = LobbyDefaultRounds;

    public int RoundLength { get; set; } = LobbyDefaultRoundLength;

    public Dictionary<long, Player> Players { get; set; } = new();
    
    public LobbyState State = LobbyState.WaitingPlayers;

    public Player SetRandomDrawer()
    {
        if (Players.Count == 0) throw new InvalidOperationException("No peer to choose random drawer.");
        Random random = new();
        var randomIndex = random.Next(0, Players.Count);
        var chosen = Players.ElementAt(randomIndex).Value;
        chosen.IsDrawer = true;
        return chosen;
    }
}