using Godot;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;
using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class WaitingPlayersPhase : OpenGamePhase
{
    public WaitingPlayersPhase(LobbyManager lobbyManager) : base(lobbyManager) {}

    public override void Enter()
    {
        base.Enter();
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Waiting for players.");
        Lobby.GameStarted += OnGameStarted;
    }

    protected override void HandlePlayerConnect(long playerId, JwtValidationResult authData)
    {
        base.HandlePlayerConnect(playerId, authData);
        
        // dev
        if (OS.HasFeature("instant_start"))
        {
            if (Lobby.Players.Count == Lobby.LobbySize) LobbyManager.TransitionTo(LobbyState.LoadingDrawing);
        }
    }

    private void OnGameStarted()
    {
        LobbyManager.TransitionTo(LobbyState.LoadingDrawing);
    }
    
}