using System;
using System.Collections.Generic;

namespace ProjectNeva.Main.NetworkingArchitecture;

public class Lobby
{
    public Lobby(
        string lobbyId,
        long creatorId,
        int playTime,
        int maxPlayers)
    {
        LobbyId = lobbyId;
        CreatorId = creatorId;
        PlayTime = playTime;
        LobbySize = maxPlayers;
    }

    public event Action<long, JwtValidationResult> PlayerConnected;
    public event Action<long> PlayerDisconnected;
    public event Action<long> PlayerLoadedNewScene;
    public event Action<long, byte[]> PlayerSentFinalImage;
    public event Action<long, bool> PlayerDrawingStateChanged;
    public event Action<long, long, int> NewScoreReceived;
    public event Action<long, bool> PlayerReplayStatusChanged;
    
    // TODO: ALERT
    public event Action GameStarted;
    
    public string LobbyId { get; set; }
    public long CreatorId { get; set; }
    public Dictionary<long, Player> Players { get; set; } = new();
    public Godot.Collections.Dictionary<long, byte[]> Images { get; set; } = new();
    public int LobbySize { get; set; }
    public string Topic { get; set; } = RoundTopic.GetNewTopic;
    public float PlayTime { get; set; }
    public float RateTime { get; } = 5;
    public float ReplayAwaitTime { get; } = 5;
    
    public void OnPlayerConnected(long playerId, JwtValidationResult authData)
    {
        PlayerConnected?.Invoke(playerId, authData);
    }

    public void OnPlayerDisconnected(long playerId)
    {
        PlayerDisconnected?.Invoke(playerId);
    }

    public void OnPlayerLoadedNewScene(long playerId)
    {
        PlayerLoadedNewScene?.Invoke(playerId);
    }

    public void OnPlayerSentFinalImage(long playerId, byte[] imageData)
    {
        PlayerSentFinalImage?.Invoke(playerId, imageData);
    }

    public void OnPlayerDrawingStateChanged(long playerId, bool drawingOn)
    {
        PlayerDrawingStateChanged?.Invoke(playerId, drawingOn);
    }

    public void OnNewScoreReceived(long fromPlayerId, long toPlayerId, int score)
    {
        NewScoreReceived?.Invoke(fromPlayerId, toPlayerId, score);
    }

    public void OnPlayerReplayStatusChanged(long playerId, bool ready)
    {
        PlayerReplayStatusChanged?.Invoke(playerId, ready);
    }

    public void OnGameStartRequested()
    {
        GameStarted?.Invoke();
    }
}