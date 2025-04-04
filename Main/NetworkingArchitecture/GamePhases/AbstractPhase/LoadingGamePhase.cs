using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

public abstract class LoadingGamePhase : ClosedGamePhase
{
    private readonly StringName _loadingMethod;

    protected LoadingGamePhase(LobbyManager lobbyManager, StringName loadingMethod) : base(lobbyManager)
    {
        _loadingMethod = loadingMethod;
        _readiness = Lobby.Players.Keys.ToDictionary(key => key, _ => false);
    }

    private readonly Dictionary<long, bool> _readiness;
    private bool EveryoneReady => _readiness.Values.All(ready => ready);

    public override void Enter()
    {
        base.Enter();

        Lobby.PlayerLoadedNewScene += OnPlayerLoaded;
        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            _loadingMethod);
    }

    private void OnPlayerLoaded(long playerId)
    {
        _readiness[playerId] = true;
        Update();
    }

    protected override void HandlePlayerDisconnect(long playerId)
    {
        base.HandlePlayerDisconnect(playerId);
        _readiness.Remove(playerId);
        Update();
    }

    private void Update()
    {
        if (EveryoneReady) OnEveryoneReady();
    }

    protected abstract void OnEveryoneReady();

    public override void Exit()
    {
        base.Exit();
        Lobby.PlayerLoadedNewScene -= OnPlayerLoaded;
    }
}