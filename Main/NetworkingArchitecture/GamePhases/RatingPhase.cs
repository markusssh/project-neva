using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class RatingPhase : ClosedGamePhase
{
    private const int MinScore = 2;
    private const int MaxScore = 10;

    private static int AvgScore => (MaxScore - MinScore) / 2;
    private static int ClampedScore(int score) => Math.Clamp(score, MinScore, MaxScore);
    
    public RatingPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
        Lobby.NewScoreReceived += OnNewScore;
        
        _ratingMatrix = new Dictionary<long, Dictionary<long, int>>();
        foreach (var playerId in Lobby.Players.Keys)
        {
            _ratingMatrix[playerId] = Lobby.Players.Keys
                .Where(id => id != playerId)
                .ToDictionary(id => id, _ => AvgScore);
        }
    }
    
    private readonly Dictionary<long, Dictionary<long, int>> _ratingMatrix;

    public void OnNewScore(long fromPlayerId, long toPlayerId, int score)
    {
        if (!_ratingMatrix.TryGetValue(fromPlayerId, out var value)) return;
        if (!value.ContainsKey(toPlayerId)) return;
        
        _ratingMatrix[fromPlayerId][toPlayerId] = ClampedScore(score);
    }

    public override void Exit()
    {
        base.Exit();
        Lobby.NewScoreReceived -= OnNewScore;
    }
}