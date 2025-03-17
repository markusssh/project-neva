using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public abstract class GamePhase
{
    protected readonly Lobby Lobby;

    protected GamePhase(Lobby lobby)
    {
        Lobby = lobby;
    }

    public void Enter()
    {
        Lobby.PlayerDisconnected += HandlePlayerDisconnect;
    }

    public void Exit()
    {
        Lobby.PlayerDisconnected -= HandlePlayerDisconnect;
    }

    protected abstract void HandlePlayerDisconnect(long playerId);
}