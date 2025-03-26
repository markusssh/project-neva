using System;
using System.Collections.Generic;

namespace ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

public class Lobby
{
    public Lobby(string lobbyId)
    {
        LobbyId = lobbyId;
    }

    public event Action<long, AuthResponseDto> PlayerConnected;
    public event Action<long> PlayerDisconnected;
    public event Action<long> PlayerLoadedDrawingScene;
    public event Action<long, byte[]> PlayerSentFinalImage;
    public event Action<long, bool> PlayerDrawingStateChanged;
    public string LobbyId { get; set; }
    public Dictionary<long, Player> Players { get; set; } = new();
    public int LobbySize { get; set; } = 3;
    public string Topic { get; set; } = RoundTopic.Topics[new Random().Next(RoundTopic.Topics.Length - 1)];
    public float DrawingTimeSec { get; set; } = 10.0f;
    public int PlayersFinishedDrawingCounter = 0;
    public int ReceivedFinalDrawingCounter = 0;

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

    public void OnPlayerSentFinalImage(long playerId, byte[] imageData)
    {
        PlayerSentFinalImage?.Invoke(playerId, imageData);
    }

    public void OnPlayerDrawingStateChanged(long playerId, bool drawingOn)
    {
        PlayerDrawingStateChanged?.Invoke(playerId, drawingOn);
    }
}