using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public abstract class ConnectableGamePhase : GamePhase
{
    protected ConnectableGamePhase(Lobby lobby) : base(lobby) {}

    public override void Enter()
    {
        base.Enter();
        Lobby.PlayerConnected += HandlePlayerConnect;
    }

    public override void Exit()
    {
        base.Exit();
        Lobby.PlayerConnected -= HandlePlayerConnect;
    }

    protected abstract void HandlePlayerConnect(AuthResponseDto authData);
}