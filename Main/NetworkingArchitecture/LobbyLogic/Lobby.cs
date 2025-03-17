using System;
using System.Collections.Generic;

namespace ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

public class Lobby
{
    private const int LobbyDefaultSize = 3;

    public Lobby(string lobbyId)
    {
        LobbyId = lobbyId;
    }
    
    public event Action<long, AuthResponseDto> PlayerConnected;
    public event Action<long> PlayerDisconnected;
    public event Action<long> PlayerLoadedDrawingScene;

    public void OnPlayerConnected(long playerId, AuthResponseDto authData)
    {
        PlayerConnected?.Invoke(playerId, authData);
    }

    public void OnPlayerDisconnected(long playerId)
    {
        PlayerDisconnected?.Invoke(playerId);
    }

    public void OnPlayerLoadedDrawingScene(long playerId)
    {
        PlayerLoadedDrawingScene?.Invoke(playerId);
    }

    public string LobbyId { get; set; }
    public Dictionary<long, Player> Players { get; set; } = new();
    public int PlayersFinishedDrawingCounter = 0;
    public int ReceivedFinalDrawingCounter = 0;

    public int LobbySize { get; set; } = LobbyDefaultSize;
    public string Topic { get; set; } = RoundTopic.Topics[new Random().Next(RoundTopic.Topics.Length - 1)];
}