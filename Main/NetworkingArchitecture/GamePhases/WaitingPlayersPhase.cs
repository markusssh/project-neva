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
    }

    protected override void HandlePlayerConnect(long playerId, JwtValidationResult authData)
    {
        base.HandlePlayerConnect(playerId, authData);
        if (Lobby.Players.Count == Lobby.LobbySize) LobbyManager.TransitionTo(LobbyState.LoadingDrawing);
    }
}