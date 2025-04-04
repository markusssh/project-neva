namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

public abstract class GamePhase
{
    protected readonly LobbyManager LobbyManager;
    protected Lobby Lobby => LobbyManager.Lobby;

    protected GamePhase(LobbyManager lobbyManager)
    {
        LobbyManager = lobbyManager;
    }

    public virtual void Enter()
    {
        Lobby.PlayerConnected += HandlePlayerConnect;
        Lobby.PlayerDisconnected += HandlePlayerDisconnect;
    }

    public virtual void Exit()
    {
        Lobby.PlayerConnected -= HandlePlayerConnect;
        Lobby.PlayerDisconnected -= HandlePlayerDisconnect;
    }

    protected abstract void HandlePlayerConnect(long playerId, AuthResponseDto authData);

    protected virtual void HandlePlayerDisconnect(long playerId)
    {
        Lobby.Players.Remove(playerId);
        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_ClearLeavingPlayer,
            playerId);
    }
}