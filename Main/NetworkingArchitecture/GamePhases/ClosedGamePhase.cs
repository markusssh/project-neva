namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public abstract class ClosedGamePhase : GamePhase
{
    protected ClosedGamePhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
    }

    protected override void HandlePlayerConnect(long playerId, AuthResponseDto authData)
    {
        //TODO: add user notification
        //TODO: add ability to reconnect
        Networking.Instance.Multiplayer.DisconnectPeer((int)playerId);
    }
}