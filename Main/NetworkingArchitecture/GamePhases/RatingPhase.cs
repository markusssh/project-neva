using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;
using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class RatingPhase : ClosedGamePhase
{
    private const float RatingSec = 4.0f;
    private const int MinScore = 2;
    private const int MaxScore = 10;

    private static int AvgScore => (MaxScore - MinScore) / 2;
    private static int ClampedScore(int score) => Math.Clamp(score, MinScore, MaxScore);

    private readonly Dictionary<long, Dictionary<long, int>> _ratingMatrix;
    private readonly Timer _timer = new Timer();
    
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

    private void OnNewScore(long fromPlayerId, long toPlayerId, int score)
    {
        if (!_ratingMatrix.TryGetValue(fromPlayerId, out var value)) return;
        if (!value.ContainsKey(toPlayerId)) return;

        _ratingMatrix[fromPlayerId][toPlayerId] = ClampedScore(score);
    }

    public override void Enter()
    {
        base.Enter();

        ChangePlayerToRate();
        MultiplayerController.Instance.AddChild(_timer);
        _timer.Start(RatingSec);
        _timer.Timeout += OnTimerTimeout;
        
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Rating scene started.");
    }

    private void OnTimerTimeout()
    {
        if (ChangePlayerToRate()) return;
        _timer.Timeout -= OnTimerTimeout;
        _timer.QueueFree();
        LobbyManager.TransitionTo(LobbyState.LoadingResults);
    }

    private bool ChangePlayerToRate()
    {
        var imgPlayers = Lobby.Images.Keys;
        if (!imgPlayers.Any()) return false;
        var newId = imgPlayers.First();
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Changing rating to {newId}");
        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_ShootRatingNextPlayer,
            newId);
        Lobby.Images.Remove(newId);
        return true;
    }

    public override void Exit()
    {
        var scores = Lobby.Players.Keys.ToDictionary(
            playerId => playerId,
            playerId => _ratingMatrix.Values.Sum(
                inner => inner.TryGetValue(playerId, out var score) ? score : 0)
        );

        var godotScores = new Godot.Collections.Dictionary();
        foreach (var kvp in scores)
        {
            godotScores[kvp.Key] = kvp.Value;
        }

        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_ReceiveScores,
            godotScores);

        base.Exit();
        Lobby.NewScoreReceived -= OnNewScore;
    }
}