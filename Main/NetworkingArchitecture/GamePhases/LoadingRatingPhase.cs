using System.Collections.Generic;
using System.Linq;
using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingRatingPhase : ClosedGamePhase
{
    public LoadingRatingPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
        _readiness = Lobby.Players.Keys.ToDictionary(key => key, _ => false);
    }

    private readonly Dictionary<long, bool> _readiness;
    private bool EveryoneReady => _readiness.Values.All(ready => ready);

    public override void Enter()
    {
        base.Enter();

        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_ReceiveFinalImages,
            Lobby.Images
        );

        Lobby.PlayerLoadedNewScene += OnPlayerLoaded;
        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_LoadRatingScene);
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
        if (EveryoneReady) LobbyManager.TransitionTo(LobbyState.LoadingDrawing);
    }

    public override void Exit()
    {
        base.Exit();
        Lobby.PlayerLoadedNewScene -= OnPlayerLoaded;
    }
}