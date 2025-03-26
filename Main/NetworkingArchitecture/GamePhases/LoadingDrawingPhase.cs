using System.Collections.Generic;
using System.Linq;
using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingDrawingPhase : ClosedGamePhase
{
    public LoadingDrawingPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
        _readiness = Lobby.Players.Keys.ToDictionary(key => key, _ => false);
    }
    
    private readonly Dictionary<long, bool> _readiness;
    private bool EveryoneReady => _readiness.Values.All(ready => ready);

    public override void Enter()
    {
        base.Enter();
        Lobby.PlayerLoadedDrawingScene += OnPlayerLoaded;
        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_LoadDrawingScene);
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

    private void Update() {
        if (EveryoneReady) LobbyManager.TransitionTo(LobbyState.Drawing);
    }

    public override void Exit()
    {
        base.Exit();
        Lobby.PlayerLoadedDrawingScene -= OnPlayerLoaded;
    }
}