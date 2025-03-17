using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public abstract class GamePhase
{
    protected Lobby Lobby;

    public GamePhase(Lobby lobby)
    {
        Lobby = lobby;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void HandlePlayerDisconnect(long playerId);
}

public interface IConnectable
{
    void HandlePlayerConnect(long playerId);
}

public interface IUpdateable
{
    void Update(float delta);
}