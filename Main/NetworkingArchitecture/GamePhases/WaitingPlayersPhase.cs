using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class WaitingPlayersPhase : OpenGamePhase
{
    public WaitingPlayersPhase(LobbyManager lobbyManager) : base(lobbyManager) {}

    protected override void HandlePlayerConnect(long playerId, JwtValidationResult authData)
    {
        base.HandlePlayerConnect(playerId, authData);
        if (Lobby.Players.Count == Lobby.LobbySize) LobbyManager.TransitionTo(LobbyState.LoadingDrawing);
    }
}