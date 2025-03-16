using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectNeva.Main.NetworkingArchitecture;

public class Lobby
{
    private const int LobbyDefaultSize = 3;

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
    public Dictionary<long, Player> Players { get; set; } = new();
    public int PlayersFinishedDrawingCounter = 0;
    public int ReceivedFinalDrawingCounter = 0;
    public LobbyState State = LobbyState.WaitingPlayers;

    public int LobbySize { get; set; } = LobbyDefaultSize;
    public string Topic { get; set; } = RoundTopic.Topics[new Random().Next(RoundTopic.Topics.Length - 1)];
    
}