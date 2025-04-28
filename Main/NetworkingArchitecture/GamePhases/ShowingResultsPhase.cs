using System.Collections.Generic;
using System.Linq;
using Godot;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;
using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class ShowingResultsPhase : ClosedGamePhase
{
    private const int MinPlayersToRestart = 3;

    public ShowingResultsPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
        _readiness = Lobby.Players.Keys.ToDictionary(key => key, _ => false);
        _timer.OneShot = true;
        _timer.Timeout += OnTimerTimeout;
    }

    private readonly Dictionary<long, bool> _readiness;
    private bool MinimalAmountIsReady => _readiness.Count(kv => kv.Value) >= MinPlayersToRestart;
    private bool EveryoneReady => _readiness.Count >= MinPlayersToRestart && _readiness.All(kv => kv.Value);
    private readonly Timer _timer = new();
    
    public override void Enter()
    {
        base.Enter();
        Lobby.PlayerReplayStatusChanged += OnPlayerReplayStatusChanged;
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Results scene started.");
        MultiplayerController.Instance.AddChild(_timer);
    }

    private void OnPlayerReplayStatusChanged(long playerId, bool ready)
    {
        _readiness[playerId] = ready;
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Voting for replay: " +
                          $"{_readiness.Count(kv => kv.Value)}/{Lobby.Players.Count}.");
        Update();
    }

    private void OnTimerTimeout()
    {
        LobbyManager.TransitionTo(LobbyState.LoadingDrawing);
    }

    protected override void HandlePlayerDisconnect(long playerId)
    {
        base.HandlePlayerDisconnect(playerId);
        _readiness.Remove(playerId);
        Update();
    }

    private void Update()
    {
        if (!MinimalAmountIsReady)
        {
            if (_timer.TimeLeft > 0)
            {
                _timer.Stop();
            }
            
            MultiplayerController.Instance.Server_BroadcastLobby(
                Lobby,
                MultiplayerController.MethodName.Client_HandleReplayAwaitStatusChange,
                false,
                _readiness.Count(kv => kv.Value),
                Lobby.Players.Count);

            return;
        }

        if (EveryoneReady)
        {
            if (_timer.TimeLeft > 0)
            {
                _timer.Stop();
            }
            Logger.LogNetwork($"Lobby {Lobby.LobbyId}. Players are ready for replay.");
            OnTimerTimeout();
        }
        else
        {
            if (_timer.TimeLeft <= 0)
            {
                _timer.Start(Lobby.ReplayAwaitTime);
            }
            MultiplayerController.Instance.Server_BroadcastLobby(
                Lobby,
                MultiplayerController.MethodName.Client_HandleReplayAwaitStatusChange,
                true,
                _readiness.Count(kv => kv.Value),
                Lobby.Players.Count);
        }
    }

    public override void Exit()
    {
        base.Exit();
        Lobby.PlayerReplayStatusChanged -= OnPlayerReplayStatusChanged;
        _timer.Timeout -= OnTimerTimeout;
        _timer.QueueFree();

        foreach (var playerId in _readiness.Keys.Where(playerId => !_readiness[playerId]))
        {
            Networking.Instance.Multiplayer.DisconnectPeer((int) playerId);
        }

        Lobby.Topic = RoundTopic.GetNewTopic;
        // TODO: костыль из-за того, что не успевает перед этим отключить пиры. исправить функцией ручного отключения
        foreach (var playerId in _readiness.Keys.Where(playerId => _readiness[playerId]))
        {
            MultiplayerController.Instance.Server_SendLobbySettings(playerId, Lobby);
        }
    }
}